using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 本地数据类
/// </summary>
public class ZPlayerMe
{

    private static ZPlayerMe m_Instance;
    public static ZPlayerMe Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new ZPlayerMe();
            }
            return m_Instance;
        }
    }

    /// <summary>
    /// 玩家数据 key=playerId value=实体
    /// </summary>
    public Dictionary<string, Entity> PlayerMap = new Dictionary<string, Entity>();

    /// <summary>
    /// 所有玩家的准备情况
    /// </summary>
    public Dictionary<string, bool> PlayerReadyDic = new Dictionary<string, bool>();


    public bool SetPlayerReadyDic(string playerId, string ready)
    {
        PlayerReadyDic[playerId] = ready.Equals("1");

        foreach (var item in PlayerReadyDic.Values)
        {
            if (!item)
                return false;
        }
        return true;
    }

    public void AddPlayer(string id, Entity player)
    {
        Debug.Log("AddPlayer : " + id);
        if (PlayerMap.ContainsKey(id))
        {
            Debug.LogError("######## Exit some ID player !!!! " + id);
        }
        else
        {
            PlayerMap[id] = player;
        }
    }
}
