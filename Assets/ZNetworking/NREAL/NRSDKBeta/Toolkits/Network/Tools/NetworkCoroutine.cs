using System;
using UnityEngine;

namespace NRKernal.ObserverView.NetWork
{
    internal class NetworkCoroutine : MonoBehaviour
    {
        private event Action ApplicationQuitEvent;

        private static NetworkCoroutine _instance;

        public static NetworkCoroutine Instance
        {
            get
            {
                if (!_instance)
                {
                    GameObject socketClientObj = new GameObject("NetworkCoroutine");
                    _instance = socketClientObj.AddComponent<NetworkCoroutine>();
                    DontDestroyOnLoad(socketClientObj);
                }
                return _instance;
            }
        }

        public void SetQuitEvent(Action func)
        {
            if (ApplicationQuitEvent != null) return;
            ApplicationQuitEvent += func;
        }

        private void OnApplicationQuit()
        {
            ApplicationQuitEvent?.Invoke();
        }
    }
}