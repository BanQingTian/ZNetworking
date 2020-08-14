using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class NREventCenter {
    public static GameObject GetCurrentRaycastTarget()
    {
        NRPointerRaycaster raycaster = GetRaycaster(NRInput.DomainHand);
        if (raycaster == null)
            return null;
        var result = raycaster.FirstRaycastResult();
        if (!result.isValid)
            return null;
        return result.gameObject;
    }

    private static Dictionary<ControllerAnchorEnum, NRPointerRaycaster> raycasterDict;

    public static NRPointerRaycaster GetRaycaster(ControllerHandEnum handEnum)
    {
        if (raycasterDict == null)
        {
            raycasterDict = new Dictionary<ControllerAnchorEnum, NRPointerRaycaster>();
            NRPointerRaycaster _raycaster = NRInput.AnchorsHelper.GetAnchor(ControllerAnchorEnum.GazePoseTrackerAnchor).GetComponentInChildren<NRPointerRaycaster>(true);
            raycasterDict.Add(ControllerAnchorEnum.GazePoseTrackerAnchor, _raycaster);
            _raycaster = NRInput.AnchorsHelper.GetAnchor(ControllerAnchorEnum.RightLaserAnchor).GetComponentInChildren<NRPointerRaycaster>(true);
            raycasterDict.Add(ControllerAnchorEnum.RightLaserAnchor, _raycaster);
            _raycaster = NRInput.AnchorsHelper.GetAnchor(ControllerAnchorEnum.LeftLaserAnchor).GetComponentInChildren<NRPointerRaycaster>(true);
            raycasterDict.Add(ControllerAnchorEnum.LeftLaserAnchor, _raycaster);
        }
        NRPointerRaycaster raycaster = null;
        if (NRInput.RaycastMode == RaycastModeEnum.Gaze)
            raycasterDict.TryGetValue(ControllerAnchorEnum.GazePoseTrackerAnchor, out raycaster);
        else if(NRInput.DomainHand == ControllerHandEnum.Right)
            raycasterDict.TryGetValue(ControllerAnchorEnum.RightLaserAnchor, out raycaster);
        else if (NRInput.DomainHand == ControllerHandEnum.Left)
            raycasterDict.TryGetValue(ControllerAnchorEnum.LeftLaserAnchor, out raycaster);
        return raycaster;
    }
}
