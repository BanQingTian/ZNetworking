using System;
using System.Collections.Generic;
using UnityEngine;

namespace NRKernal.NRExamples
{

    public class ColoredPen : MonoBehaviour
    {
        public GameObject lineRendererPrefab;
        public Transform penPoint;
        public float lineWidth = 0.005f;

        private GameObject m_LineRendererObj;
        private LineRenderer m_LineRenderer;
        private List<Vector3> m_WorldPosList = new List<Vector3>();
        private bool m_IsDrawing = false;
        private bool m_IsPickedUp = true;

        private const float MIN_LINE_SEGMENT = 0.01f;

        //hide laser and reticle in this demo
        private void Start()
        {
            NRInput.LaserVisualActive = false;
            NRInput.ReticleVisualActive = false;
        }

        private void Update()
        {
            m_IsDrawing = m_IsPickedUp && NRInput.GetButton(ControllerButton.TRIGGER);
            if (m_IsDrawing)
            {
                if (m_LineRendererObj == null)
                    CreateColoredLine();
                Vector3 pos = penPoint.position;
                if (m_WorldPosList.Count > 1 && Vector3.Distance(pos, m_WorldPosList[m_WorldPosList.Count - 1]) < MIN_LINE_SEGMENT)
                    return;
                m_WorldPosList.Add(pos);
                Draw();
            }
            else
            {
                if (m_LineRendererObj)
                    m_LineRendererObj = null;
                if (m_WorldPosList.Count != 0)
                    m_WorldPosList.Clear();
            }

        }

        private void CreateColoredLine()
        {
            m_LineRendererObj = Instantiate(lineRendererPrefab, this.transform);
            m_LineRendererObj.SetActive(true);
            m_LineRenderer = m_LineRendererObj.GetComponent<LineRenderer>();
            m_LineRenderer.numCapVertices = 8;
            m_LineRenderer.numCornerVertices = 8;
            m_LineRenderer.startWidth = 0.01f;
            m_LineRenderer.endWidth = 0.01f;
        }

        private void Draw()
        {
            m_LineRenderer.positionCount = m_WorldPosList.Count;
            m_LineRenderer.SetPositions(m_WorldPosList.ToArray());
        }
    }
}
