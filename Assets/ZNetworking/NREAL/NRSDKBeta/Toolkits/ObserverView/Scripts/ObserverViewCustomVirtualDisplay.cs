using UnityEngine;
using UnityEngine.UI;

public class ObserverViewCustomVirtualDisplay : MonoBehaviour
{
    public Button showBtn;
    public Button hideBtn;
    public GameObject baseControllerPanel;

    private void Start()
    {
        showBtn.onClick.AddListener(() => { SetBaseControllerEnabled(true); });
        hideBtn.onClick.AddListener(() => { SetBaseControllerEnabled(false); });
    }

    private void SetBaseControllerEnabled(bool isEnabled)
    {
        baseControllerPanel.SetActive(isEnabled);
    }
}
