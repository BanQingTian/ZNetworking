using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;

public class DragonManager : MonoBehaviour
{

    public static DragonManager Instance;

    private ZMain m_ZMain;

    private bool m_Initialized = false;

    public bool PlayingFight = false;

    // 是否已经开始等待游戏开始
    private bool showReadyBtnYet = false;
    // 准备状态
    private bool ready = false;

    #region UI

    public Button ReadyBtn;
    public Button PlayBtn;

    #endregion

    /// <summary>
    /// 数据类
    /// </summary>
    public DragonAttack Dragon;


    #region Unity Internal

    private void Awake()
    {
        Instance = this;
        ReadyBtn.onClick.AddListener(ShowReadyBtnClk);
        PlayBtn.onClick.AddListener(ClkPlayBtn);

        ZClient.Instance.ConnectFailEvent += ConnectFaildTip;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayingFight)
        {
            if (NRInput.GetButtonDown(ControllerButton.TRIGGER))
            {
                ZMessageManager.Instance.SendMsg(MsgId.__SHOOT_BUBBLE_MSG_, ZClient.Instance.PlayerID);
            }
        }
        onUpdate();
    }

    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="main"></param>
    public void Init(ZMain main)
    {
        if (m_Initialized)
            return;

        m_ZMain = main;


        m_Initialized = true;
    }

    private void onUpdate()
    {
        if (!m_Initialized)
            return;

        // 扫码marker成功后 显示准备按钮
        if (m_ZMain.IS_MATCH && !showReadyBtnYet && ZClient.Instance.Connected)
        {
            ShowReadyBtn();
        }
    }



    #region Fight

    public void ShootBubble(string playerId)
    {
        Entity ee;
        if (ZPlayerMe.Instance.PlayerMap.TryGetValue(playerId, out ee))
        {
            PlayerEntity pe = ee as PlayerEntity;
            pe.Weapon.ShootBubble(playerId);
        }
        else
        {
            Debug.LogError("不存在 player id : " + playerId);
        }
    }

    public void DamageDragon(int damageHp)
    {
        Dragon.DamageDragon(damageHp);
    }

    public void DragonDead()
    {
        Dragon.DragonDeath();
    }

    #endregion


    #region begin stage

    /// <summary>
    /// 有人准备
    /// </summary>
    public void ReadyPlay(string playerId, string isReady)
    {
        bool allready = ZPlayerMe.Instance.SetPlayerReadyDic(playerId, isReady);

        if (allready)
        {
            Debug.Log("All ready !!");
            PlayBtn.enabled = true;
            PlayBtn.image.material.SetFloat("_Saturation", 1);
        }
        else
        {
            PlayBtn.enabled = false;
            PlayBtn.image.material.SetFloat("_Saturation", 0);
        }
    }

    /// <summary>
    /// 开始 QAQ
    /// </summary>
    public void PlayGame()
    {
        ReadyBtn.gameObject.SetActive(false);
        Debug.Log("playing");

        playDragonAnim();
        resetDragonHp();
    }

    /// <summary>
    /// 播放龙进场动画
    /// </summary>
    private void playDragonAnim()
    {
        // 播放龙进场动画
        Dragon.AnimManager.PlayDragonEnterAnim();
    }
    /// <summary>
    /// 根据人数动态设置龙的血量
    /// </summary>
    private void resetDragonHp()
    {
        Dragon.Init(ZPlayerMe.Instance.PlayerMap.Count);
    }

    public void BeginFight()
    {
        PlayingFight = true;
        (ZPlayerMe.Instance.PlayerMap[ZClient.Instance.PlayerID] as PlayerEntity).Weapon.gameObject.SetActive(true);
    }

    public void ResetGame()
    {
        PlayingFight = false;
        Dragon.gameObject.SetActive(false);
        Dragon.AnimManager.DeathEffSwitch.SetActive(false);
        Dragon.AnimManager.Roof.SetActive(false);
        resetDragonHp();
        ShowReadyBtn();
    }

    #endregion

    #region NetUI

    public void ConnectFaildTip()
    {
        ReadyBtn.gameObject.SetActive(false);
        PlayBtn.gameObject.SetActive(false);
    }

    #endregion


    #region UILogic

    public void RemovePlayerRefreshUI(string playerId)
    {
        var allready = ZPlayerMe.Instance.IsAllReady();
        ShowReadyBtn();
    }


    public void ShowReadyBtn()
    {
        ReadyBtn.gameObject.SetActive(true);
        //ReadyBtn.image.material.SetFloat("_Saturation", 0.3f);
        SetUILayout(ZClient.Instance.IsHouseOwner);
        showReadyBtnYet = true;
        ready = false;
    }

    private void SetUILayout(bool isHouseOwner)
    {
        if (isHouseOwner)
        {
            ReadyBtn.transform.localPosition = new Vector3(-228, 0, 0);
            PlayBtn.image.material.SetFloat("_Saturation", 0);
            PlayBtn.enabled = false;
            PlayBtn.gameObject.SetActive(true);
        }
        else
        {
            ReadyBtn.transform.localPosition = Vector3.zero;
            PlayBtn.gameObject.SetActive(false);
        }
    }
    public void ShowReadyBtnClk()
    {
        if (!ready)
        {
            ready = true;
            ZMessageManager.Instance.SendMsg(MsgId.__READY_PLAY_MSG_, string.Format("{0},{1}", ZClient.Instance.PlayerID, ready ? "1" : "0"));
        }
    }
    public void ShowPlayBtn()
    {
        PlayBtn.gameObject.SetActive(true);
    }
    public void ClkPlayBtn()
    {
        if (ZPlayerMe.Instance.IsAllReady())
        {
            PlayBtn.gameObject.SetActive(false);
            ZMessageManager.Instance.SendMsg(MsgId.__PLAY_GAME_MSG_, "Go");
        }
        
    }

    #endregion

}
