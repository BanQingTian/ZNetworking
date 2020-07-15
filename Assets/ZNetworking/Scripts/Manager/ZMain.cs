using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class ZMain : MonoBehaviour {


    public bool IS_MATCH = false;

	void Start () {

        Begin();

	}
	
	// Update is called once per frame
	void Update () {
		
        if(Input.GetKeyDown(KeyCode.R) /*|| CheckMappingMatch()*/)
        {
            IS_MATCH = true;
            ZMessageManager.Instance.SendMsg(MsgId.__PLAY_GAME_MSG, "nihao");
        }

        if (IS_MATCH)
        {
            PrepareBeginGame();
        }
	}

    private void Begin()
    {
        Global.CurRoom = Global.GetRoomName(RoomEnum.Dragon);

        ZMessageManager.Instance.Init();

        ZMessageManager.Instance.SendConnectAndJoinRoom("192.168.31.141", "50010");
    }

    public bool CheckMappingMatch()
    {
        // todo - match mapping and marker


        return true;
    }

    public void PrepareBeginGame()
    {
        loadPlayerEntity();
    }
   

    private void loadPlayerEntity()
    {

    }
}
