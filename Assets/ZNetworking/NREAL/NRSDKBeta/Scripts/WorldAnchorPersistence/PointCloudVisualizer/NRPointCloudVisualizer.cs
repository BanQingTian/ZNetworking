using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace NRKernal
{
    public class NRPointCloudVisualizer : IPointCloudDrawer
    {
        public Dictionary<Int64, PointCloudPoint> Points = new Dictionary<Int64, PointCloudPoint>();
        private Mesh m_Mesh;
        private GameObject visualizer;

        public NRPointCloudVisualizer()
        {
            visualizer = new GameObject("PointCloudVisualizer");
            m_Mesh = new Mesh();
            visualizer.AddComponent<MeshFilter>().mesh = m_Mesh;
            visualizer.AddComponent<MeshRenderer>().material = new Material(Resources.Load<Shader>("VertexColor"));
        }

        public void Draw()
        {
            PointCloudPoint[] pointlist = Points.Values.ToArray<PointCloudPoint>();
            Vector3[] points = new Vector3[pointlist.Length];
            int[] indecies = new int[pointlist.Length];
            Color[] colors = new Color[pointlist.Length];

            for (int i = 0; i < points.Length; ++i)
            {
                var pos = pointlist[i].Position;
                points[i] = new Vector3(pos.X, pos.Y, pos.Z);
                indecies[i] = i;
                colors[i] = GetColorByConfidence(pointlist[i].Confidence);
            }

            m_Mesh.vertices = points;
            m_Mesh.colors = colors;
            m_Mesh.SetIndices(indecies, MeshTopology.Points, 0);
        }

        private Color GetColorByConfidence(float confidence)
        {
            if (confidence > 0.3333f)
            {
                confidence = 0.33333f;
            }
            confidence *= 3;
            return Color.LerpUnclamped(Color.red, Color.blue, confidence);
        }

        public void AdjustPointSize(float size)
        {
            visualizer.GetComponent<MeshRenderer>().material.SetFloat("_PointSize", size);
        }

        public void Update(PointCloudPoint point)
        {
            Points[point.Id] = point;
        }
    }
}
