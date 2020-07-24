using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DragonAnimManager : MonoBehaviour {

    // 进程动画
    public PlayableDirector MainTimeline;

    public Animator DragonAnimator;

    public GameObject DeathEffSwitch;

    public GameObject Roof;

    private DragonAttack dragon;

    public void Init(DragonAttack da)
    {
        dragon = da;
    }

    // 龙 入场动画
    public void PlayDragonEnterAnim()
    {
        MainTimeline.gameObject.SetActive(true);
        DragonAnimator.gameObject.SetActive(false);
        MainTimeline.Play();
        double time = MainTimeline.duration;
        StartCoroutine(MainTimelineEnd((float)time));
    }
    private IEnumerator MainTimelineEnd(float time)
    {
        yield return new WaitForSeconds(time);
        MainTimeline.gameObject.SetActive(false);
        DragonAnimator.gameObject.SetActive(true);

    }

    public void DragonFire()
    {
        if (dragon.dead) return;

        //DragonAnimator.SetBool("fireball", true);
        DragonAnimator.SetTrigger("fireball");
    }


    public void ResetStatue()
    {
        DragonAnimator.SetBool("dead", false);
        DragonAnimator.SetBool("fireball", false);
        DeathEffSwitch.SetActive(false);
    }
    public void PlayDeadAnim()
    {
        DragonAnimator.SetBool("dead", true);
    }
    public void PlayDeathEff()
    {
        DeathEffSwitch.SetActive(true);
        StartCoroutine(roofAppear());
    }
    private IEnumerator roofAppear()
    {
        yield return new WaitForSeconds(40);
        Roof.SetActive(true);
        yield return new WaitForSeconds(10);
        if (ZClient.Instance.IsHouseOwner)
        {
            ZMessageManager.Instance.SendMsg(MsgId.__RESET_GAME_MSG_, "go");
        }
    }
}
