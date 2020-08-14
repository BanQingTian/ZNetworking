/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal.ObserverView
{
    using NRKernal.ObserverView.NetWork;
    using System;
    using System.Collections;
    using UnityEngine;

    public class ObserverViewNetWorker
    {
        ObserverViewFrameCaptureContext m_Context;
        NetWorkClient m_NetWorkClient;

        private const float limitWaittingTime = 5f;
        private bool m_IsConnected = false;
        private bool m_IsJoninSuccess = false;
        private bool m_IsClosed = false;

        public ObserverViewNetWorker(ObserverViewFrameCaptureContext contex = null)
        {
            this.m_Context = contex;

            m_NetWorkClient = new NetWorkClient();
            m_NetWorkClient.OnDisconnect += OnDisconnect;
            m_NetWorkClient.OnConnect += OnConnected;
            m_NetWorkClient.OnJoinRoomResult += OnJoinRoomResult;
            m_NetWorkClient.OnCameraParamUpdate += OnCameraParamUpdate;
        }

        public void CheckServerAvailable(string ip, Action<bool> callback)
        {
            if (string.IsNullOrEmpty(ip))
            {
                callback?.Invoke(false);
            }
            else
            {
                NRKernalUpdater.Instance.StartCoroutine(CheckServerAvailableCoroutine(ip, callback));
            }
        }

        private IEnumerator CheckServerAvailableCoroutine(string ip, Action<bool> callback)
        {
            // Start to connect the server.
            m_NetWorkClient.Connect(ip, 6000);
            float timeLast = 0;
            while (!m_IsConnected)
            {
                if (timeLast > limitWaittingTime || m_IsClosed)
                {
                    Debug.Log("[ObserverView] Connect the server TimeOut!");
                    callback?.Invoke(false);
                    yield break;
                }
                timeLast += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            // Start to enter the room.
            m_NetWorkClient.EnterRoomRequest();

            timeLast = 0;
            while (!m_IsJoninSuccess)
            {
                if (timeLast > limitWaittingTime || m_IsClosed)
                {
                    Debug.Log("[ObserverView] Join the server TimeOut!");
                    callback?.Invoke(false);
                    yield break;
                }
                timeLast += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            callback?.Invoke(true);
        }

        #region Net msg
        private void OnConnected()
        {
            Debug.Log("OnConnected...");
            m_IsConnected = true;
        }

        private void OnDisconnect()
        {
            Debug.Log("OnDisconnect...");
            this.Close();
        }

        private void OnCameraParamUpdate(CameraParam param)
        {
            if (this.m_Context == null)
            {
                return;
            }
            this.m_Context.GetBehaviour().UpdateCameraParam(param.fov);
        }

        private void OnJoinRoomResult(bool result)
        {
            Debug.Log("OnJoinRoomResult :" + result);
            m_IsJoninSuccess = result;
            if (!result)
            {
                this.Close();
            }
        }
        #endregion

        internal void Close()
        {
            m_NetWorkClient?.Dispose();
            m_NetWorkClient = null;
        }
    }
}
