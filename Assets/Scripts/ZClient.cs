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

    private Channel channel;
    private Exhibit.ExhibitClient client;


    GameObject Model;

    public ZClient()
    {
        m_RoomID = "szcw";
        m_PlayerID = "player";
    }
    public ZClient(string roomID, GameObject prefab)
    {
        m_RoomID = roomID;
        m_PlayerID = "player";
        Model = prefab;
    }

    public void Persist()
    {
        m_PlayerID = "target";
    }

    /// <summary>
    /// 连接服务器，并加入指定房间
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

        var res = await client.JoinAsync(new JoinRequest { RoomId = m_RoomID });

        m_PlayerID = res.PlayerId;

        m_Initialized = true;
    }

    public async void CreateStream()
    {
        if (m_Dispose)
            return;
        await bindStream();
    }

    public void DisposeStream()
    {
        m_Dispose = true;
    }

    private async Task bindStream()
    {
        try
        {
            Vector3 pos = Model.transform.position;
            Vector3 euler = Model.transform.eulerAngles;

            var call = client.CreateStream(new Zrime.Connect
            {
                Player = new Player
                {
                    PlayerId = m_PlayerID,
                    PlayerName = "visit",
                    Position = new ZPosition
                    {
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                    },
                    Rotation = new ZRotation
                    {
                        EulerX = euler.x,
                        EulerY = euler.y,
                        EulerZ = euler.z,
                    },
                },
                Active = true,
                DeviceType = "0",
            });

            m_Dispose = false;

            while (true)
            {
                if (m_Dispose)
                {
                    call.Dispose();
                    return;
                }

                IAsyncStreamReader<Message> rs = call.ResponseStream;
                
                while (await rs.MoveNext())
                {
                    Message msg = rs.Current;
                    Debug.Log(msg.Content);
                }

            }

        }
        catch (System.Exception)
        {

            throw;
        }
    }

    public async void SendMsg(string type,string content)
    {
        var res = await client.BroadcastMessageAsync(new Message
        {
            PlayerId = m_PlayerID,
            ContentType = type,
            Content = content,
            Timestamp = "",
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

        m_Initialized = false;
        m_Connected = false;
    }
}
