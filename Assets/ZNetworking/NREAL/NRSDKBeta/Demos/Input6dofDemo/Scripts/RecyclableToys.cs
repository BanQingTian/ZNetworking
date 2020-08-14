namespace NRKernal.NRExamples
{
    using UnityEngine;

    public class RecyclableToys : NRGrabbableObject
    {
        private Vector3 m_OriginPos;
        private Quaternion m_OriginRot;
        private float m_MinPositionY = -1.8f;

        void Start()
        {
            m_OriginPos = transform.position;
            m_OriginRot = transform.rotation;
        }

        void Update()
        {
            if (transform.position.y < m_MinPositionY)
                RecycleSelf();
        }

        private void RecycleSelf()
        {
            transform.position = m_OriginPos;
            transform.rotation = m_OriginRot;
            m_AttachedRigidbody.velocity = Vector3.zero;
            m_AttachedRigidbody.angularVelocity = Vector3.zero;
        }
    }
}