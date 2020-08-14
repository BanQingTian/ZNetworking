using UnityEngine;
using UnityEngine.UI;
using NRKernal;
using System.IO;
using NRKernal.Persistence;

public class PointCloudTool : MonoBehaviour
{
    public Slider m_PointSizeSlider;
    private NRPointCloudCreator creator;
    private NRPointCloudVisualizer visualizer;

    public GameObject anchorLoader;

    void Start()
    {
        m_PointSizeSlider.onValueChanged.AddListener(OnScaleChange);
        visualizer = new NRPointCloudVisualizer();

        anchorLoader.SetActive(false);
    }

    public void Open()
    {
        if (creator != null)
        {
            return;
        }
        creator = NRPointCloudCreator.Create(visualizer);
    }

    public void Save()
    {
        if (creator == null)
        {
            return;
        }
        string path = Path.Combine(Application.persistentDataPath, "NrealMaps");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        path = Path.Combine(path, "nreal_map.dat");
        creator.Save(path);

        Invoke("AfterBuildMap", 2f);
    }

    public void Load()
    {
        anchorLoader.SetActive(true);
    }

    private void AfterBuildMap()
    {
        creator.Destroy();
    }

    public void OnScaleChange(float value)
    {
        visualizer?.AdjustPointSize(value * 20);
    }
}
