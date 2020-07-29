using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

    public Transform t1;
    public Transform t2;

	// Use this for initialization
	void Start () {
        test();
	}
	void test()
    {
        Debug.Log(5 >> 1);
        Debug.Log(5 >> 2);
    }
    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Vector3 dir = t2.position - t1.position;
            Debug.Log(dir.normalized);
            Debug.Log(t1.forward);
            Debug.Log(ZUtils.GetAngle(dir.normalized,t1.forward));
        }
	}
}
