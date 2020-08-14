/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal.Persistence
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal class NRWorldAnchorStore : IDisposable
    {
        public int anchorCount
        {
            get
            {
                return m_AnchorDict.Count;
            }
        }

        private NativeMapping m_NativeMapping { get; set; }
        private Dictionary<int, NRWorldAnchor> m_AnchorDict = new Dictionary<int, NRWorldAnchor>();
        private Dictionary<UInt64, NRWorldAnchor> m_OriginAnchorDict = new Dictionary<UInt64, NRWorldAnchor>();
        private Dictionary<string, int> m_Anchor2ObjectDict;
        private Dictionary<string, int> m_LocalPersistAnchor = new Dictionary<string, int>();

        private static NRWorldAnchorStore m_NRWorldAnchorStore;
        private const string MapFolder = "NrealMaps";
        private const string MapFile = "nreal_map.dat";
        private const string Anchor2ObjectFile = "anchor2object.json";
        private const string m_RootAnchor = "root";

        /// <summary>
        /// Gets the WorldAnchorStore instance.
        /// </summary>
        /// <param name="onCompleted"></param>
        public static void GetAsync(GetAsyncDelegate onCompleted)
        {
            NRKernalUpdater.Instance.StartCoroutine(GetWorldAnchorStore(onCompleted));
        }

        private static IEnumerator GetWorldAnchorStore(GetAsyncDelegate onCompleted)
        {
            // Wait for slam ready.
            while (NRFrame.LostTrackingReason != LostTrackingReason.NONE ||
                NRFrame.SessionStatus != SessionState.Running)
            {
                Debug.Log("Wait for slam ready...");
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (m_NRWorldAnchorStore == null)
            {
                m_NRWorldAnchorStore = new NRWorldAnchorStore();
            }

            yield return new WaitForSeconds(0.2f);
            Debug.Log("[NRWorldAnchorStore] : GetWorldAnchorStore true");
            onCompleted?.Invoke(m_NRWorldAnchorStore);
        }

        internal NRWorldAnchorStore()
        {
#if !UNITY_EDITOR
            m_NativeMapping = new NativeMapping(NRSessionManager.Instance.NativeAPI);
#endif
            EnsurePath(Path.Combine(Application.persistentDataPath, MapFolder));
            LoadWorldMap(Path.Combine(Application.persistentDataPath, MapFolder, MapFile));
            string path = Path.Combine(Application.persistentDataPath, MapFolder, Anchor2ObjectFile);
            if (!File.Exists(path))
            {
                NRDebugger.Log("[NRWorldAnchorStore] File not exit:" + path);
                m_Anchor2ObjectDict = new Dictionary<string, int>();
            }
            else
            {
                string json = File.ReadAllText(path);
                NRDebugger.Log("[NRWorldAnchorStore] Anchor2Object json:" + json);
                m_Anchor2ObjectDict = LitJson.JsonMapper.ToObject<Dictionary<string, int>>(json);
                foreach (var item in m_Anchor2ObjectDict)
                {
                    NRDebugger.LogFormat("[NRWorldAnchorStore] key:{0} value:{1}", item.Key, item.Value);
                    m_LocalPersistAnchor.Add(item.Key, item.Value);
                }
            }

            // Add a root anchor for default.
            if (!m_LocalPersistAnchor.ContainsKey(m_RootAnchor))
            {
                m_LocalPersistAnchor.Add(m_RootAnchor, 0);
            }

            NRKernalUpdater.Instance.OnUpdate -= OnUpdate;
            NRKernalUpdater.Instance.OnUpdate += OnUpdate;
        }

        private void OnUpdate()
        {
            UpdateAnchors();
            foreach (var item in m_AnchorDict)
            {
                item.Value.Update();
            }
        }

        private bool LoadWorldMap(string path)
        {
            Assert.IsTrue(File.Exists(path), "[NRWorldAnchorStore] World map File is not exit!!!");
#if !UNITY_EDITOR
            m_NativeMapping.CreateDataBase();
            return m_NativeMapping.LoadMap(path);
#else
            return true;
#endif
        }

        private bool WriteWorldMap()
        {
            string basepath = Path.Combine(Application.persistentDataPath, MapFolder);
            if (!Directory.Exists(basepath))
            {
                Directory.CreateDirectory(basepath);
            }
#if !UNITY_EDITOR
            return m_NativeMapping.SaveMap(Path.Combine(basepath, MapFile));
#else
            return true;
#endif
        }

        /// <summary>
        /// Clears all persisted NRWorldAnchors.
        /// </summary>
        public void Clear()
        {
            foreach (var item in m_OriginAnchorDict)
            {
#if !UNITY_EDITOR
                m_NativeMapping.DestroyAnchor(item.Key);
#endif
                GameObject.Destroy(item.Value.gameObject);
            }
            m_OriginAnchorDict.Clear();
            m_AnchorDict.Clear();
            m_Anchor2ObjectDict.Clear();
            m_LocalPersistAnchor.Clear();

            this.Save();
        }

        /// <summary>
        /// Deletes a persisted NRWorldAnchor from the store.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Delete(string key)
        {
            NRWorldAnchor anchor;
            if (TryGetValue(key, out anchor))
            {
#if !UNITY_EDITOR
                m_NativeMapping.DestroyAnchor(anchor.AnchorNativeHandle);
#endif
                m_AnchorDict.Remove(anchor.GetNativeSpatialAnchorPtr());
                m_OriginAnchorDict.Remove(anchor.AnchorNativeHandle);
                m_LocalPersistAnchor.Remove(key);
                m_Anchor2ObjectDict.Remove(key);
                GameObject.Destroy(anchor.gameObject);
                this.Save();
            }

            return false;
        }

        /// <summary>
        /// Cleans up the WorldAnchorStore and releases memory.
        /// </summary>
        public void Dispose()
        {
#if !UNITY_EDITOR
            m_NativeMapping.DestroyDataBase();
#endif
            m_NRWorldAnchorStore = null;

            if (GameObject.FindObjectOfType<NRKernalUpdater>() != null)
            {
                NRKernalUpdater.Instance.OnUpdate -= OnUpdate;
            }
        }

        /// <summary>
        /// Gets all of the identifiers of the currently persisted NRWorldAnchors.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllIds()
        {
            if (m_Anchor2ObjectDict == null || m_Anchor2ObjectDict.Count == 0)
            {
                return null;
            }

            string[] ids = new string[m_Anchor2ObjectDict.Count];

            int index = 0;
            foreach (var item in m_Anchor2ObjectDict)
            {
                ids[index++] = item.Key;
            }

            return ids;
        }

        /// <summary>
        /// Update all NRWorldAnchors. 
        /// </summary>
        /// <param name="anchorlist"></param>
        private void UpdateAnchors()
        {
#if !UNITY_EDITOR
            var listhandle = m_NativeMapping.CreateAnchorList();
            m_NativeMapping.UpdateAnchor(listhandle);
            var size = m_NativeMapping.GetAnchorListSize(listhandle);
            for (int i = 0; i < size; i++)
            {
                var anchorhandle = m_NativeMapping.AcquireItem(listhandle, i);
                CreateAnchor(anchorhandle);
            }
            m_NativeMapping.DestroyAnchorList(listhandle);

#else
            if (m_Anchor2ObjectDict.Count == 0)
            {
                CreateAnchor(0);
            }
            foreach (var item in m_Anchor2ObjectDict)
            {
                CreateAnchor((UInt64)item.Value);
            }
#endif
        }

        /// <summary>
        /// Loads a WorldAnchor from disk for given identifier and attaches it to the GameObject. 
        /// If the GameObject has a WorldAnchor, that WorldAnchor will be updated. 
        /// If the anchor is not found, a new anchor will be add in the position of go.
        /// If AddAnchor failed, null will be returned. The GameObject and any existing NRWorldAnchor attached to it will not be modified.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="go"></param>
        /// <returns></returns>
        public NRWorldAnchor Load(string id, GameObject go)
        {
            NRDebugger.Log("[NRWorldAnchorStore] load worldanchor:" + id);
            if (string.IsNullOrEmpty(id) || go == null)
            {
                return null;
            }

            NRWorldAnchor anchor;
            if (TryGetValue(id, out anchor))
            {
                if ((go.transform.parent == null || go.transform.parent.GetComponent<NRWorldAnchor>() == null)
                    && anchor.transform.childCount == 0)
                {
                    go.transform.parent = anchor.transform;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    return anchor;
                }
                else
                {
                    NRDebugger.Log("[NRWorldAnchorStore] find the worldanchor and delete it which id is:" + anchor.GetNativeSpatialAnchorPtr());
                    this.Delete(id);
                }
            }

            // Add a new anchor.
            anchor = AddAnchor(id, new Pose(go.transform.position, go.transform.rotation));
            if (anchor != null)
            {
#if UNITY_EDITOR
                anchor.transform.position = go.transform.position;
                anchor.transform.rotation = go.transform.rotation;
#endif
                go.transform.parent = anchor.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
            }

            return anchor;
        }

        private bool TryGetValue(string key, out NRWorldAnchor out_anchor)
        {
            NRWorldAnchor anchor;
            int anchorID;
            if (m_Anchor2ObjectDict.TryGetValue(key, out anchorID))
            {
                if (m_AnchorDict.TryGetValue(anchorID, out anchor))
                {
                    out_anchor = anchor;
                    return true;
                }
            }

            out_anchor = null;
            return false;
        }

        private NRWorldAnchor AddAnchor(string key, Pose worldPose)
        {
            NRDebugger.Log("[NRWorldAnchorStore] Add a new worldanchor");
#if !UNITY_EDITOR
            var handle = m_NativeMapping.AddAnchor(worldPose);
#else
            var handle = (UInt64)UnityEngine.Random.Range(1, 100000);
#endif
            if (handle == 0)
            {
                NRDebugger.LogError("Add anchor failed anchor handle:" + handle);
                return null;
            }
            return CreateAnchor(handle, key);
        }

        private NRWorldAnchor CreateAnchor(UInt64 handler, string anchorkey = null)
        {
            if (m_OriginAnchorDict.ContainsKey(handler))
            {
                //Debug.LogError("Already has the anchor :" + handler);
                return null;
            }
            NRDebugger.Log("[NRWorldAnchorStore] Create a new worldanchor handle:" + handler);
            var anchor = new GameObject("NRWorldAnchor").AddComponent<NRWorldAnchor>();
            // Make sure anchor would not be destroied when change the scene.
            GameObject.DontDestroyOnLoad(anchor);
            anchor.SetNativeSpatialAnchorPtr(handler, m_NativeMapping);
            int anchorID = anchor.GetNativeSpatialAnchorPtr();
            m_AnchorDict[anchorID] = anchor;
            m_OriginAnchorDict[handler] = anchor;
            if (!string.IsNullOrEmpty(anchorkey))
            {
                m_Anchor2ObjectDict[anchorkey] = anchorID;
            }

            return anchor;
        }

        /// <summary>
        /// Saves the provided NRWorldAnchor with the provided identifier. 
        /// If the identifier is already in use, the method will return false.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        private bool Save(string id, NRWorldAnchor anchor)
        {
            NRDebugger.Log("[NRWorldAnchorStore] Save a new worldanchor:" + id);
            if (m_Anchor2ObjectDict.ContainsKey(id))
            {
                return false;
            }

            try
            {
                m_Anchor2ObjectDict.Add(id, anchor.GetNativeSpatialAnchorPtr());
                string json = LitJson.JsonMapper.ToJson(m_Anchor2ObjectDict);
                string path = Path.Combine(Application.persistentDataPath, MapFolder, Anchor2ObjectFile);
                NRDebugger.Log("[NRWorldAnchorStore] Save to the path:" + path + " json:" + json);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception e)
            {
                NRDebugger.LogWarning("Write anchor to object dict exception:" + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Saves all NRWorldAnchor. 
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            if (m_Anchor2ObjectDict == null)
            {
                return false;
            }

            NRDebugger.Log("[NRWorldAnchorStore] Save all worldanchor");
            try
            {
                string path = Path.Combine(Application.persistentDataPath, MapFolder, Anchor2ObjectFile);
                if (m_Anchor2ObjectDict.Count != 0)
                {
                    string json = LitJson.JsonMapper.ToJson(m_Anchor2ObjectDict);
                    NRDebugger.Log("[NRWorldAnchorStore] Save to the path:" + path + " json:" + json);
                    File.WriteAllText(path, json);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }

                return this.WriteWorldMap();
            }
            catch (Exception e)
            {
                NRDebugger.LogWarning("Write anchor to object dict exception:" + e.ToString());
                return false;
            }
        }

        private void EnsurePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// The handler for when getting the WorldAnchorStore from GetAsync.
        /// </summary>
        /// <param name="store"></param>
        public delegate void GetAsyncDelegate(NRWorldAnchorStore store);
    }
}
