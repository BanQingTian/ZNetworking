using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 本地数据类
/// </summary>
public class ZPlayerMe {

    private static ZPlayerMe m_Instance;
    public static ZPlayerMe Instance
    {
        get
        {
            if(m_Instance == null)
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

    public void AddPlayer(string id,Entity player)
    {
        Debug.Log("run1");
        if (PlayerMap.ContainsKey(id))
        {
            Debug.LogError("######## Exit some ID player !!!! "+ id);
        }
        else
        {
            PlayerMap[id] = player;
        }
    }
}
