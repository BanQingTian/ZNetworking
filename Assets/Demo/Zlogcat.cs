using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zlogcat : MonoBehaviour {

    public Transform ARCoreCamera;
    public Transform ArCoreSession;
	
	// Update is called once per frame
	void Update () {
        Debug.Log("czlog " + ARCoreCamera.position);
        Debug.Log("czlog " + ArCoreSession.position);
    }
}
