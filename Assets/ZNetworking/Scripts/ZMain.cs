using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class ZMain : MonoBehaviour {

    [HideInInspector]
    public bool IS_MATCH = false;

    [Space(12)]
    public RoomEnum RE;

    [Space(12)]
    public DeviceTypeEnum DeviceType;

	void Start ()
    {
        Main();
	}
	
	void Update () {
		
        if(Input.GetKeyDown(KeyCode.R) /*|| CheckMappingMatch()*/)
        {
            IS_MATCH = true;
            ZMessageManager.Instance.SendMsg(MsgId.__PLAY_GAME_MSG, "nihao");
        }

        if (IS_MATCH)
        {
        }
	}

    private void Main()
    {
        LoadNetworkingModule();
    }

    // 网络所需组件，实例化网络组件
    private void LoadNetworkingModule()
    {
        Global.CurRoom = Global.GetRoomName(RE);
        Global.DeviceType = DeviceType;

        ZMessageManager.Instance.Init();
        ZMessageManager.Instance.SendConnectAndJoinRoom("192.168.31.141", "50010");
    }


}
