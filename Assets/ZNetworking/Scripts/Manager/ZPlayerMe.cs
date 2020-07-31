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

    public delegate void RemovePlayerDelegate(string playerId);
    public event RemovePlayerDelegate RemovePlayerEvent;

    /// <summary>
    /// 玩家数据 key=playerId value=实体
    /// </summary>
    public Dictionary<string, Entity> PlayerMap = new Dictionary<string, Entity>();
    public List<string> PlayerKeys = new List<string>();
    
    /// <summary>
    /// 所有玩家的准备情况
    /// </summary>
    public Dictionary<string, bool> PlayerReadyDic = new Dictionary<string, bool>();

    public void Init()
    {
        RemovePlayerEvent += Remove;
        RemovePlayerEvent += DragonManager.Instance.RefreshUI;

    }

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

    public void RemovePlayer(string id)
    {
        RemovePlayerEvent.Invoke(id);
    }
    private void Remove(string id)
    {
        if (PlayerMap.ContainsKey(id))
        {
            GameObject.Destroy(PlayerMap[id].gameObject);
            Debug.LogError("Dostroy player id : " + id);
            PlayerMap.Remove(id);
            PlayerKeys.Remove(id);
            PlayerReadyDic.Remove(id);
        }
        else
        {
            Debug.Log("PlayerMe Remove : " + id + " Failed !!!");
        }

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
            PlayerKeys.Add(id);
            PlayerReadyDic[id] = false;
        }

        DragonManager.Instance.RefreshUI(id);
    }

    public void ResetPlayerStatus()
    {
        for (int i = 0; i < PlayerKeys.Count; i++)
        {
            PlayerReadyDic[PlayerKeys[i]] = false;
        }
    }

    public bool IsAllReady()
    {
        foreach (var item in PlayerReadyDic.Values)
        {
            if (!item)
                return false;
        }
        return true;
    }
}
