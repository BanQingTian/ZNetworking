using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zrime;

public static class MsgId
{
    public const string __EXHIBIT_SHOW_PNG_MSG_ = "exhibit_show_png_msg";

    #region special id

    public const string __READY_PLAY_MSG_ = "ready_play_msg";
    public const string __JOIN_NEW_PLAYER_MSG_ = "join_new_player_msg";
    public const string __LEAVE_A_PLAYER_MSG_ = "leave_a_player_msg";
    public const string __PLAY_GAME_MSG_ = "play_Game_msg";
    public const string __SHOOT_BUBBLE_MSG_ = "shoot_bubble_msg";
    public const string __DRAGON_BEHIT_MSG_ = "dragon_behit_msg";
    public const string __DRAGON_DEATH_MSG_ = "dragon_death_msg";
    public const string __RESET_GAME_MSG_ = "reset_game_msg";
    public const string __RANDOM_PLAYER_MSG_ = "random_player_msg";
    public const string __TEST_MSG_ = "test_msg";

    #endregion
}

public class ZMessageManager
{

    public static ZMessageManager m_Instance;
    public static ZMessageManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new ZMessageManager();
            }
            return m_Instance;
        }
    }

    public ZClient client;

    private bool m_Initialized = false;

    public void Init()
    {
        if (m_Initialized) return;

        client = ZClient.Instance;
        client.Persist();
        client.AddListener(MsgId.__READY_PLAY_MSG_, _S2C_ReadyPlay);
        client.AddListener(MsgId.__PLAY_GAME_MSG_, _S2C_PlayGame);
        client.AddListener(MsgId.__JOIN_NEW_PLAYER_MSG_, _S2C_JoinNewPlayer);
        if (Global.CurRoom == Global.dragon)
        {
            client.AddListener(MsgId.__SHOOT_BUBBLE_MSG_, _S2C_ShootBubble);
            client.AddListener(MsgId.__DRAGON_BEHIT_MSG_, _S2C_DragonBehit);
            client.AddListener(MsgId.__DRAGON_DEATH_MSG_, _S2C_DragonDeath);
            client.AddListener(MsgId.__RESET_GAME_MSG_, _S2C_ResetGame);
            client.AddListener(MsgId.__RANDOM_PLAYER_MSG_, _S2C_RandomAttackPlayer);
        }
        else if (Global.CurRoom == Global.exhibit)
        {

        }
        m_Initialized = true;
    }

    #region C2SFunc

    public void SendConnectAndJoinRoom(string serverIp, string port)
    {
        client.Connect(serverIp, port);
    }

    public void SendMsg(string msdId, string msgContent)
    {
        client.SendMsg(msdId, msgContent);
    }

    #endregion


    #region ResponseFunc

    public void _S2C_JoinNewPlayer(object msg)
    {
        Player player = msg as Player;

        Debug.Log("createAAAAAA");
        PlayerEntity pe = GameObject.Instantiate<PlayerEntity>(ZNetworkingManager.Instance.GetPrefab());
        pe.Init(player);
        pe.UpdatePoseData();

        ZPlayerMe.Instance.AddPlayer(player.PlayerId, pe);
    }

    public void _S2C_ReadyPlay(object msg)
    {
        // msg.content = playerid,true
        Message m = msg as Message;
        Debug.Log(m.Content);

        var ms = m.Content.Split(',');

        if (client.RoomID == Global.dragon)
        {
            DragonManager.Instance.ReadyPlay(ms[0], ms[1]);
        }
        else if (client.RoomID == Global.exhibit)
        {

        }
    }

    public void _S2C_PlayGame(object msg)
    {
        Message m = msg as Message;
        Debug.Log(m.Content);

        if (client.RoomID == Global.dragon)
        {
            DragonManager.Instance.PlayGame();
        }
        else if (client.RoomID == Global.exhibit)
        {

        }
    }

    public void _S2C_ResetGame(object msg)
    {
        DragonManager.Instance.ResetGame();
    }

    public void _S2C_ShootBubble(object msg)
    {
        Message m = msg as Message;
        DragonManager.Instance.ShootBubble(m.Content);
    }

    public void _S2C_DragonBehit(object msg)
    {
        Message m = msg as Message;

        DragonManager.Instance.DamageDragon(int.Parse(m.Content));
    }

    public void _S2C_DragonDeath(object msg)
    {
        Message m = msg as Message;

        DragonManager.Instance.DragonDead();
    }

    public void _S2C_RandomAttackPlayer(object msg)
    {
        Message m = msg as Message;

        DragonManager.Instance.Dragon.RandomTarget(m.Content);
    }

    #endregion
}
