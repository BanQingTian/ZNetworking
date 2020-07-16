using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class sunSlider : MonoBehaviour {
	public Slider mainSlider;
	public Vector3 rotateAxis = new Vector3(1f,0.2f,0f);
	public void sun () {
		float value2 = (mainSlider.value) * 2F;
		if (mainSlider.value>0.5) {//when slider value is greater than 1, value 2 pingpong backward instead of growing greater.
			value2 = (1F-value2)*2F+value2;
		}
		Light light = GetComponent<Light> ();
		light.intensity = Mathf.Clamp(value2,0.001f,3f);
		RenderSettings.ambientIntensity = Mathf.Clamp(value2 * 2f,0.001f,3f);
		//transform.eulerAngles = new Vector3 (mainSlider.value*360f-90f,transform.eulerAngles.y,transform.eulerAngles.z);
		transform.rotation = Quaternion.AngleAxis(mainSlider.value*360f-90f, rotateAxis);
	}
	
	void Update() {
		if (Input.GetKey(KeyCode.UpArrow)){
			if (mainSlider.value >= 1f) {
				mainSlider.value = 0.001f;
			} else {
				mainSlider.value += 0.01f;
			}
		}
		if (Input.GetKey(KeyCode.DownArrow)){
			if (mainSlider.value <= 0f) {
				mainSlider.value = 1f;
			} else {
				mainSlider.value -= 0.01f;
			}
		}
	}
}