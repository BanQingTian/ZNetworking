using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlock
{
    public struct FishBehaviour
    {
        public int id;
        public float speed;
        public float force;
        public float acceleration;
        public float turnSpeed;
        public float radius;
        public float distFromNeighbour;
        public float distFromPredator;
        public Vector3 currentPos;
        public Vector3 destination;
        public Vector3 desiredVelocity;
        public Vector3 velocity;
        public Vector3 targetDirection;
        public float targetDirectionMagnitude;
        public bool seek;
        public bool outOfBounds;
        public Transform transform;

        public float scaredTime;

        public Vector3 scarePosition;

        public float scareRadius;
    }
}