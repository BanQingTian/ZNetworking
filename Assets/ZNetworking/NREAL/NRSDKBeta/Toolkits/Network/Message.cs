using System;

namespace NRKernal.ObserverView.NetWork
{
    // Net msg type
    public enum MessageType
    {
        None = 0,          // Empty type

        Connected = 1,     // Connect server
        Disconnect = 2,    // Disconnect from server

        HeartBeat = 3,     // Heart beat 
        EnterRoom = 4,     // Enter room
        ExitRoom = 5,     // Enter room

        UpdateCameraParam = 6,
    }

    [Serializable]
    public class EnterRoomData
    {
        public bool result;   // Enter room result
    }

    [Serializable]
    public class ExitRoomData
    {
        public bool Suc;   // Exit room result
    }

    [Serializable]
    public class CameraParam
    {
        public Fov4f fov;    // Camera fov
    }
}
