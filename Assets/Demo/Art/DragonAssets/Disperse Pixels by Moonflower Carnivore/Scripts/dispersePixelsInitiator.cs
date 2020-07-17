using UnityEngine;
using System.Collections;

public class dispersePixelsInitiator : MonoBehaviour
{
	[Tooltip("Interval in second each Disperse Pixels script is executed. Zero interval will often result in the Disperse particles failed to be tinted, because the added latency of capturing many screen shots all at once is too much to allow the particles to be tinted properly.")]
	public float executionInterval = 0.01f;
	public dispersePixels[] dispersePixelsObjects;
	uint seed;
	void OnEnable() {
		for (int i = 0; i < dispersePixelsObjects.Length; i++) {
			StartCoroutine(executeDispersePixels(i));
		}
	}
	IEnumerator executeDispersePixels(int i) {
		yield return new WaitForSeconds(i * executionInterval);
		dispersePixelsObjects[i].enabled = true;
		yield return null;
	}
	void OnDisable() {
		foreach (dispersePixels dp in dispersePixelsObjects) {
			dp.enabled = false;
		}
	}
}