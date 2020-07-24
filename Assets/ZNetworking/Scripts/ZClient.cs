using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zrime;
using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

public class ZClient
{
    private bool m_Initialized = false;
    private bool m_Connected = false;    // connect server
    private bool m_Dispose = false;      // create stream dispose

    private string m_RoomID; // = "world";
    private string m_PlayerID;
    private string m_PlayerName;
    private bool m_IsHouseOwner;

    public bool Connected
    {
        get { return m_Connected; }
    }
    public bool Dispose
    {
        get { return m_Dispose; }
    }
    public string RoomID
    {
        get { return m_RoomID; }
    }
    public string PlayerName
    {
        get { return m_PlayerName; }
    }
    public string PlayerID
    {
        get { return m_PlayerID; }
    }
    public bool IsHouseOwner
    {
        get { return m_IsHouseOwner; }
    }


    private Channel channel;
    private Exhibit.ExhibitClient client;

    public string extraContent = "shelter";

    public delegate void RevMsgListener(object msg);
    public delegate void JoinNewPlayerEventHandler(Player player);
    public delegate void ConnectFaildResponse();
    public event ConnectFaildResponse ConnectFailEvent;

    public Dictionary<string, RevMsgListener> MsgListener;

    /// <summary>
    /// 同步位置的主物体
    /// </summary>
    public GameObject Model;

    public Transform Controller;

