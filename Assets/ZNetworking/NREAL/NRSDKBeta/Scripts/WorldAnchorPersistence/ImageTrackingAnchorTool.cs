using NRKernal.NRExamples;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NRKernal.Persistence
{
    public class ImageTrackingAnchorTool : MonoBehaviour
    {
        internal event Action<string, NRWorldAnchor> OnAnchorLoaded;
        public event Action<Pose> OnAnchorPoseUpdate;
        public TrackingImageVisualizerForAnchor TrackingImageVisualizerPrefab;
        public int usedImageIndex = 0;
        TrackingImageVisualizerForAnchor visualizer;

        private bool m_IsStart = true;
        public bool useMap = false;
        public string anchorKey = "nrealmarker";

        private NRWorldAnchorStore anchorStore;
        private NRWorldAnchor currentAnchor;

        void Start()
        {
            if (useMap)
            {
                this.LoadAnchor();
            }
        }

        private void LoadAnchor()
        {
            NRWorldAnchorStore.GetAsync((store) =>
            {
                Debug.Log("Get world anchor map success.");
                this.anchorStore = store;
                var keys = anchorStore.GetAllIds();
                if (keys == null)
                {
                    NRDebugger.Log("can not get any keys...");
                    return;
                }

                foreach (var item in keys)
                {
                    if (item.Equals(anchorKey))
                    {
                        GameObject go = new GameObject(anchorKey);
                        currentAnchor = anchorStore.Load(anchorKey, go);
                        Debug.Log("Find the anchor ：" + anchorKey);
                        OnAnchorLoaded?.Invoke(anchorKey, currentAnchor);
                        break;
                    }
                }
            });
        }

        private void Update()
        {
            if (!m_IsStart)
            {
                return;
            }

            List<NRTrackableImage> trackableImages = new List<NRTrackableImage>();
            NRFrame.GetTrackables<NRTrackableImage>(trackableImages, NRTrackableQueryFilter.All);
            Pose pose;
            foreach (var image in trackableImages)
            {
                if (image.GetDataBaseIndex() == usedImageIndex)
                {
                    pose = image.GetCenterPose();
                    if (visualizer == null)
                    {
                        visualizer = (TrackingImageVisualizerForAnchor)Instantiate(TrackingImageVisualizerPrefab, pose.position, pose.rotation);
                        visualizer.Image = image;
                        if (useMap)
                        {
                            visualizer.SaveButton.onClick.AddListener(SaveAnchor);
                        }
                    }
                    break;
                }
            }

            // Update pose.
            if (useMap)
            {
                if (currentAnchor != null && currentAnchor.GetTrackingState() == TrackingState.Tracking)
                {
                    pose = currentAnchor.GetCenterPose();
                    OnAnchorPoseUpdate?.Invoke(pose);
                }
            }
            else
            {
                if (visualizer != null && visualizer.Image.GetTrackingState() == TrackingState.Tracking)
                {
                    pose = visualizer.Image.GetCenterPose();
                    OnAnchorPoseUpdate?.Invoke(pose);
                }
            }
        }

        private bool saveLock = false;
        private void SaveAnchor()
        {
            if (anchorStore != null && visualizer != null && visualizer.Image.GetTrackingState() == TrackingState.Tracking)
            {
                if (saveLock)
                {
                    Invoke("Unlock", 1f);
                }
                saveLock = true;
                GameObject go = new GameObject(anchorKey);
                go.transform.position = visualizer.transform.position;
                go.transform.rotation = visualizer.transform.rotation;
                currentAnchor = anchorStore.Load(anchorKey, go);
                OnAnchorLoaded?.Invoke(anchorKey, currentAnchor);
                anchorStore.Save();

                Debug.Log("Save the anchor.");
            }
        }

        private void Unlock()
        {
            saveLock = false;
        }

        public void SwitchTrackableImage(bool isopen)
        {
            m_IsStart = isopen;
            visualizer?.gameObject.SetActive(isopen);
            //var config = NRSessionManager.Instance.NRSessionBehaviour.SessionConfig;
            //config.ImageTrackingMode = isopen ? TrackableImageFindingMode.ENABLE : TrackableImageFindingMode.DISABLE;
            //NRSessionManager.Instance.SetConfiguration(config);
        }
    }
}