using UnityEngine;
using System.Collections;

public class limitFramerate : MonoBehaviour {
	public int fps = 60;
    void Awake() {
        Application.targetFrameRate = fps;
    }
}