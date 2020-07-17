using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class materialColor : MonoBehaviour {
	public Material[] materials;
	public Renderer[] rend;
	public ParticleSystem ps;
	public TrailRenderer[] trail;
	public Text[] text;
	Color color2;
	
	public void colorButtonWhite () {
		colorSwitch(new Color(1f,1f,1f,1f));
	}
	
	public void colorButtonBlack () {
		colorSwitch(new Color(0.25f,0.25f,0.25f,1f));
	}
	
	public void colorButtonRed () {
		colorSwitch(new Color(1f,0.25f,0.25f,1f));
	}
	
	public void colorButtonYellow () {
		colorSwitch(new Color(1f,1f,0.25f,1f));
	}
	
	public void colorButtonGreen () {
		colorSwitch(new Color(0.25f,1f,0.25f,1f));
	}
	
	public void colorButtonCyan () {
		colorSwitch(new Color(0.25f,1f,1f,1f));
	}
	
	public void colorButtonBlue () {
		colorSwitch(new Color(0.25f,0.25f,1f,1f));
	}
	
	public void colorButtonPurple () {
		colorSwitch(new Color(1f,0.25f,1f,1f));
	}
	/*
	void OnDisable() {
		colorSwitch(new Color(1f,1f,1f,1f));
	}
	*/
	void colorSwitch(Color color) {
		for (int i = 0; i < rend.Length; i++) {
			rend[i].sharedMaterial=materials[0];
		}
		for (int i = 0; i < materials.Length; i++) {
			materials[i].SetColor("_Color", color);
		}
		if (color == new Color(1f,1f,1f,1f)) {
			color2 = color;
		} else if (color == new Color(0f,0f,0f,1f)) {
			color2 = new Color(1f,1f,1f,1f);
		} else {
			color2 = new Color(Mathf.Clamp(color.r + 0.25f, 0f, 1f), Mathf.Clamp(color.g + 0.25f, 0f, 1f), Mathf.Clamp(color.b + 0.25f, 0f, 1f), 1f);
		}
		ParticleSystem.MainModule psmain = ps.main;
		psmain.startColor = color2;
		for (int i = 0; i < trail.Length; i++) {
			trail[i].startColor = color2;
			trail[i].endColor = color;
		}
		for (int i = 0; i < text.Length; i++) {
			text[i].color = color;
		}
	}
}