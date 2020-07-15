using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zrime;

public class ZGameManager : MonoBehaviour {

    public static ZGameManager Instance;

    public PlayerEntity PlayerPrafab;

    [HideInInspector]
    public PlayerEntity OwnerTarget;
    [HideInInspector] 
    public Vector3[] OwnerPose = new Vector3[2]; // 通过maker识别获取pose

    private void Awake()
    {
        Instance = this;
    }


    void Update () {

	}

    
}
