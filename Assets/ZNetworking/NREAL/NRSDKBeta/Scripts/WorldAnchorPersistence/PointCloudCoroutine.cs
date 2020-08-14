using UnityEngine;

namespace NRKernal
{
    internal class PointCloudCoroutine : MonoBehaviour
    {
        private static PointCloudCoroutine _instance;

        public static PointCloudCoroutine Instance
        {
            get
            {
                if (!_instance)
                {
                    GameObject pointCloudObj = new GameObject("PointCloudCoroutine");
                    _instance = pointCloudObj.AddComponent<PointCloudCoroutine>();
                    DontDestroyOnLoad(pointCloudObj);
                }
                return _instance;
            }
        }
    }
}
