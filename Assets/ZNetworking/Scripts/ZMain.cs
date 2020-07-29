using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class ZMain : MonoBehaviour
{

    [Space(12)]
    public RoomEnum RoomName;

    [Space(12)]
    public DeviceTypeEnum DeviceType;

    [Space(12)]
    public ZScanMarker MarkerHelper;

    [Space(12)]
    public bool IS_MATCH = false;

    void Start()
    {
        Begin();
    }


    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 5;
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            Time.timeScale = 1;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            ZMessageManager.Instance.SendMsg(MsgId.__DRAGON_BEHIT_MSG_, "1");
        }
#endif

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
        deviceCheck();
        loadRoomManager();
        loadNetworkingModule();

        ZPlayerMe.Instance.Init();
    }

    private void deviceCheck()
    {
        Global.DeviceType = DeviceType;

        if (DeviceType == DeviceTypeEnum.NRLight)
        {
            var nrCam = GameObject.Find("NRCameraRig");
            var nrInput = NRInput.AnchorsHelper.GetAnchor(ControllerAnchorEnum.RightModelAnchor);
            ZClient.Instance.Model = nrCam;
            ZClient.Instance.Controller = nrInput;
            ZClient.Instance.extraContent = "";

        }
        else if (DeviceType == DeviceTypeEnum.Pad)
        {
            var arCam = GameObject.Find("First Person Camera");
            ZClient.Instance.Model = arCam;
        }
    }

    public void loadRoomManager()
    {
        if (RoomName == RoomEnum.Dragon)
        {
            DragonManager.Instance.Init(this);
        }
        else if (RoomName == RoomEnum.Exhibit)
        {

        }
    }

    // 网络所需组件，实例化网络组件
    private void loadNetworkingModule()
    {
        Global.CurRoom = Global.GetRoomName(RoomName);
        ZMessageManager.Instance.Init();
        ZMessageManager.Instance.SendConnectAndJoinRoom("192.168.69.39", "50010"); //192.168.69.39
    }


}
