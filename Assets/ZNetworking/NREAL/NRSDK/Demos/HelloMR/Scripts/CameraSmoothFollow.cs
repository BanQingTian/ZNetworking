using UnityEngine;

namespace NRKernal.NRExamples
{
    public class CameraSmoothFollow : MonoBehaviour
    {
        [Header("Window Settings")]
        [SerializeField, Tooltip("What part of the view port to anchor the window to.")]
        private TextAnchor Anchor = TextAnchor.LowerCenter;
        [SerializeField, Range(0.0f, 100.0f), Tooltip("How quickly to interpolate the window towards its target position and rotation.")]
        private float FollowSpeed = 5.0f;
        private float defaultDistance;

        //default rotation at start
        private Vector2 defaultRotation = new Vector2(0f, 0f);
        private Quaternion HorizontalRotation;
        private Quaternion HorizontalRotationInverse;
        private Quaternion VerticalRotation;
        private Quaternion VerticalRotationInverse;

        [SerializeField, Tooltip("The offset from the view port center applied based on the window anchor selection.")]
        private Vector2 Offset = new Vector2(0.1f, 0.1f);

        void Start()
        {
            HorizontalRotation = Quaternion.AngleAxis(defaultRotation.y, Vector3.right);
            HorizontalRotationInverse = Quaternion.Inverse(HorizontalRotation);
            VerticalRotation = Quaternion.AngleAxis(defaultRotation.x, Vector3.up);
            VerticalRotationInverse = Quaternion.Inverse(VerticalRotation);

            defaultDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        }

        private void LateUpdate()
        {
            float t = Time.deltaTime * FollowSpeed;
            transform.position = Vector3.Lerp(transform.position, CalculatePosition(Camera.main.transform), t);
            transform.rotation = Quaternion.Slerp(transform.rotation, CalculateRotation(Camera.main.transform), t);
        }

        private Vector3 CalculatePosition(Transform cameraTransform)
        {
            Vector3 position = cameraTransform.position + (cameraTransform.forward * defaultDistance);
            Vector3 horizontalOffset = cameraTransform.right * Offset.x;
            Vector3 verticalOffset = cameraTransform.up * Offset.y;

            switch (Anchor)
            {
                case TextAnchor.UpperLeft: position += verticalOffset - horizontalOffset; break;
                case TextAnchor.UpperCenter: position += verticalOffset; break;
                case TextAnchor.UpperRight: position += verticalOffset + horizontalOffset; break;
                case TextAnchor.MiddleLeft: position -= horizontalOffset; break;
                case TextAnchor.MiddleRight: position += horizontalOffset; break;
                case TextAnchor.LowerLeft: position -= verticalOffset + horizontalOffset; break;
                case TextAnchor.LowerCenter: position -= verticalOffset; break;
                case TextAnchor.LowerRight: position -= verticalOffset - horizontalOffset; break;
            }

            return position;
        }

        private Quaternion CalculateRotation(Transform cameraTransform)
        {
            Quaternion rotation = cameraTransform.rotation;

            switch (Anchor)
            {
                case TextAnchor.UpperLeft: rotation *= HorizontalRotationInverse * VerticalRotationInverse; break;
                case TextAnchor.UpperCenter: rotation *= HorizontalRotationInverse; break;
                case TextAnchor.UpperRight: rotation *= HorizontalRotationInverse * VerticalRotation; break;
                case TextAnchor.MiddleLeft: rotation *= VerticalRotationInverse; break;
                case TextAnchor.MiddleRight: rotation *= VerticalRotation; break;
                case TextAnchor.LowerLeft: rotation *= HorizontalRotation * VerticalRotationInverse; break;
                case TextAnchor.LowerCenter: rotation *= HorizontalRotation; break;
                case TextAnchor.LowerRight: rotation *= HorizontalRotation * VerticalRotation; break;
            }

            return rotation;
        }
    }
}