using NRKernal.ObserverView.NetWork;
using UnityEngine;

public class NetTest : MonoBehaviour
{
    private NetWorkClient network;
    private void Start()
    {
        network = new NetWorkClient();

        EnterRoomData data = new EnterRoomData();
        data.result = false;

        var serilizer = SerializerFactory.Create();
        var bnary = serilizer.Serialize(data);

        var data2 = serilizer.Deserialize<EnterRoomData>(bnary);
        Debug.Log(data2.result);
    }

    public void Connect()
    {
        network.Connect("192.168.69.213", 6000);
    }

    public void EnterRoom()
    {
        network.EnterRoomRequest();
    }

    public void ExitRoom()
    {
        network.ExitRoomRequest();
    }

    public void UpdateCameraParam()
    {
        network.UpdateCameraParamRequest();
    }

    public void Disconnect()
    {
        network.Dispose();
    }
}
