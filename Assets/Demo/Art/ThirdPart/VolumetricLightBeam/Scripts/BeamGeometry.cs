#if DEBUG
//#define DEBUG_SHOW_MESH_NORMALS
//#define DEBUG_SHOW_BOUNDING_BOX
#endif
#define FORCE_CURRENT_CAMERA_DEPTH_TEXTURE_MODE

using UnityEngine;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    [AddComponentMenu("")] // hide it from Component search
    [ExecuteInEditMode]
    [HelpURL(Consts.HelpUrlBeam)]
    public class BeamGeometry : MonoBehaviour
    {
        const int kNbSegments = 0;

        VolumetricLightBeam m_Master = null;

        public MeshRenderer meshRenderer { get; private set; }
        public MeshFilter meshFilter { get; private set; }
        public Material material { get; private set; }
        public Mesh coneMesh { get; private set; }

        public bool visible
        {
            get { return meshRenderer.enabled; }
            set { meshRenderer.enabled = value; }
        }

        void Start()
        {
            // Handle copy / paste the LightBeam in Editor
            if (!m_Master)
                DestroyImmediate(gameObject);
            }
        
        void OnDestroy()
        {
            if (material)
            {
                DestroyImmediate(material);
                material = null;
            }
        }
        
        public void Initialize(VolumetricLightBeam master, Shader shader)
        {
            var hideFlags = Consts.ProceduralObjectsHideFlags;
            m_Master = master;

            transform.SetParent(master.transform, false);
            material = new Material(shader);
            material.hideFlags = hideFlags;

            meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();
            meshRenderer.hideFlags = hideFlags;
            meshRenderer.material = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
#if UNITY_5_4_OR_NEWER
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
#else
            meshRenderer.useLightProbes = false;
#endif
            meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            meshFilter.hideFlags = hideFlags;

            gameObject.hideFlags = hideFlags;
        }

        /// <summary>
        /// Generate the cone mesh and calls UpdateMaterialAndBounds.
        /// Since this process involves recreating a new mesh, make sure to not call it at every frame during playtime.
        /// </summary>
        public void RegenerateMesh()
        {
            Debug.Assert(m_Master);
            gameObject.layer = Config.Instance.geometryLayerID;

            if (coneMesh)
                DestroyImmediate(coneMesh);

            coneMesh = MeshGenerator.GenerateConeZ_Radius(1f, 1f, 1f, m_Master.geomSides, kNbSegments, m_Master.geomCap);

            coneMesh.hideFlags = Consts.ProceduralObjectsHideFlags;
            meshFilter.mesh = coneMesh;

            UpdateMaterialAndBounds();
        }

        void ComputeBounds()
        {
            if (coneMesh)
            {
                var radiusStart = m_Master.coneRadiusStart;
                var radiusEnd = m_Master.coneRadiusEnd;
                var radiusMax = Mathf.Max(radiusStart, radiusEnd);
                var length = m_Master.fadeEnd;

                var bounds = new Bounds(
                    new Vector3(0, 0, length / 2),
                    new Vector3(radiusMax * 2, radiusMax * 2, length)
                    );
                coneMesh.bounds = bounds;
            }
        }

        public void UpdateMaterialAndBounds()
        {
            Debug.Assert(m_Master);

            float slopeRad = (m_Master.coneAngle * Mathf.Deg2Rad) / 2; // use coneAngle (instead of spotAngle) which is more correct with the geometry
            var coneRadius = new Vector2(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
            material.SetVector("_ConeSlopeCosSin", new Vector2(Mathf.Cos(slopeRad), Mathf.Sin(slopeRad)));
            material.SetVector("_ConeRadius", coneRadius);
            material.SetFloat("_ConeApexOffsetZ", m_Master.coneApexOffsetZ);
            material.SetColor("_Color", m_Master.color);
            material.SetFloat("_AlphaInside", m_Master.alphaInside);
            material.SetFloat("_AlphaOutside", m_Master.alphaOutside);
            material.SetFloat("_AttenuationLerpLinearQuad", m_Master.attenuationLerpLinearQuad);
            material.SetFloat("_DistanceFadeStart", m_Master.fadeStart);
            material.SetFloat("_DistanceFadeEnd", m_Master.fadeEnd);
            material.SetFloat("_DistanceCamClipping", m_Master.cameraClippingDistance);
            material.SetFloat("_FresnelPow", m_Master.fresnelPow);
            material.SetFloat("_GlareBehind",  m_Master.glareBehind);
            material.SetFloat("_GlareFrontal",  m_Master.glareFrontal);

            if (m_Master.depthBlendDistance > 0f)
            {
                material.EnableKeyword("VLB_DEPTH_BLEND");
                material.SetFloat("_DepthBlendDistance", m_Master.depthBlendDistance);
            }
            else
                material.DisableKeyword("VLB_DEPTH_BLEND");

            if (m_Master.noiseEnabled && m_Master.noiseIntensity > 0f && Noise3D.isSupported) // test Noise3D.isSupported the last
            {
                Noise3D.LoadIfNeeded();
                material.EnableKeyword("VLB_NOISE_3D");
                material.SetVector("_NoiseLocal", new Vector4(m_Master.noiseVelocityLocal.x, m_Master.noiseVelocityLocal.y, m_Master.noiseVelocityLocal.z, m_Master.noiseScaleLocal));
                material.SetVector("_NoiseParam", new Vector3(m_Master.noiseIntensity, m_Master.noiseVelocityUseGlobal ? 1f : 0f, m_Master.noiseScaleUseGlobal ? 1f : 0f));
            }
            else
                material.DisableKeyword("VLB_NOISE_3D");

            // Need to manually compute mesh bounds since the shape of the cone is generated in the Vertex Shader
            ComputeBounds();

#if DEBUG_SHOW_MESH_NORMALS
            for (int vertexInd = 0; vertexInd < coneMesh.vertexCount; vertexInd++)
            {
                var vertex = coneMesh.vertices[vertexInd];

                // apply modification done inside VS
                vertex.x *= Mathf.Lerp(coneRadius.x, coneRadius.y, vertex.z);
                vertex.y *= Mathf.Lerp(coneRadius.x, coneRadius.y, vertex.z);
                vertex.z *= m_Master.fadeEnd;

                var cosSinFlat = new Vector2(vertex.x, vertex.y).normalized;
                var normal = new Vector3(cosSinFlat.x * Mathf.Cos(slopeRad), cosSinFlat.y * Mathf.Cos(slopeRad), -Mathf.Sin(slopeRad)).normalized;

                vertex = transform.TransformPoint(vertex);
                normal = transform.TransformDirection(normal);
                Debug.DrawRay(vertex, normal * 0.25f);
            }
#endif
        }

        public void SetClippingPlane(Plane planeWS)
        {
            var normal = planeWS.normal;
            material.EnableKeyword("VLB_CLIPPING_PLANE");
            material.SetVector("_ClippingPlaneWS", new Vector4(normal.x, normal.y, normal.z, planeWS.distance));
        }

        public void SetClippingPlaneOff()
        {
            material.DisableKeyword("VLB_CLIPPING_PLANE");
        }

#if DEBUG_SHOW_BOUNDING_BOX
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            DrawRendererBounds();
        }

        void DrawRendererBounds()
        {
            var bounds = meshRenderer.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        void DrawMeshBounds()
        {
            var bounds = meshFilter.sharedMesh.bounds;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
#endif

        void OnWillRenderObject()
        {
            if (m_Master)
            {
                var cam = Camera.current;
                if (material)
                {
                    var camForwardVectorOSN = transform.InverseTransformDirection(cam.transform.forward).normalized;
                    float camIsInsideBeamFactor = cam.orthographic ? -1f : m_Master.GetInsideBeamFactor(cam.transform.position);
                    material.SetVector("_CameraParams", new Vector4(camForwardVectorOSN.x, camForwardVectorOSN.y, camForwardVectorOSN.z, camIsInsideBeamFactor));
                }

#if FORCE_CURRENT_CAMERA_DEPTH_TEXTURE_MODE
                if (m_Master.depthBlendDistance > 0f)
                    cam.depthTextureMode |= DepthTextureMode.Depth;
#endif
            }
        }
    }
}
