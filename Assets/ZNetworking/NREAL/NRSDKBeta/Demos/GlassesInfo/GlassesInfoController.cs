using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NRKernal;
using System.Collections;

public class GlassesInfoController : MonoBehaviour
{
    public Button m_Switch2dBtn;
    public Button m_Switch3dBtn;
    public Slider m_Slider;
    public Text m_GlassesInfo;

    public Text ForeHeadTemp;
    public Text TempleTemp;

    // Use this for initialization
    void Start()
    {
        m_Switch2dBtn.onClick.AddListener(new UnityAction(() =>
        {
            NRDevice.Instance.Mode = NativeGlassesMode.TwoD_1080;
        }));

        m_Switch3dBtn.onClick.AddListener(new UnityAction(() =>
        {
            NRDevice.Instance.Mode = NativeGlassesMode.ThreeD_1080;
        }));

        m_Slider.onValueChanged.AddListener((value) =>
        {
            NRDevice.Instance.Brightness = (int)value;
        });

        m_GlassesInfo.text = NRDevice.Instance.GlassesVersion;

        StartCoroutine(UpdateTemprature());
    }

    IEnumerator UpdateTemprature()
    {
        while (true)
        {
            ForeHeadTemp.text = NRDevice.Instance.NativeGlassesController.GetTemprature(NativeGlassesTemperaturePosition.TEMPERATURE_POSITION_GLASSES_FOREHEAD).ToString();
            TempleTemp.text = NRDevice.Instance.NativeGlassesController.GetTemprature(NativeGlassesTemperaturePosition.TEMPERATURE_POSITION_GLASSES_TEMPLE).ToString();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
