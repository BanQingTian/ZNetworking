using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using GoogleARCore;
using GoogleARCore.Examples.AugmentedImage;

public class ZScanMarker : MonoBehaviour
{
    public static Matrix4x4 world_in_marker;

    public GameObject MarkerPrefab;

    private Transform transformParent;

    public bool MarkerTrackingUpdate()
    {
#if UNITY_EDITOR
        return true;
#else
        return Global.DeviceType == DeviceTypeEnum.NRLight ? NRLightMarkerTrackingUpdate() : ARCoreMarkerTrackingUpdate();
#endif

    }


    int scanCount = 0;
    List<NRTrackableImage> m_NRLightTrackingImages = new List<NRTrackableImage>();
    private bool NRLightMarkerTrackingUpdate()
    {
        NRFrame.GetTrackables<NRTrackableImage>(m_NRLightTrackingImages, NRTrackableQueryFilter.All);
        foreach (var item in m_NRLightTrackingImages)
        {
            if (item.GetTrackingState() == NRKernal.TrackingState.Tracking)
            {
                MarkerPrefab.SetActive(true);
                MarkerPrefab.transform.position = item.GetCenterPose().position;
                MarkerPrefab.transform.rotation = item.GetCenterPose().rotation;
                MarkerPrefab.transform.localScale = new Vector3(item.Size.x, MarkerPrefab.transform.localScale.y, item.Size.y);

                var marker_in_world = ZUtils.GetTMatrix(item.GetCenterPose().position, item.GetCenterPose().rotation);//ZUtils.GetTMatrix(item.GetCenterPose().position, item.GetCenterPose().rotation);
                world_in_marker = Matrix4x4.Inverse(marker_in_world);

                DragonManager.Instance.FreshScanMarkerClkEnterTip(true);

                if (NRInput.GetButtonDown(ControllerButton.TRIGGER))
                {
                    DragonManager.Instance.FreshScanMarkerClkEnterTip(false);

                    MarkerPrefab.SetActive(false);

                    GameObject nrCam = GameObject.Find("NRCameraRig");
                    //GameObject nrInput = GameObject.Find("NRInput");

                    TranslatePose_NR(nrCam.transform);

                    NRInput.RecenterController();

                    SwitchImageTrackingMode(false); // 关闭maker识别

                    return true;
                }

                return false;
            }
            else if (item.GetTrackingState() == NRKernal.TrackingState.Stopped)
            {
                MarkerPrefab.SetActive(false);
                DragonManager.Instance.ShowScanMarkerTip();
            }
        }
        MarkerPrefab.SetActive(false);
        DragonManager.Instance.ShowScanMarkerTip();
        return false;

    }

    public void SwitchImageTrackingMode(bool on)
    {
        var config = NRSessionManager.Instance.NRSessionBehaviour.SessionConfig;
        config.ImageTrackingMode = on ? TrackableImageFindingMode.ENABLE : TrackableImageFindingMode.DISABLE;
        NRSessionManager.Instance.SetConfiguration(config);
    }



    private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();
    private bool ARCoreMarkerTrackingUpdate()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.Updated);

        foreach (var item in m_TempAugmentedImages)
        {
            if (item.TrackingState == GoogleARCore.TrackingState.Tracking)
            {
                //Anchor anchor = item.CreateAnchor(item.CenterPose);
                MarkerPrefab.SetActive(true);
                MarkerPrefab.transform.position = item.CenterPose.position;// anchor.transform.position;
                MarkerPrefab.transform.rotation = item.CenterPose.rotation;// anchor.transform.rotation;
                MarkerPrefab.transform.localScale = new Vector3(0.4f, MarkerPrefab.transform.localScale.y, 0.4f);

                DragonManager.Instance.FreshScanMarkerClkEnterTip(true);

                GameObject arcore = GameObject.Find("ARCore Device");
                arcore.transform.position = Vector3.zero;
                arcore.transform.rotation = Quaternion.identity;

                if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)
                {

                    DragonManager.Instance.FreshScanMarkerClkEnterTip(false);

                    var marker_in_world = ZUtils.GetTMatrix(item.CenterPose.position, item.CenterPose.rotation);
                    world_in_marker = Matrix4x4.Inverse(marker_in_world);

                    GameObject arcoreCam = GameObject.Find("First Person Camera");
                    
                    TranslatePose(arcoreCam.transform, arcore.transform);



                    MarkerPrefab.SetActive(false);

                    return true;
                }
                scanCount++;

                return false;
            }
            else if (item.TrackingState == GoogleARCore.TrackingState.Stopped)
            {
                MarkerPrefab.SetActive(false);
                DragonManager.Instance.ShowScanMarkerTip();
            }
        }
        MarkerPrefab.SetActive(false);
        DragonManager.Instance.ShowScanMarkerTip();

        return false;
    }
    private void TranslatePose_NR(Transform camera, Transform endParent = null)
    {
        if (transformParent == null)
        {
            transformParent = new GameObject("---TransformParent---").transform;
        }

        transformParent.position = camera.position;
        transformParent.rotation = camera.rotation;

        camera.SetParent(transformParent);

        transformParent.position = ZUtils.GetPositionFromTMatrix(world_in_marker);
        transformParent.rotation = ZUtils.GetRotationFromTMatrix(world_in_marker);

        if (endParent != null)
        {
            endParent.position = ZUtils.GetPositionFromTMatrix(world_in_marker);
            endParent.rotation = ZUtils.GetRotationFromTMatrix(world_in_marker);
        }

        //camera.SetParent(endParent);
    }
    private void TranslatePose(Transform camera, Transform endParent = null)
    {
        if (transformParent == null)
        {
            transformParent = new GameObject("---TransformParent---").transform;
        }

        transformParent.position = camera.position;
        transformParent.rotation = camera.rotation;

        camera.SetParent(transformParent);

        transformParent.position = ZUtils.GetPositionFromTMatrix(world_in_marker);
        transformParent.rotation = ZUtils.GetRotationFromTMatrix(world_in_marker);

        if (endParent != null)
        {
            endParent.position = ZUtils.GetPositionFromTMatrix(world_in_marker);
            endParent.rotation = ZUtils.GetRotationFromTMatrix(world_in_marker);
        }

        camera.SetParent(endParent);
    }


}
