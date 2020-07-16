using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class toggleControl : MonoBehaviour {
	public dispersePixels disperseScript;
	public Camera mainCamera;
	public int count = 0;
	public float[] executionInterval;
	public GameObject[] effects;
	//public bool[] chromaKeying = new bool[1]{true};
	public float[] maxPartilceMultiplier = new float[1]{1f};
	public enum FOM{none=0,outInstantly=1,outByTransparencyTintColor=2,outByTransparencyColor=3,outByCutoff=4,inInstantly=5,inByTransparencyTintColor=6,inByTransparencyColor=7,inByCutoff=8,outInInstantly=9,outInByTransparencyTintColor=10,outInByTransparencyColor=11,outInByCutoff=12};
	public FOM[] fadingMode = new FOM[1]{FOM.outInstantly};
	public float[] fadingDelay;
	public float[] fadingDuration;
	public float[] outInDuration;
	public Material[] materialSubstitute;
	public AudioClip[] audioClip;
	public string[] label;
	public Text toggleText;
	public Toggle toggleButton;
	public Button prevButton;
	public Button nextButton;
	Animator animator;
	public Toggle animationToggle;
	Vector3 randPos;
	Vector3 mousePos;
	public bool mobile;
	
	void Start(){
		animator = disperseScript.gameObject.GetComponent<Animator>();
		nextPrevAction(count);
		mousePos = disperseScript.gameObject.transform.position;
		if (!mainCamera) {
			mainCamera = Camera.main;
		}
	}
	
	void Update(){
		if (!mobile) {
			if (Input.GetKeyDown(KeyCode.Space)){
				//onOff();
				if (toggleButton.isOn) {
					toggleButton.isOn = false;
				} else {
					toggleButton.isOn = true;
				}
			}
			if (Input.GetKeyDown(KeyCode.RightArrow)){
				next();
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow)){
				prev();
			}/*
			if (Input.GetKeyDown(KeyCode.DownArrow)){
				next();
				next();
			}
			if (Input.GetKeyDown(KeyCode.UpArrow)){
				prev();
				prev();
			}*/
			if (Input.GetMouseButtonDown(0) && (fadingMode[count] == FOM.outInInstantly || fadingMode[count] == FOM.outInByTransparencyTintColor || fadingMode[count] == FOM.outInByTransparencyColor || fadingMode[count] == FOM.outInByCutoff)) {
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out hit)) {
					mousePos = new Vector3(Mathf.Clamp(hit.point.x, -3f,3f),0f,Mathf.Clamp(hit.point.z, -3f,3f));
					toggleButton.isOn = true;
				}
			}
		}
	}
	
	public void onOff() {
		//Debug.Log("triggered");
		if (toggleButton.isOn) {
			if (fadingMode[count] == FOM.outInInstantly || fadingMode[count] == FOM.outInByTransparencyTintColor || fadingMode[count] == FOM.outInByTransparencyColor || fadingMode[count] == FOM.outInByCutoff) {
				if (mousePos != disperseScript.gameObject.transform.position) {
					randPos = mousePos;
				} else {
					randPos = new Vector3(Mathf.Clamp(-disperseScript.gameObject.transform.position.x + Random.Range(-3f,3f),-2f,2f),0f,Mathf.Clamp(-disperseScript.gameObject.transform.position.z + Random.Range(-3f,3f),-2f,2f));
				}
				//effects[count].GetComponent<particleHomecoming>().newPositionVector = randPos;
				disperseScript.outInNewPosition = randPos;
				disperseScript.enabled = true;
				StartCoroutine(outInNewPos(count));
			} else {
				disperseScript.enabled = true;
			}
			if (!animationToggle.isOn) {
				disperseScript.animateAfterFadeIn = animationToggle.isOn;
				animator.enabled = animationToggle.isOn;
			}
		} else {
			disperseScript.enabled = false;
			disperseScript.animateAfterFadeIn = animationToggle.isOn;
			animator.enabled = animationToggle.isOn;
			if (fadingMode[count] == FOM.inInstantly || fadingMode[count] == FOM.inByTransparencyTintColor || fadingMode[count] == FOM.inByTransparencyColor || fadingMode[count] == FOM.inByCutoff) {
				animator.enabled = false;
			}
		}
	}
	
	IEnumerator outInNewPos(int i) {
		//yield return new WaitForSeconds(outInDuration[i] * 0.5f);
		float t = 0f;
		yield return new WaitForSeconds(0.3f);
		while (t < outInDuration[i]) {
			t += Time.deltaTime / (outInDuration[i] - 0.6f);
			disperseScript.gameObject.transform.position = Vector3.Lerp(disperseScript.gameObject.transform.position, randPos, t);
			yield return null;
		}
		//disperseScript.gameObject.transform.position = randPos;
		//yield return new WaitForSeconds(1f);
		toggleButton.isOn = false;
	}
	
	public void next(){
		disperseScript.enabled = false;
		if (count == effects.Length-1) {
			count = 0;
		} else {
			count++;
		}
		nextPrevAction(count);
	}
	
	public void prev(){
		disperseScript.enabled = false;
		if (count == 0) {
			count = effects.Length-1;
		} else {
			count--;
		}
		nextPrevAction(count);
	}
	
	void nextPrevAction(int i) {
		if (fadingMode[i] != FOM.outInInstantly || fadingMode[i] != FOM.outInByTransparencyTintColor || fadingMode[i] != FOM.outInByTransparencyColor || fadingMode[i] != FOM.outInByCutoff) {
			disperseScript.gameObject.transform.position = new Vector3(0f,0f,0f);
		}
		disperseScript.disperseParticle = effects[i].GetComponent<ParticleSystem>();
		//disperseScript.chromaKeying = chromaKeying[i];
		disperseScript.maxParticlesMultiplier = maxPartilceMultiplier[i];
		FOMConvert(i);
		if (materialSubstitute[i]) {
			disperseScript.materialSubstitute[0] = materialSubstitute[i];
			disperseScript.materialSubstitute[1] = materialSubstitute[i];
		}
		disperseScript.executionInterval = executionInterval[i];
		disperseScript.fadingDelay = fadingDelay[i];
		disperseScript.fadingDuration = fadingDuration[i];
		disperseScript.outInDuration = outInDuration[i];
		disperseScript.audioClip = audioClip[i];
		toggleText.text = label[i].ToString();
		disperseScript.enabled = false;
		toggleButton.isOn = false;
		disperseScript.animateAfterFadeIn = animationToggle.isOn;
		animator.enabled = animationToggle.isOn;
		if (fadingMode[i] == FOM.inInstantly || fadingMode[i] == FOM.inByTransparencyTintColor || fadingMode[i] == FOM.inByTransparencyColor || fadingMode[i] == FOM.inByCutoff) {
			for (int j = 0; j < disperseScript.targetObject.Length; j++) {
				disperseScript.targetObject[j].GetComponent<Renderer>().enabled = false;
			}
			animator.enabled = false;
		} else {
			for (int j = 0; j < disperseScript.targetObject.Length; j++) {
				disperseScript.targetObject[j].GetComponent<Renderer>().enabled = true;
			}
		}
	}
	
	void FOMConvert(int i) {
		if (fadingMode[i] == FOM.none) {
			disperseScript.fadingMode = dispersePixels.FOM.none;
		} else if (fadingMode[i] == FOM.outInstantly) {
			disperseScript.fadingMode = dispersePixels.FOM.outInstantly;
		} else if (fadingMode[i] == FOM.outByTransparencyTintColor) {
			disperseScript.fadingMode = dispersePixels.FOM.outByTransparencyTintColor;
		} else if (fadingMode[i] == FOM.outByTransparencyColor) {
			disperseScript.fadingMode = dispersePixels.FOM.outByTransparencyColor;
		} else if (fadingMode[i] == FOM.outByCutoff) {
			disperseScript.fadingMode = dispersePixels.FOM.outByCutoff;
		} else if (fadingMode[i] == FOM.inInstantly) {
			disperseScript.fadingMode = dispersePixels.FOM.inInstantly;
		} else if (fadingMode[i] == FOM.inByTransparencyTintColor) {
			disperseScript.fadingMode = dispersePixels.FOM.inByTransparencyTintColor;
		} else if (fadingMode[i] == FOM.inByTransparencyColor) {
			disperseScript.fadingMode = dispersePixels.FOM.inByTransparencyColor;
		} else if (fadingMode[i] == FOM.inByCutoff) {
			disperseScript.fadingMode = dispersePixels.FOM.inByCutoff;
		} else if (fadingMode[i] == FOM.outInInstantly) {
			disperseScript.fadingMode = dispersePixels.FOM.outInInstantly;
		} else if (fadingMode[i] == FOM.outInByTransparencyTintColor) {
			disperseScript.fadingMode = dispersePixels.FOM.outInByTransparencyTintColor;
		} else if (fadingMode[i] == FOM.outInByTransparencyColor) {
			disperseScript.fadingMode = dispersePixels.FOM.outInByTransparencyColor;
		} else if (fadingMode[i] == FOM.outInByCutoff) {
			disperseScript.fadingMode = dispersePixels.FOM.outInByCutoff;
		}
	}
}