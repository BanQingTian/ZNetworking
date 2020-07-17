using UnityEngine;

public class assetUrl : MonoBehaviour {
	public string link;
	public void url () {
		Application.OpenURL(link);
	}
}