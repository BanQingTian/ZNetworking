using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateScaleToDistanceFromTarget : MonoBehaviour {
    public Transform target;
    [Header("最小缩放")]
    public float MinScale = 1f;
    [Header("最大缩放")]
    public float MaxScale = 2f;

    [Header("最小距离")]
    public float MinDistance = 0.1f;
    [Header("最大距离")]
    public float MaxDistance = 0.2f;

	// Use this for initialization
	void Start () {
        if (ValleyCenter.Instance == null)
        {
            transform.localScale = Vector3.one * 10f;
            Destroy(this);
            return;
        }
		target = ValleyCenter.Instance.center;
		if(target == null)
        {
            Debug.LogError("Target is null, please set target!");
            Destroy(gameObject);
        }
        if(MaxDistance == MinDistance)
        {
            Debug.LogError("MinDistance and maxdistance can not be the same");
            Destroy(gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
        UpdateLocalScale();
    }

    private void UpdateLocalScale()
    {
        transform.localScale = GetCurrentLocalScale();
    }

    private Vector3 GetCurrentLocalScale()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        distance = Mathf.Clamp(distance, MinDistance, MaxDistance);
        float scale = MinScale + (distance - MinDistance) * (MaxScale - MinScale) / (MaxDistance - MinDistance);
        return Vector3.one * scale;
    }
}
