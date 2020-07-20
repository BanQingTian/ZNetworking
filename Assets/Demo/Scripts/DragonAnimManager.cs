using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DragonAnimManager : MonoBehaviour {

    // 进程动画
    public PlayableDirector MainTimeline;

    public Animator DragonAnimator;

    public GameObject DeathEffSwitch;

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
        DragonFire();
    }
    private void DragonFire()
    {
        DragonAnimator.gameObject.SetActive(true);
    }


    public void ResetStatue()
    {
        DragonAnimator.SetBool("dead", false);
        DeathEffSwitch.SetActive(false);
    }
    public void PlayDeadAnim()
    {
        DragonAnimator.SetBool("dead", true);
    }
    public void PlayDeathEff()
    {
        DeathEffSwitch.SetActive(true);
    }
}
