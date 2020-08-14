using NRKernal.Persistence;
using UnityEngine;

namespace NRKernal.NRExamples
{
    [RequireComponent(typeof(ImageTrackingAnchorTool))]
    public class AnchorLoader : MonoBehaviour
    {
        // This is the object which you want to load
        public GameObject AnchorPrefab;
        // This is an anchor load/add tool using image tracking.
        private ImageTrackingAnchorTool m_ImageTrackingAnchorTool;

        private void Start()
        {
            m_ImageTrackingAnchorTool = gameObject.GetComponent<ImageTrackingAnchorTool>();
            m_ImageTrackingAnchorTool.OnAnchorLoaded += OnImageTrackingAnchorLoaded;
        }

        /// <summary>
        /// After local anchor been loaded
        /// </summary>
        /// <param name="key"></param>
        /// <param name="anchor"></param>
        private void OnImageTrackingAnchorLoaded(string key, NRWorldAnchor anchor)
        {
            // Load your prefab as a child of the anchor
            var go = Instantiate(AnchorPrefab);
            go.transform.SetParent(anchor.transform);
            go.name = key;
        }
    }
}