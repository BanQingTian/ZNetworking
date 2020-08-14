using UnityEngine;
using NRKernal;

public class ChangeRenderModeTest : MonoBehaviour
{
    public void Change0DofMode()
    {
        NRRenderer.ChangeRenderMode(NativeRenderMode.RENDER_MODE_0DOF);
    }

    public void Change3DofMode()
    {
        NRRenderer.ChangeRenderMode(NativeRenderMode.RENDER_MODE_3DOF);
    }

    public void Change6DofMode()
    {
        NRRenderer.ChangeRenderMode(NativeRenderMode.RENDER_MODE_6DOF);
    }
}
