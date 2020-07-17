using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FishFlock
{
    public struct FishBehaviourGPU
    {
        public Vector3 position;
        public Vector3 velocity;
        public float speed;
        public float rot_speed;
        public float speed_offset;
    }

    public struct CollisionArea
    {
        public Vector3 position;
        public Vector3 size;
    }

    public class FishFlockControllerGPU : MonoBehaviour
    {
        public enum MovementAxis
        {
            XYZ,
            XY,
            XZ,
        };

        [CustomTitle("Settings")]
        [Tooltip("The width limit of the swimming area where the group can swim.")]
        public float swimmingAreaWidth = 10;
        [Tooltip("The height limit of the swimming area where the group can swim.")]
        public float swimmingAreaHeight = 10;
        [Tooltip("The depth limit of the swimming area where the group can swim.")]
        public float swimmingAreaDepth = 10;
        [Tooltip("The width limit that the fishes will force themselves to swim inside.")]
        public float groupAreaWidth = 20;
        [Tooltip("The height limit that the fishes will force themselves to swim inside.")]
        public float groupAreaHeight = 20;
        [Tooltip("The depth limit that the fishes will force themselves to swim inside.")]
        public float groupAreaDepth = 20;
        [Tooltip("The movement axis of the fishes. This must be set before play mode to work properly.")]
        public MovementAxis movementAxis = MovementAxis.XYZ;
        [Tooltip("Draw the gizmos or debug lines on the scene view.")]
        public bool debugDraw = true;

        [CustomTitle("Flocking")]
        public int fishesCount;
        [Tooltip("Minimum speed to be applied on the fish direction vector.")]
        public float minSpeed = 3;
        [Tooltip("Maximum speed to be applied on the fish direction vector.")]
        public float maxSpeed = 8;
        [Tooltip("Minimum turn speed when rotating the fish to it's direction vector.")]
        public float minRotationSpeed = 5;
        [Tooltip("Maximum turn speed when rotating the fish to it's direction vector.")]
        public float maxRotationSpeed = 10;
        [Tooltip("Desired distance between neighbours")]
        public float neighbourDistance = 1f;
        [Tooltip("Spawn Radius of the fishes")]
        public float spawnRadius;
        [Tooltip("Variation speed that will be applied to the normal speed of the fishes.")]
        public float speedVariation = 0.6f;

        [CustomTitle("Target Following")]
        [Tooltip("Follow the specified target or not?")]
        public bool followTarget = false;
        [Tooltip("The transform target that the group will follow.")]
        public Transform target;

        [CustomTitle("Random Target Points Following")]
        [Tooltip("Minimum target points that will randomly be generated to follow if not following a target.")]
        public int minTargetPoints = 5;
        [Tooltip("Maximum target points that will randomly be generated to follow if not following a target.")]
        public int maxTargetPoints = 12;
        Vector3[] targetPositions;
        int currentTargetPosIndex = 0;
        [Tooltip("Recalculate points after the group reaches the last one.")]
        public bool recalculatePoints = false;
        [Tooltip("The speed in which the group area will move")]
        public float groupAreaSpeed = 0.8f;

        [CustomTitle("Gfx")]
        public ComputeShader computeShader;
        public FishBehaviourGPU[] fishesData;
        public Mesh fishMesh;
        [Tooltip("Instanced Material to be used to draw the fishes.")]
        public Material fishInstancedMaterial;

        [CustomTitle("Collision Avoidance"), Tooltip("Avoidance force when checking collisions with the boxes.")]
        public float force = 1;
        [Tooltip("Colliders that the fishes will try to avoid (for now it's only Box Colliders)")]
        public BoxCollider[] boxColliders;
        CollisionArea[] collisionData;
        int collisionDataLength;
        ComputeBuffer collisionBuffer;

        int kernelHandle;
        ComputeBuffer fishBuffer;
        ComputeBuffer drawArgsBuffer;
        MaterialPropertyBlock props;
        Vector3 groupAnchor;
        const int GROUP_SIZE = 256;
        Transform myTransform;

        private void Awake()
        {
            myTransform = transform;

            int collidersLength = boxColliders.Length;
            if (boxColliders != null && collidersLength > 0)
            {
                collisionData = new CollisionArea[collidersLength];

                for (int i = 0; i < collidersLength; i++)
                {
                    CollisionArea ca = new CollisionArea();
                    BoxCollider bc = boxColliders[i];
                    if(bc == null)
                    {
                        Debug.LogError("One of the Box Colliders is null!");

                        boxColliders = new BoxCollider[0];
                        return;
                    }

                    Transform bcTransform = bc.transform;
                    Vector3 localScale = bcTransform.localScale;

                    Vector3 coll_pos = bc.transform.position;

                    Vector3 coll_size = bc.size;
                    coll_size.x *= localScale.x;
                    coll_size.y *= localScale.y;
                    coll_size.z *= localScale.z;

                    coll_pos.x -= coll_size.x / 2.0f;
                    coll_pos.y -= coll_size.y / 2.0f;
                    coll_pos.z -= coll_size.z / 2.0f;

                    ca.position = coll_pos;
                    ca.size = coll_size;

                    collisionData[i] = ca;
                }
            }
        }

        void Start()
        {
            if(followTarget)
                groupAnchor = target.position;
            else
            {
                GeneratePath();
                groupAnchor = targetPositions[0];
            }

            drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            drawArgsBuffer.SetData(new uint[5] { fishMesh.GetIndexCount(0), (uint) fishesCount, 0, 0, 0 });

            // This property block is used only for avoiding an instancing bug.
            props = new MaterialPropertyBlock();
            props.SetFloat("_UniqueID", Random.value);

            fishesData = new FishBehaviourGPU[fishesCount];
            kernelHandle = computeShader.FindKernel("CSMain");

            for (int i = 0; i < fishesCount; i++)
            {
                fishesData[i] = CreateBehaviour();
                fishesData[i].speed_offset = Random.value * 1000.0f;
            }

            fishBuffer = new ComputeBuffer(fishesCount, sizeof(float) * 9);
            fishBuffer.SetData(fishesData);

            if (boxColliders.Length <= 0)
                collisionData = new CollisionArea[1] { new CollisionArea() };

            collisionBuffer = new ComputeBuffer(collisionData.Length, sizeof(float) * 6);
            collisionBuffer.SetData(collisionData);

            collisionDataLength = boxColliders.Length <= 0 ? 0 : collisionData.Length;
        }

        FishBehaviourGPU CreateBehaviour()
        {
            FishBehaviourGPU behaviour = new FishBehaviourGPU();
            Vector3 pos = groupAnchor + Random.insideUnitSphere * spawnRadius;
            Quaternion rot = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);

            switch (movementAxis)
            {
                case MovementAxis.XY:                
                    pos.z = rot.z = 0.0f;                 
                    break;
                case MovementAxis.XZ:
                    pos.y = rot.y = 0.0f;
                    break;
            }

            behaviour.position = pos;
            behaviour.velocity = rot.eulerAngles;

            behaviour.speed = Random.Range(minSpeed, maxSpeed);
            behaviour.rot_speed = Random.Range(minRotationSpeed, maxRotationSpeed);

            return behaviour;
        }

        void Update()
        {
            UpdateGroupAnchor();

            switch(movementAxis)
            {
                case MovementAxis.XY:
                    computeShader.SetInt("movementMode", 1);
                    break;
                case MovementAxis.XZ:
                    computeShader.SetInt("movementMode", 2);
                    break;
                default:
                    computeShader.SetInt("movementMode", 0);
                    break;
            }

            computeShader.SetFloat("deltaTime", Time.deltaTime);
            computeShader.SetVector("target", groupAnchor);
            computeShader.SetFloat("neighbourDistance", neighbourDistance);
            computeShader.SetInt("fishesCount", fishesCount);
            computeShader.SetFloat("collisionForce", force);
            computeShader.SetInt("collisionCount", collisionDataLength);
            computeShader.SetFloat("speedVariation", speedVariation);
            computeShader.SetBuffer(this.kernelHandle, "fishBuffer", fishBuffer);
            computeShader.SetBuffer(kernelHandle, "collisionBuffer", collisionBuffer);

            computeShader.Dispatch(this.kernelHandle, this.fishesCount / GROUP_SIZE + 1, 1, 1);

            fishInstancedMaterial.SetBuffer("fishBuffer", fishBuffer);
            Graphics.DrawMeshInstancedIndirect(
                fishMesh, 0, fishInstancedMaterial,
                new Bounds(Vector3.zero, Vector3.one * 1000),
                drawArgsBuffer, 0, props
            );
        }

        void OnDestroy()
        {
            if (collisionBuffer != null) collisionBuffer.Release();
            if (fishBuffer != null) fishBuffer.Release();
            if (drawArgsBuffer != null) drawArgsBuffer.Release();
        }

        void UpdateGroupAnchor()
        {
            float minX = myTransform.position.x - (swimmingAreaWidth / 2) + (groupAreaWidth / 2);
            float maxX = myTransform.position.x + (swimmingAreaWidth / 2) - (groupAreaWidth / 2);

            float minY = myTransform.position.y - (swimmingAreaHeight / 2) + (groupAreaHeight / 2);
            float maxY = myTransform.position.y + (swimmingAreaHeight / 2) - (groupAreaHeight / 2);

            float minZ = myTransform.position.z - (swimmingAreaDepth / 2) + (groupAreaDepth / 2);
            float maxZ = myTransform.position.z + (swimmingAreaDepth / 2) - (groupAreaDepth / 2);

            Vector3 futurePosition = myTransform.position;

            if (!followTarget && targetPositions.Length > 0)
            {
                if ((groupAnchor - targetPositions[currentTargetPosIndex]).magnitude < 1)
                {
                    currentTargetPosIndex++;

                    if (currentTargetPosIndex >= targetPositions.Length)
                    {
                        if (recalculatePoints)
                            GeneratePath();
                        else
                            currentTargetPosIndex = targetPositions.Length - 1;
                    }
                }

                Vector3 vel = (targetPositions[currentTargetPosIndex] - groupAnchor);
                futurePosition = groupAnchor + vel * Time.deltaTime * groupAreaSpeed;
            }
            else if (followTarget)
            {
                if (target != null)
                {
                    Vector3 vel = (target.position - groupAnchor);
                    futurePosition = groupAnchor + vel * Time.deltaTime * groupAreaSpeed;
                }
            }

            futurePosition.x = Mathf.Clamp(futurePosition.x, minX, maxX);
            futurePosition.y = Mathf.Clamp(futurePosition.y, minY, maxY);
            futurePosition.z = Mathf.Clamp(futurePosition.z, minZ, maxZ);

            groupAnchor = futurePosition;
        }

        void GeneratePath()
        {
            float minX = myTransform.position.x - (swimmingAreaWidth / 2) + (groupAreaWidth / 2);
            float maxX = myTransform.position.x + (swimmingAreaWidth / 2) - (groupAreaWidth / 2);

            float minY = myTransform.position.y - (swimmingAreaHeight / 2) + (groupAreaHeight / 2);
            float maxY = myTransform.position.y + (swimmingAreaHeight / 2) - (groupAreaHeight / 2);

            float minZ = myTransform.position.z - (swimmingAreaDepth / 2) + (groupAreaDepth / 2);
            float maxZ = myTransform.position.z + (swimmingAreaDepth / 2) - (groupAreaDepth / 2);

            targetPositions = new Vector3[Random.Range(minTargetPoints, maxTargetPoints)];

            Vector3 tempPos;
            for (int i = 0; i < targetPositions.Length; i++)
            {
                tempPos.x = Random.Range(minX, maxX);
                tempPos.y = Random.Range(minY, maxY);
                tempPos.z = Random.Range(minZ, maxZ);

                targetPositions[i] = tempPos;
            }

            currentTargetPosIndex = 0;
        }

        Vector3 volumeSize;
        private void OnDrawGizmos()
        {
            if (groupAreaWidth > swimmingAreaWidth || groupAreaHeight > swimmingAreaHeight || groupAreaDepth > swimmingAreaDepth)
            {
                groupAreaWidth = swimmingAreaWidth;
                groupAreaHeight = swimmingAreaHeight;
                groupAreaDepth = swimmingAreaDepth;

                Debug.Log("[Flocking Behaviour] The group area can't be bigger than the bounds.");
            }

            if (!debugDraw) return;

            volumeSize.x = swimmingAreaWidth;
            volumeSize.y = swimmingAreaHeight;
            volumeSize.z = swimmingAreaDepth;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, volumeSize);

            volumeSize.x = groupAreaWidth;
            volumeSize.y = groupAreaHeight;
            volumeSize.z = groupAreaDepth;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Application.isPlaying ? groupAnchor : transform.position, volumeSize);

            if (Application.isPlaying && !followTarget)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < targetPositions.Length - 1; i++)
                {
                    Gizmos.DrawLine(targetPositions[i], targetPositions[i + 1]);

                    Gizmos.DrawWireSphere(targetPositions[i], 1f);

                    if ((i + 1) == targetPositions.Length - 1)
                        Gizmos.DrawWireSphere(targetPositions[i + 1], 1f);
                }
            }
        }
    }

}