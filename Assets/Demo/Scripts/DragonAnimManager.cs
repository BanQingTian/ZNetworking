using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DragonAnimManager : MonoBehaviour {

    public const string flyFire = "fly_Fire";

    // 进程动画
    public PlayableDirector MainTimeline;

    public Animator DragonAnimator;


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



    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PlayDragonEnterAnim();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ZMessageManager.Instance.SendMsg(MsgId.__READY_PLAY_MSG_, string.Format("{0},{1}", ZClient.Instance.PlayerID, "aaaaaaaaa"));
        }

    }
}
