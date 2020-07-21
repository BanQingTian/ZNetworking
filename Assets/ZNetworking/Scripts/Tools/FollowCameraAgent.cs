using UnityEngine;
//using DG.Tweening;

public class FollowCameraAgent : MonoBehaviour
{
    public enum FollowMode
    {
        NONE = 0,
        POSITION_ONLY,
        POSITION_AND_FOWARD,
        POSITION_AND_ROTATION
    }

    private Transform m_FollowAnchor;

    private Transform m_CenterCamera = null;
    private Transform CameraCenter { get { if (m_CenterCamera == null) m_CenterCamera = Camera.main.transform; return m_CenterCamera; } }

    public FollowMode CurrentFollowMode;

    protected void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha0))
            OnFollowCamera(FollowMode.POSITION_AND_ROTATION);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            OnFollowCamera(FollowMode.POSITION_ONLY);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            OnFollowCamera(FollowMode.NONE);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            OnFollowCamera(FollowMode.POSITION_AND_FOWARD);
#endif
        switch (CurrentFollowMode)
        {
            case FollowMode.NONE:
                break;
            case FollowMode.POSITION_ONLY:
                if (m_FollowAnchor)
                {
                    m_FollowAnchor.position = CameraCenter.position;
                }
                break;
            case FollowMode.POSITION_AND_FOWARD:
                if (m_FollowAnchor)
                {
                    m_FollowAnchor.position = CameraCenter.position;
                    m_FollowAnchor.eulerAngles = Vector3.up * CameraCenter.eulerAngles.y;
                }
                break;
            case FollowMode.POSITION_AND_ROTATION:
                if (m_FollowAnchor)
                {
                    m_FollowAnchor.position = CameraCenter.position;
                    m_FollowAnchor.rotation = CameraCenter.rotation;
                }
                break;
            default:
                break;
        }
    }

    public void OnFollowCamera(FollowMode followMode)
    {
        switch (followMode)
        {
            case FollowMode.NONE:
                CurrentFollowMode = followMode;
                break;
            case FollowMode.POSITION_AND_FOWARD:
                Transform lastAnchor = m_FollowAnchor;
                m_FollowAnchor = CreateFollowAnchor();
                m_FollowAnchor.eulerAngles = Vector3.up * CameraCenter.eulerAngles.y;
                transform.SetParent(m_FollowAnchor, true);
                ClearLastFollowAnchor(lastAnchor);
                CurrentFollowMode = followMode;
                break;
            case FollowMode.POSITION_ONLY:
            case FollowMode.POSITION_AND_ROTATION:
                ReplaceFollowAnchor();
                CurrentFollowMode = followMode;
                break;
            default:
                break;
        }
    }

    private void ReplaceFollowAnchor()
    {
        Transform lastAnchor = m_FollowAnchor;
        m_FollowAnchor = CreateFollowAnchor();
        transform.SetParent(m_FollowAnchor, true);
        ClearLastFollowAnchor(lastAnchor);
    }

    private Transform CreateFollowAnchor()
    {
        GameObject anchor = new GameObject("followNode");
        anchor.transform.position = CameraCenter.position;
        anchor.transform.rotation = CameraCenter.rotation;
        anchor.transform.localScale = Vector3.one;
        return anchor.transform;
    }

    private void ClearLastFollowAnchor(Transform lastAnchor)
    {
        if (lastAnchor == null || lastAnchor.gameObject == null)
            return;
        Destroy(lastAnchor.gameObject);
    }

    //public Transform rotationNode;
    //public float faceCameraDuration = 1f;

    //private bool m_IsFollowing = true;
    //private Vector3 m_FollowOffset;
    //private Tween m_MoveTween;

    //protected void Start()
    //{
    //    UpdateFollowOffset();
    //}

    //private void UpdateFollowOffset()
    //{
    //    m_FollowOffset = transform.position - cameraCenter.position;
    //}

    //public void OnFaceCamera()
    //{
    //    Vector3 endValue = Vector3.up * cameraCenter.eulerAngles.y;
    //    if (rotationNode)
    //    {
    //        rotationNode.DOKill();
    //        rotationNode.DORotate(endValue, faceCameraDuration);
    //    }
    //    if (m_MoveTween != null)
    //    {
    //        m_MoveTween.Kill();
    //        m_MoveTween = null;
    //    }
    //    m_FollowOffset = transform.position - cameraCenter.position;
    //    float rotateAngle = Mathf.Sign(Vector3.Cross(m_FollowOffset, cameraCenter.forward).y) * Vector3.Angle(m_FollowOffset, cameraCenter.forward);
    //    Vector3 targetOffset = Quaternion.AngleAxis(rotateAngle, Vector3.up) * m_FollowOffset;
    //    if (m_IsFollowing)
    //        m_MoveTween = DOTween.To(() => m_FollowOffset, x => m_FollowOffset = x, targetOffset, faceCameraDuration);
    //    else
    //        m_MoveTween = transform.DOMove(cameraCenter.position + targetOffset, faceCameraDuration);
    //}

    //public void OnFollowCamera(bool follow)
    //{
    //    if (follow)
    //    {
    //        m_IsFollowing = true;
    //        UpdateFollowOffset();
    //    }
    //    else
    //    {
    //        m_IsFollowing = false;
    //    }
    //}
}

