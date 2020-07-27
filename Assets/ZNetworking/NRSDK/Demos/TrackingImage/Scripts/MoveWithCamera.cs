using UnityEngine;

namespace NRKernal.NRExamples
{
    public class MoveWithCamera : MonoBehaviour
    {
        private float originDistance;
        [SerializeField]
        private bool useRelative = true;

        private Transform _MainCamera;
        private Transform mainCamera
        {
            get
            {
                if (_MainCamera == null)
                {
                    if (Camera.main != null)
                    {
                        _MainCamera = Camera.main.transform;
                    }
                    else
                    {
                        _MainCamera = NRSessionManager.Instance.NRHMDPoseTracker.centerCamera.transform;
                    }
                }
                return _MainCamera;
            }
        }

        private void Awake()
        {
            originDistance = useRelative ? Vector3.Distance(transform.position, mainCamera.position) : 0;
        }

        void LateUpdate()
        {
            transform.position = mainCamera.transform.position + mainCamera.transform.forward * originDistance;
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}
