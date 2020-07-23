using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 龙对象类
/// </summary>
public class DragonAttack : MonoBehaviour
{
    private int totalHp;
    public int CurHP = 99;
    private const int ratio = 10; // 血量单位基数*人数

    private bool firstBeHit = true;
    private float bubbleAddRatio = 0.1f;
    public Transform DragonBubble;


    public GameObject AttackPoint;
    public GameObject Fireball;
    public Rigidbody Rig;
    public Transform DragonParent;

    private bool dead = false;

    /// <summary>
    /// 动画管理类
    /// </summary>
    public DragonAnimManager AnimManager;

    // Audio
    public AudioSource MainAS;
    public AudioClip FlapWingClip;
    public AudioClip FireClip;

    private void Start()
    {
        gameObject.SetActive(false);
        DragonBubble.gameObject.SetActive(false);
    }

    public void Init(int playerCount)
    {
        firstBeHit = true;
        dead = false;
        AnimManager.ResetStatue();

        totalHp = playerCount * ratio;
        CurHP = totalHp;

        bubbleAddRatio = (2 - 1) / (float)totalHp;
    }

    public void DamageDragon(int damageHp)
    {
        CurHP -= damageHp;

        SetBubble();

        CurHP = CurHP > 0 ? CurHP : 0;

        if (CurHP == 0)
        {
            dead = true;
            if (ZClient.Instance.IsHouseOwner)
            {
                ZMessageManager.Instance.SendMsg(MsgId.__DRAGON_DEATH_MSG_, CurHP.ToString());
                return;
            }
        }
    }
    private void SetBubble()
    {
        if (firstBeHit)
        {
            DragonBubble.gameObject.SetActive(true);
        }

        float add = (totalHp - CurHP) * bubbleAddRatio;

        DragonBubble.localScale = Vector3.one * (1 + add);
    }

    public void DragonDeath()
    {
        dead = true;
        AnimManager.PlayDeadAnim();
    }

    private void ResetDeathAnim()
    {
        gameObject.SetActive(true);
    }

    #region Animation Event

    public void DragonDeathBegin()
    {
        DragonBubble.gameObject.SetActive(false);

        StopCoroutine("rotateHeadCor");
        StartCoroutine(rotateHeadCor(DragonParent, 90, 40, () =>
        {
           

        }));
    }

    /// <summary>
    /// 死亡动画播放完播放特效
    /// </summary>
    public void DragonDeathEff()
    {
        Debug.Log("DeathEff Playing");

        //DragonParent.eulerAngles = new Vector3(0, 90, 0);



        AnimManager.PlayDeathEff();
        gameObject.SetActive(false);

    }

    /// <summary>
    /// 动画事件调用
    /// </summary>
    public void DragonShootFireball()
    {
        PlayFireballAudio();

        Rig.isKinematic = true;
        Rig.isKinematic = false;
        Fireball.transform.position = AttackPoint.transform.position;
        Fireball.transform.rotation = AttackPoint.transform.rotation;
        Fireball.SetActive(true);
        Rig.AddForce(AttackPoint.transform.forward * 600);
    }

    /// <summary>
    /// 动画事件调用
    /// </summary>
    /// <param name="damageHp"></param>
    public void DragonRotateHead()
    {
        DragonManager.Instance.BeginFight();

        PlayWingReadyAudio();

        float random = Random.Range(50f, 130f);
        StopCoroutine(rotateHeadCor(DragonParent, random, 5));

        if (dead)
        {
            StartCoroutine(rotateHeadCor(DragonParent, 90, 12));
        }
        else
        {
            StartCoroutine(rotateHeadCor(DragonParent, random, 5));
        }

    }
    private IEnumerator rotateHeadCor(Transform startTrans, float end, float speed = 1, System.Action finishi = null)
    {
        float dir = Time.deltaTime * 4 * speed;
        dir = end > startTrans.eulerAngles.y ? dir : -dir;
        while (Mathf.Abs(startTrans.eulerAngles.y - end) > Mathf.Abs(dir * 1.5f))
        {
            Vector3 v3 = DragonParent.transform.eulerAngles;
            float y = v3.y + dir;
            startTrans.transform.eulerAngles = new Vector3(v3.x, y, v3.z);
            if (y > 130 || y < 50)
            {
                startTrans.transform.eulerAngles = new Vector3(v3.x, Mathf.Clamp(y, 50, 130), v3.z);
            }
            yield return null;
        }
        startTrans.eulerAngles = new Vector3(DragonParent.transform.eulerAngles.x, end, DragonParent.transform.eulerAngles.z);
        finishi?.Invoke();
    }

    #region Audio Event

    // 上升扇翅膀音效
    public void PlayWingReadyAudio()
    {
        MainAS.clip = FlapWingClip;
        MainAS.Play();
    }

    // 喷火球动画音效
    public void PlayFireballAudio()
    {
        MainAS.clip = FireClip;
        MainAS.Play();
    }

    #endregion

    #endregion




}
