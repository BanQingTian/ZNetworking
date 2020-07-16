using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class ZMain : MonoBehaviour {

    

    [Space(12)]
    public RoomEnum RoomName;

    [Space(12)]
    public DeviceTypeEnum DeviceType;

    [Space(12)]
    public ZScanMarker MarkerHelper;

    [Space(12)]
    public bool IS_MATCH = false;


    void Start ()
    {
        Debug.Log("start1");
        Begin();
	}
	
	void Update () {

        if (!IS_MATCH)
        {
            if (MarkerHelper.MarkerTrackingUpdate())
            {
                IS_MATCH = true;
            }
        }

        //if(Input.GetKeyDown(KeyCode.R) /*|| CheckMappingMatch()*/)
        //{
        //    IS_MATCH = true;
        //    ZMessageManager.Instance.SendMsg(MsgId.__PLAY_GAME_MSG, "nihao");
        //}
	}

    private void Begin()
    {
        DeviceCheck();
        LoadNetworkingModule();
    }

    private void DeviceCheck()
    {
        if(DeviceType == DeviceTypeEnum.NRLight)
        {
            var nrCam = GameObject.Find("NRCameraRig");
            ZClient.Instance.Model = nrCam;
        }
        else if(DeviceType == DeviceTypeEnum.Pad)
        {
            var nrCam = GameObject.Find("NRCameraRig");
            ZClient.Instance.Model = nrCam;
        }
    }

    // 网络所需组件，实例化网络组件
    private void LoadNetworkingModule()
    {
        Global.CurRoom = Global.GetRoomName(RoomName);
        Global.DeviceType = DeviceType;
        ZMessageManager.Instance.Init();
        ZMessageManager.Instance.SendConnectAndJoinRoom("192.168.31.141", "50010");
    }


}
