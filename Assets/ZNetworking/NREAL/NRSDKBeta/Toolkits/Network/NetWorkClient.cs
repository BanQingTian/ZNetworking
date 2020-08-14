using System;
using UnityEngine;

namespace NRKernal.ObserverView.NetWork
{
    public class NetWorkClient : IDisposable
    {
        public event Action<bool> OnJoinRoomResult;
        public event Action<CameraParam> OnCameraParamUpdate;
        public event Action OnDisconnect;
        public event Action OnConnect;

        public NetWorkClient()
        {
            NetworkSession.Register(MessageType.Connected, OnConnected);
            NetworkSession.Register(MessageType.Disconnect, OnDisConnected);
            NetworkSession.Register(MessageType.HeartBeat, HeartbeatResponse);
            NetworkSession.Register(MessageType.EnterRoom, EnterRoomResponse);
            NetworkSession.Register(MessageType.ExitRoom, ExitRoomResponse);
            NetworkSession.Register(MessageType.UpdateCameraParam, UpdateCameraParamResponse);
        }

        // Join the server's room.
        public void EnterRoomRequest()
        {
            NetworkSession.Enqueue(MessageType.EnterRoom);
        }

        public void ExitRoomRequest()
        {
            NetworkSession.Enqueue(MessageType.ExitRoom);
        }

        public void UpdateCameraParamRequest()
        {
            NetworkSession.Enqueue(MessageType.UpdateCameraParam);
        }

        public void Connect(string ip, int port)
        {
            NetworkSession.Connect(ip, port);
        }

        private void OnConnected(byte[] data)
        {
            OnConnect?.Invoke();
        }

        public void Dispose()
        {
            NetworkSession.UnRegister(MessageType.Connected, OnConnected);
            NetworkSession.UnRegister(MessageType.Disconnect, OnDisConnected);
            NetworkSession.UnRegister(MessageType.HeartBeat, HeartbeatResponse);
            NetworkSession.UnRegister(MessageType.EnterRoom, EnterRoomResponse);
            NetworkSession.UnRegister(MessageType.ExitRoom, ExitRoomResponse);
            NetworkSession.UnRegister(MessageType.UpdateCameraParam, UpdateCameraParamResponse);
            NetworkSession.Close();
        }

        private void OnDisConnected(byte[] data)
        {
            OnDisconnect?.Invoke();
        }

        #region Net msg response
        private void HeartbeatResponse(byte[] data)
        {
            NetworkSession.Received = true;
            Debug.Log("Receive a heart beat package.");
        }

        private void EnterRoomResponse(byte[] data)
        {
            EnterRoomData result = SerializerFactory.Create().Deserialize<EnterRoomData>(data);

            if (result.result)
            {
                Debug.Log("Join the room success.");
                OnJoinRoomResult?.Invoke(true);
            }
            else
            {
                Debug.Log("Join the room faild.");
                OnJoinRoomResult?.Invoke(false);
            }
        }

        private void ExitRoomResponse(byte[] data)
        {
            ExitRoomData result = SerializerFactory.Create().Deserialize<ExitRoomData>(data);
            if (result.Suc)
            {
                Debug.Log("Exit the room success.");
            }
            else
            {
                Debug.Log("Exit the room faild.");
            }
        }

        private void UpdateCameraParamResponse(byte[] data)
        {
            CameraParam result = SerializerFactory.Create().Deserialize<CameraParam>(data);
            OnCameraParamUpdate?.Invoke(result);
            Debug.Log(result.fov.ToString());
        }
        #endregion
    }
}