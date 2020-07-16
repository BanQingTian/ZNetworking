using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class globalVolumeTuner : MonoBehaviour {
	public Slider mainSlider;
	void Start() {
		AudioListener.volume = 0f;
	}
    public void slider() {
        AudioListener.volume = mainSlider.value;
    }
}