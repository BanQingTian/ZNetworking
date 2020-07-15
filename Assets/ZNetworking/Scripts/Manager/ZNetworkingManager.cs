using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zrime;

public class ZNetworkingManager : MonoBehaviour
{
    public static ZNetworkingManager m_Instance;
    public static ZNetworkingManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                var net = Resources.Load<ZNetworkingManager>("ZNetworkingManager");
                GameObject netGO = Instantiate<GameObject>(net.gameObject);
                DontDestroyOnLoad(netGO);
            }
            return m_Instance;
        }
    }

    public PlayerEntity PlayerPrefab;

    private void Awake()
    {
        m_Instance = this;
    }

    public GameObject CreateOwner(string playerid, bool isOwner)
    {
        Player virtualPlayer = new Player
        {
            PlayerId = playerid,
            PlayerName = "shelter",
            IsHouseOwner = isOwner,
            Position = new ZPosition(),
            Rotation = new ZRotation(),
            ExtraContent = "shelter",
        };

        Debug.Log("createBBBB");


        PlayerEntity pe = GameObject.Instantiate<PlayerEntity>(PlayerPrefab);
        pe.Init(virtualPlayer);
        pe.UpdatePoseData();

        ZPlayerMe.Instance.AddPlayer(virtualPlayer.PlayerId, pe);
        
        return pe.gameObject;
    }

    private void OnApplicationQuit()
    {
        ZClient.Instance.Leave();
    }
}
