using UnityEngine;

namespace NRKernal.NRExamples
{
    public class GestureRecognizeExample : MonoBehaviour
    {
        private NRGestureRecognizer m_GestureRecognizer;

        void Start()
        {
            StartGesture();
        }

        private void OnDestroy()
        {
            DestroyGesture();
        }

        private void StartGesture()
        {
            m_GestureRecognizer = new NRGestureRecognizer();
            m_GestureRecognizer.OnGestureUpdate += OnGestureUpdate;
            m_GestureRecognizer.StartRecognize();
        }

        private void DestroyGesture()
        {
            if (m_GestureRecognizer != null)
            {
                m_GestureRecognizer.Destroy();
                m_GestureRecognizer = null;
            }
        }

        private void OnGestureUpdate(NRGestureInfo gestureInfo)
        {
            Debug.Log(gameObject.name + " OnGestureUpdate:");
            Debug.Log(gestureInfo.ToString());
        }
    }
}
