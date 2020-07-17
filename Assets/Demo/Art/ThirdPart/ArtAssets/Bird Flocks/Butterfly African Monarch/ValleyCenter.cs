using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValleyCenter : MonoBehaviour {
	public Transform center;

	private static ValleyCenter _instance;
	public static ValleyCenter Instance{
		get{
			return _instance;
		}
	}

	private void Awake()
	{
		_instance = this;
	}
}
