using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSoundManager : MonoBehaviour
{

    [Header("------------Audio-----------")]
    // Audio
    public AudioSource MainAS;
    public AudioSource BGAS;
    public AudioClip FlapWing_Clip; // 扇翅膀
    public AudioClip FlapWing_Fire_Clip; //  扇翅膀&喷火
    public AudioClip Dragon_Death_Clip;
    public AudioClip Rose_Clip;


    public static DragonSoundManager Instance;

    // Use this for initialization
    void Start()
    {
        Instance = this;
    }
    #region Audio Event

    // 扇翅膀音效
    public void PlayWingReadyAudio()
    {
        MainAS.clip = FlapWing_Clip;
        MainAS.Play();
    }

    // 扇翅膀&喷火球动画音效
    public void PlayWingAndFireballAudio()
    {
        MainAS.clip = FlapWing_Fire_Clip;
        MainAS.Play();
    }

    public void PlayDeathAudio()
    {
        MainAS.clip = Dragon_Death_Clip;
        MainAS.Play();
    }

    public void PlayRoseAudio()
    {
        MainAS.Stop();
        BGAS.clip = Rose_Clip;
        BGAS.Play();
    }

    #endregion
}