    public string UUID;
    private static ZClient m_Instance;
    public static ZClient Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new ZClient(Global.CurRoom);
                m_Instance.UUID = System.Guid.NewGuid().ToString("N");
            }
            return m_Instance;
        }
    }


    public ZClient(string roomID)
    {
        m_RoomID = roomID;
        m_PlayerID = "player";
        m_PlayerName = Global.GetName();
        MsgListener = new Dictionary<string, RevMsgListener>();
    }

    public void Persist()
    {
        m_PlayerID = "target";
    }

    public void AddListener(string id, RevMsgListener listener)
    {
        if (MsgListener.ContainsKey(id))
        {
            Debug.LogError("msg listener has repeated");
            return;
        }
        MsgListener[id] = listener;
    }

    public void RemoveListener(string id, RevMsgListener listener)
    {
        if (!MsgListener.ContainsKey(id))
        {
            Debug.Log("Msg listener was not exist!");
            return;
        }
        MsgListener.Remove(id);
    }

    /// <summary>
    /// 连接服务器，并加入房间
    /// </summary>
    public async void Connect(string ip, string port)
    {
        if (m_Initialized || m_Connected)
            return;

#if UNITY_EDITOR
        ip = "127.0.0.1";
#endif
        channel = new Channel(string.Format("{0}:{1}", ip, port), ChannelCredentials.Insecure);
        client = new Exhibit.ExhibitClient(channel);
        try
        {
            var res = await client.JoinAsync(new JoinRequest { RoomId = m_RoomID });
            m_PlayerID = res.PlayerId;
            m_IsHouseOwner = res.IsHouseOwner;
            m_Initialized = true;
            m_Connected = true; // 完成stream连接
        }
        catch (System.Exception e)
        {
            Debug.LogError("Connect Server failed..." + e);
            return;
        }

        Debug.Log("CreateStream Begin");
        CreateStream();


        ZNetworkingManager.Instance.CreateOwner(m_PlayerID, m_IsHouseOwner);
        ClientUpdator.Instance.StartCoroutine(SyncPoseCor());

    }


    /// <summary>
    /// Upload pose
    /// </summary>
    /// <returns></returns>
    private IEnumerator SyncPoseCor()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / Global.FrameRate);
            if (m_Connected)
            {
                Sync(Model.transform.position, Model.transform.eulerAngles, extraContent);
            }
        }
    }

    public async void Sync(Vector3 position, Vector3 euler, string extra)
    {
        Vector3 cp = Vector3.zero;
        Vector3 cr = Vector3.zero;
        if (Global.DeviceType == DeviceTypeEnum.NRLight)
        {
            cp = Controller.position;
            cr = Controller.eulerAngles;
        }

        var response = await client.SyncPoseAsync(new SyncRequest
        {
            RoomId = m_RoomID,
            Player = new Player
            {
                PlayerId = m_PlayerID,
                PlayerName = m_PlayerName,
                IsHouseOwner = m_IsHouseOwner,
                Position = new ZPosition
                {
                    X = position.x,
                    Y = position.y,
                    Z = position.z,
                },
                Rotation = new ZRotation
                {
                    EulerX = euler.x,
                    EulerY = euler.y,
                    EulerZ = euler.z,
                },
                SecondPosition = new ZPosition { X = cp.x, Y = cp.y, Z = cp.z },
                SecondRotation = new ZRotation { EulerX = cr.x, EulerY = cr.y, EulerZ = cr.z },
                ExtraContent = extra,
            }
        });

        //SyncEvent?.Invoke(response.Players.GetEnumerator());
        DownloadSyncData(response.Players.GetEnumerator());
    }

    public void DownloadSyncData(IEnumerator<Player> enumerator)
    {
        var exceptPlayerList = new List<string>(ZPlayerMe.Instance.PlayerKeys);

        while (enumerator.MoveNext())
        {
            Player player = enumerator.Current;
            Entity entity;
            PlayerEntity pe;
            if (ZPlayerMe.Instance.PlayerMap.TryGetValue(player.PlayerId, out entity))
            {
                pe = entity as PlayerEntity;
                if (pe != null)
                    pe.UpdateData(player);
            }
            else
            {
                MsgListener[MsgId.__JOIN_NEW_PLAYER_MSG_]?.Invoke(player);
            }

            if (player.PlayerId == m_PlayerID)
            {
                // 同步client中数据的状态
                m_IsHouseOwner = player.IsHouseOwner;
            }

            exceptPlayerList.Remove(player.PlayerId);
        }
        foreach (var item in exceptPlayerList)
        {


            ZPlayerMe.Instance.RemovePlayer(item);
        }
    }

    #region Bind Stream

    public async void CreateStream()
    {
        if (m_Dispose)
            return;
        await bindStream();
    }

    public void DisposeStream()
    {
        m_Dispose = true;
        call?.Dispose();
    }


    AsyncServerStreamingCall<Message> call;
    private async Task bindStream()
    {
        try
        {
            using (call = client.CreateStream(new Zrime.Connect
            {
                Player = new Player
                {
                    PlayerId = m_PlayerID,
                    PlayerName = m_PlayerName,
                },
                RoomId = m_RoomID,
                Active = true,
                DeviceType = "0",
            }))
            {
                IAsyncStreamReader<Message> rs = call.ResponseStream;

                while (await rs.MoveNext())
                {
                    Message msg = rs.Current;
                    OperateMsg(msg);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Create Stream Error - " + e);
        }
    }


    #endregion
    List<System.Action> aa = new List<System.Action>();
    public void OperateMsg(Message msg)
    {
        //lock (MsgListener)
        {
            RevMsgListener listener;
            if (MsgListener.TryGetValue(msg.ContentType, out listener))
            {
                listener?.Invoke(msg);
            }
        }
    }

    public async void SendMsg(string type, string content)
    {
        var res = await client.BroadcastMessageAsync(new Message
        {
            PlayerId = m_PlayerID,
            RoomId = m_RoomID,
            ContentType = type,
            Content = content,
            Timestamp = ZUtils.GetTimeStamp().ToString(),
        });
        Debug.Log("sendmsg : " + type);
    }

    public void Leave()
    {
        client?.Leave(new LeaveRequest
        {
            RoomId = m_RoomID,
            PlayerId = m_PlayerID,
        });

        DisposeStream();

        m_Initialized = false;
        m_Connected = false;
    }
}
