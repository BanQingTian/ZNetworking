using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlock
{
    public class Predator : MonoBehaviour
    {
        [Tooltip("The radius of the area the fishes will scare away as soon as they enter it.")]
        public float radius = 5;
        [HideInInspector]
        public Vector3 currentPosition;
        Vector3 lastTargetPos;
        [Tooltip("The controller that the predator will follow to get the fishes.")]
        public FishFlockController controller;
        Transform myTransform;
        [Tooltip("The predator's speed.")]
        public float speed;
        [Tooltip("The predator's turn rotation speed.")]
        public float turnSpeed;
        [Tooltip("The predator's mouth transform so it can eat fishes that are close to it.")]
        public Transform mouth;
        [Tooltip("A blood visual effect to spawn when the predator eats a fish.")]
        public GameObject blood;
        [Tooltip("Draw the radius area on the scene view.")]
        public bool drawGizmos = false;
        [Tooltip("Time to wait until it randomly gets a new position to go inside the fish's group area.")]
        public float changePositionRate = 0.4f;
        float currChangePositionRate = 0.0f;
        Vector3 vecToFollow;

        private void Awake()
        {
            myTransform = transform;

            lastTargetPos = myTransform.position;
        }

        void Update()
        {
            float minX = controller.groupAnchor.x - (controller.groupAreaWidth / 2);
            float maxX = controller.groupAnchor.x + (controller.groupAreaWidth / 2);
            float minY = controller.groupAnchor.y - (controller.groupAreaHeight / 2);
            float maxY = controller.groupAnchor.y + (controller.groupAreaHeight / 2);
            float minZ = controller.groupAnchor.z - (controller.groupAreaDepth / 2);
            float maxZ = controller.groupAnchor.z + (controller.groupAreaDepth / 2);
           
            if (currChangePositionRate < Time.time)
            {
                lastTargetPos.x = Random.Range(minX, maxX);
                lastTargetPos.y = Random.Range(minY, maxY);
                lastTargetPos.z = Random.Range(minZ, maxZ);
                vecToFollow = lastTargetPos - myTransform.position;

                currChangePositionRate = Time.time + changePositionRate;
            }

            myTransform.position += myTransform.forward * Time.deltaTime * speed;

            Quaternion futureRotation = Quaternion.LookRotation(vecToFollow);
            if(futureRotation.eulerAngles.magnitude > 0.1f)
                myTransform.rotation = Quaternion.Slerp(myTransform.rotation, futureRotation, Time.smoothDeltaTime * turnSpeed);
            
            currentPosition = myTransform.position;

            for (int i = 0; i < controller.behaviours.Count; i++)
            {
                FishBehaviour b = controller.behaviours[i];

                if((b.currentPos - mouth.position).magnitude < b.radius)
                {
                    controller.KillFish(i, b.id);
                    GameObject bloodInstance = Instantiate(blood, mouth.position, blood.transform.rotation);
                    Destroy(bloodInstance, 2f);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            if (myTransform == null) myTransform = transform;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(myTransform.position, radius);
        }
    }
}