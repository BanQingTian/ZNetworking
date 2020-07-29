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
    public ParticleSystem BoomEff;
    public Rigidbody Rig;
    public Transform DragonParent;

    public bool dead = false;
    public bool dragonFire = false;

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
        gameObject.SetActive(false); // begin play animation
        DragonBubble.gameObject.SetActive(false);
        AnimManager.Init(this);
    }

    public void Init(int playerCount)
    {
        firstBeHit = true;
        dead = false;
        dragonFire = false;
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



    /// <summary>
    /// 动画事件调用 - 死亡动画开始播放时
    /// </summary>
    public void DragonDeathBegin()
    {
        DragonBubble.gameObject.SetActive(false);

        StopCoroutine("rotateHeadCor");
        StartCoroutine(rotateHeadCor(DragonParent, 180, 40, () =>
        {

        }));
    }

    /// <summary>
    /// 动画事件调用 - 死亡动画播放完播放特效
    /// </summary>
    public void DragonDeathEff()
    {
        Debug.Log("DeathEff Playing");

        //DragonParent.eulerAngles = new Vector3(0, 90, 0);



        AnimManager.PlayDeathEff();
        gameObject.SetActive(false);

    }

    /// <summary>
    /// 龙准备喷火
    /// </summary>
    public void DragonFireBegin()
    {
        dragonFire = true;
    }
    public void DragonFireEnd()
    {
        dragonFire = false;
        //RandomTarget();
        if (ZClient.Instance.IsHouseOwner && !dragonFire)
        {
            int randonkey = Random.Range(0, ZPlayerMe.Instance.PlayerKeys.Count);
            if (randonkey >= ZPlayerMe.Instance.PlayerKeys.Count)
            {
                randonkey = 0;
            }
            ZMessageManager.Instance.SendMsg(MsgId.__RANDOM_PLAYER_MSG_, ZPlayerMe.Instance.PlayerKeys[randonkey]);
        }
    }

    /// <summary>
    /// 动画事件调用-喷火球
    /// </summary>
    public void DragonShootFireball()
    {
        PlayFireballAudio();
        Rig.isKinematic = true;
        Rig.isKinematic = false;
        Fireball.SetActive(true);
        Fireball.transform.position = AttackPoint.transform.position;
        Rig.AddForce((targetPos - Fireball.transform.position).normalized * 300);

        //PlayFireballAudio();
        //Rig.isKinematic = true;
        //Rig.isKinematic = false;
        //Fireball.transform.position = AttackPoint.transform.position;
        //Fireball.transform.rotation = AttackPoint.transform.rotation;
        //Fireball.SetActive(true);
        //Rig.AddForce(AttackPoint.transform.forward * 600);
    }

    /// <summary>
    /// 翅膀音效
    /// </summary>
    public void PlayWing()
    {
        PlayWingReadyAudio();
    }

    /// <summary>
    /// 开启旋转跟随协程
    /// </summary>
    public void StartFollow()
    {
        Debug.Log("StartFollow");
        DragonManager.Instance.BeginFight();
        //RandomTarget();
        if (ZClient.Instance.IsHouseOwner && !dragonFire)
        {
            int randonkey = Random.Range(0, ZPlayerMe.Instance.PlayerKeys.Count);
            if (randonkey >= ZPlayerMe.Instance.PlayerKeys.Count)
            {
                randonkey = 0;
            }
            ZMessageManager.Instance.SendMsg(MsgId.__RANDOM_PLAYER_MSG_, ZPlayerMe.Instance.PlayerKeys[randonkey]);
        }
        StartCoroutine(look());
    }

    Vector3 targetPos;
    float randomAngle;
    bool randomed = false;
    // 随机获取一名玩家位置, server同步随机到的玩家
    public void RandomTarget(string key)
    {
        randomed = true;
        targetPos = ZPlayerMe.Instance.PlayerMap[key].transform.position;

        Vector3 v1 = new Vector3(targetPos.x, 0, targetPos.z) - new Vector3(DragonParent.position.x, 0, DragonParent.position.z);
        randomAngle = ZUtils.GetAngle(v1, DragonParent.right);
    }
    private IEnumerator look()
    {
        while (true)
        {
            if (dead)
            {
                //StartCoroutine(rotateHeadCor(DragonParent, 180, 40));

                //Vector3 v1 = new Vector3(targetPos.x, 0, targetPos.z) - new Vector3(DragonParent.position.x, 0, DragonParent.position.z);
                //float a = Vector3.Angle(v1, DragonParent.forward);
                //if (a > 2)
                //{
                //    ZUtils.Look(true, DragonParent, Vector3.zero, randomAngle);
                //}
                //else
                //{
                //    ZUtils.Look(true, DragonParent, targetPos, randomAngle);

                //}
                yield break;
            }

            if (!randomed)
            {
                yield return null;
            }

            if (!dragonFire)
            {
                Vector3 v1 = new Vector3(targetPos.x, 0, targetPos.z) - new Vector3(DragonParent.position.x, 0, DragonParent.position.z);
                float a = Vector3.Angle(v1, DragonParent.forward);
                if (a > 2)
                {
                    ZUtils.Look(true, DragonParent, targetPos, randomAngle);
                }
                else
                {
                    ZUtils.Look(true, DragonParent, targetPos, randomAngle);
                    AnimManager.DragonFire();
                }

            }
            yield return null;
        }
    }
    private IEnumerator rotateHeadCor(Transform startTrans, float end, float speed = 1, System.Action finishi = null)
    {
        Debug.Log("end = " + end);

        float dir = Time.deltaTime * 4 * speed;
        dir = end > startTrans.eulerAngles.y ? dir : -dir;
        while (Mathf.Abs(startTrans.eulerAngles.y - end) > Mathf.Abs(dir * 1.5f))
        {
            Vector3 v3 = DragonParent.transform.eulerAngles;
            float y = v3.y + dir;
            startTrans.transform.eulerAngles = new Vector3(v3.x, y, v3.z);

            yield return null;
        }
        //AnimManager.DragonAnimator.SetTrigger("fireball");
        startTrans.eulerAngles = new Vector3(DragonParent.transform.eulerAngles.x, end, DragonParent.transform.eulerAngles.z);
        finishi?.Invoke();

        Debug.Log("jiajioa === " + Vector3.Angle(startTrans.forward, getplyaer().position - startTrans.position));
    }


    /// <summary>
    /// 播放喷火球动画
    /// </summary>
    public void PlayFireAnim()
    {

    }

    private Transform getplyaer()
    {
        int index = Random.Range(0, ZPlayerMe.Instance.PlayerKeys.Count);
        return ZPlayerMe.Instance.PlayerMap[ZPlayerMe.Instance.PlayerKeys[index]].transform;
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
