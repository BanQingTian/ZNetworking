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
    public string PlayerID
    {
        get { return m_PlayerID; }
    }


    private Channel channel;
    private Exhibit.ExhibitClient client;

    public string extraContent = "shelter";

    public delegate void RevMsgListener(object msg);
    public delegate void JoinNewPlayerEventHandler(Player player);

    public Dictionary<string, RevMsgListener> MsgListener;

    /// <summary>
    /// 同步位置的主物体
    /// </summary>
    public GameObject Model;

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
        catch (System.Exception)
        {
            Debug.LogError("Connect Server failed...");
            return;
        }

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
        var response = await client.SyncPoseAsync(new SyncRequest
        {
            RoomId = m_RoomID,
            Player = new Player
            {
                PlayerId = m_PlayerID,
                PlayerName = "nihao",
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
                ExtraContent = extra,
            }
        });

        //SyncEvent?.Invoke(response.Players.GetEnumerator());
        DownloadSyncData(response.Players.GetEnumerator());
    }

    public void DownloadSyncData(IEnumerator<Player> enumerator)
    {
        var exceptPlayerList = new List<string>(ZPlayerMe.Instance.PlayerMap.Keys);

        while (enumerator.MoveNext())
        {
            Player player = enumerator.Current;

            //if (m_PlayerID != player.PlayerId)
            {
                Entity entity;
                PlayerEntity pe;
                if (ZPlayerMe.Instance.PlayerMap.TryGetValue(player.PlayerId, out entity))
                {
                    pe = entity as PlayerEntity;
                    pe.UpdateData(player);
                }
                else
                {
                    MsgListener[MsgId.__JOIN_NEW_PLAYER_MSG_]?.Invoke(player);
                }

                exceptPlayerList.Remove(player.PlayerId);
            }
            //else
            //{
            //    exceptPlayerList.Remove(m_PlayerID);
            //}
        }
        foreach (var item in exceptPlayerList)
        {
            GameObject.Destroy(ZPlayerMe.Instance.PlayerMap[item].gameObject);
            Debug.LogError("Dostroy player id : " + item);
            ZPlayerMe.Instance.PlayerMap.Remove(item);
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
                    PlayerName = "visit",
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
        catch (System.Exception)
        {
            Debug.LogError("Create Stream Error");
        }
    }


    #endregion

    public void OperateMsg(Message msg)
    {
        RevMsgListener listener;
        if (MsgListener.TryGetValue(msg.ContentType, out listener))
        {
            listener?.Invoke(msg);
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
        Debug.Log("sendmsg");
    }

    public void Leave()
    {
        client.Leave(new LeaveRequest
        {
            RoomId = m_RoomID,
            PlayerId = m_PlayerID,
        });

        DisposeStream();

        m_Initialized = false;
        m_Connected = false;
    }
}
