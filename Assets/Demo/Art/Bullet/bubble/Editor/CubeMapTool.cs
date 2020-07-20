using UnityEngine;
using UnityEditor;

public class CubeMapTool : EditorWindow
{
    private Cubemap cubeMap = null;

    [MenuItem("Tools/Cube Map Generate")]
    public static void GenerateCubeMap()
    {
        GetWindow<CubeMapTool>();
    }

    private void OnGUI()
    {
        cubeMap = EditorGUILayout.ObjectField(cubeMap, typeof(Cubemap), false, GUILayout.Width(400)) as Cubemap;
        if (GUILayout.Button("Render To Cube Map"))
        {
            SceneView.lastActiveSceneView.camera.RenderToCubemap(cubeMap);
        }
    }
}