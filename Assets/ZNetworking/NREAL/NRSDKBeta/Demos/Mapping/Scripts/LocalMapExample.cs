using NRKernal.Persistence;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace NRKernal.NRExamples
{
    public class LocalMapExample : MonoBehaviour
    {
        private NRWorldAnchorStore m_NRWorldAnchorStore;
        private ImageTrackingAnchorTool m_ImageTrackingAnchorTool;
        public ImageTrackingAnchorTool m_ImageTrackingAnchorTool2;
        public Transform m_AnchorPanel;
        public Text debugText;
        private Transform target;

        private Dictionary<string, GameObject> m_AnchorPrefabDict = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> m_LoadedAnchorDict = new Dictionary<string, GameObject>();
        private StringBuilder m_LogStr = new StringBuilder();

        private void Start()
        {
            m_ImageTrackingAnchorTool = gameObject.GetComponent<ImageTrackingAnchorTool>();
            var anchorItems = FindObjectsOfType<AnchorItem>();
            foreach (var item in anchorItems)
            {
                item.OnAnchorItemClick += OnAnchorItemClick;
                m_AnchorPrefabDict.Add(item.key, item.gameObject);
            }
            m_AnchorPanel.gameObject.SetActive(false);

            m_ImageTrackingAnchorTool.OnAnchorLoaded += OnImageTrackingAnchorLoaded;
            m_ImageTrackingAnchorTool2.OnAnchorLoaded += OnImageTrackingAnchorLoaded;
        }

        private void OnImageTrackingAnchorLoaded(string key, NRWorldAnchor anchor)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(go.GetComponent<BoxCollider>());
            go.transform.parent = anchor.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * 0.3f;
            go.name = key;
        }

        private void Update()
        {
            if (NRInput.GetButtonDown(ControllerButton.TRIGGER))
            {
                AddAnchor();
            }
            debugText.text = m_LogStr.ToString();
        }

        // Open or close anchor panel.
        public void SwitchAnchorPanel()
        {
            m_AnchorPanel.gameObject.SetActive(!m_AnchorPanel.gameObject.activeInHierarchy);
        }

        private void OnAnchorItemClick(string key, GameObject anchorItem)
        {
            if (target != null)
            {
                DestroyImmediate(target.gameObject);
            }

            target = Instantiate(anchorItem).transform;
            target.parent = NRInput.AnchorsHelper.GetAnchor(ControllerAnchorEnum.RightModelAnchor);
            target.position = target.parent.transform.position + target.parent.forward;
            target.forward = target.parent.forward;
            Destroy(target.gameObject.GetComponent<BoxCollider>());

            this.SwitchAnchorPanel();
        }

        // Create NRWorldAnchorStore object.
        public void Load()
        {
            NRWorldAnchorStore.GetAsync(GetAnchorStoreCallBack);
        }

        private void GetAnchorStoreCallBack(NRWorldAnchorStore store)
        {
            if (store == null)
            {
                Debug.Log("store is null");
                return;
            }
            m_NRWorldAnchorStore = store;
            m_LogStr.AppendLine("Load map result: true");
            var keys = m_NRWorldAnchorStore.GetAllIds();
            if (keys != null)
            {
                foreach (var key in m_NRWorldAnchorStore.GetAllIds())
                {
                    m_LogStr.AppendLine("Get a anchor from NRWorldAnchorStore  key: " + key);
                    GameObject prefab;
                    if (m_AnchorPrefabDict.TryGetValue(key, out prefab))
                    {
                        var go = Instantiate(prefab);
                        m_NRWorldAnchorStore.Load(key, go);
                        m_LoadedAnchorDict[key] = go;
                    }
                }
            }
        }

        // Save anchors your add.
        public void Save()
        {
            if (m_NRWorldAnchorStore == null)
            {
                return;
            }
            bool result = m_NRWorldAnchorStore.Save();
            m_LogStr.AppendLine("Save map result:" + result);
        }

        // Clear all anchors.
        public void Clear()
        {
            if (m_NRWorldAnchorStore == null)
            {
                return;
            }
            m_NRWorldAnchorStore.Clear();
            m_LogStr.AppendLine("Clear map anchor");
        }

        // Add a new anchor.
        public void AddAnchor()
        {
            if (m_NRWorldAnchorStore == null || target == null)
            {
                return;
            }

            var anchorItem = target.GetComponent<AnchorItem>();
            if (anchorItem == null)
            {
                return;
            }
            var go = Instantiate(target.gameObject);
            go.transform.position = target.position;
            go.transform.rotation = target.rotation;
            go.SetActive(true);

            string key = go.GetComponent<AnchorItem>().key;
            bool result = m_NRWorldAnchorStore.Load(key, go);
            if (!result)
            {
                DestroyImmediate(go);
            }
            else
            {
                GameObject lastgo;
                m_LoadedAnchorDict.TryGetValue(key, out lastgo);
                if (lastgo != null)
                {
                    DestroyImmediate(lastgo);
                }
                m_LoadedAnchorDict[key] = go;
            }

            DestroyImmediate(target.gameObject);
            m_LogStr.AppendLine("Add anchor " + result);
        }
    }
}