using UnityEngine;
using System.Collections;

public class RefractionEffect : MonoBehaviour
{
	public GameObject[] m_RefractionObjects;
	private RenderTexture m_RTScene = null;
	private Camera m_RTCamera = null;
	private Shader m_SdrRefractionMesh = null;
	private Shader m_SdrRefractionMeshAlpha = null;
	private bool m_ToggleBump = true;

	void Start ()
	{
		m_RTScene = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
		m_SdrRefractionMesh = Shader.Find ("Crystal Glass/Refraction/Mesh");
		m_SdrRefractionMeshAlpha = Shader.Find ("Crystal Glass/Refraction/Mesh Alpha");
		Shader.EnableKeyword ("CRYSTAL_GLASS_BUMP");
	}
	void Update ()
	{
		Camera camCurr = Camera.main;
		if (m_RTCamera == null)
		{
			GameObject go = new GameObject ("RTCamera", typeof(Camera), typeof(Skybox));
			m_RTCamera = go.GetComponent<Camera>();
			go.transform.parent = camCurr.transform;
		}
		m_RTCamera.CopyFrom (camCurr);
		m_RTCamera.enabled = false;
		if (m_RTCamera.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox skyCurr = camCurr.GetComponent (typeof (Skybox)) as Skybox;
			Skybox skyRT = m_RTCamera.GetComponent (typeof (Skybox)) as Skybox;
			skyRT.enabled = true;
			skyRT.material = skyCurr.material;
		}

		// render all non refract objects to scene render target
		for (int i = 0; i < m_RefractionObjects.Length; i++)
		{
			Renderer rd = m_RefractionObjects[i].GetComponent<Renderer>();
			rd.material.shader = m_SdrRefractionMeshAlpha;
		}
		m_RTCamera.targetTexture = m_RTScene;
		m_RTCamera.Render ();
		for (int i = 0; i < m_RefractionObjects.Length; i++)
		{
			Renderer rd = m_RefractionObjects[i].GetComponent<Renderer>();
			rd.material.shader = m_SdrRefractionMesh;
			rd.material.SetTexture ("_SceneTex", m_RTScene);
		}
	}
	private void OnGUI()
	{
		GUI.Label (new Rect (10, 10, 250, 30), "Crystal Glass Demo");
//		m_ToggleBump = GUI.Toggle (new Rect(10, 50, 120, 30), m_ToggleBump, "Bump Mapping");
		if (m_ToggleBump)
			Shader.EnableKeyword ("CRYSTAL_GLASS_BUMP");
		else
			Shader.DisableKeyword ("CRYSTAL_GLASS_BUMP");
	}
}