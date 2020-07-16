using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace FishFlock
{
    public class FishFlockController : MonoBehaviour
    {
        [CustomTitle("Settings")]
        [Tooltip("How many frames to skip from calculating the flocking behaviours. (Except for the collision avoidance)")]
        public int frameSkipAmount = 1;
        int frameSkipAmountCount = 0;
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
        [Tooltip("Draw the gizmos or debug lines on the scene view.")]
        public bool debugDraw = true;

        [CustomTitle("Flocking")]
        [Tooltip("Fish prefab to be instantiated.")]
        public GameObject prefab;
        [Tooltip("Amount of fish to be instantiated."), Range(1, 300)]
        public int fishAmount = 4;
        [Tooltip("Minimum acceleration to be applied on the fish vector.")]
        public float minAcceleration = 6f;
        [Tooltip("Maximum acceleration to be applied on the fish vector.")]
        public float maxAcceleration = 15f;
        [Tooltip("Minimum speed to be applied on the fish direction vector.")]
        public float minSpeed = 0.1f;
        [Tooltip("Maximum speed to be applied on the fish direction vector.")]
        public float maxSpeed = 1.0f;
        [Tooltip("Minimum force to be applied on the fish direction vector.")]
        public float minForce = 0.01f;
        [Tooltip("Maximum force to be applied on the fish direction vector.")]
        public float maxForce = 0.4f;
        [Tooltip("Minimum scale of the fish transform.")]
        public float minScale = 0.5f;
        [Tooltip("Maximum scale of the fish transform.")]
        public float maxScale = 1.2f;
        [Tooltip("Minimum turn speed when rotating the fish to it's direction vector.")]
        public float minTurnSpeed = 5;
        [Tooltip("Maximum turn speed when rotating the fish to it's direction vector.")]
        public float maxTurnSpeed = 7;
        [Tooltip("Size of the distance to look ahead to check for colliders.")]
        public float lookAheadDistance = 3.0f;
        [Tooltip("Size of the distance to look on the sides to check for colliders.")]
        public float lookSideDistance = 2.0f;
        [Tooltip("Distance to try to keep away from fishes that are close to each other.")]
        public float neighbourDistance = 2.2f;
        [Tooltip("The size of each fish to be multiplied with the neighbour distance so it can calculate the separation between the fishes.")]
        public float fishSize = 2;
        [Tooltip("The separation scale effect to apply between each fish, how separated you want them to behave.")]
        public float separationScale = 1.0f;
        [Tooltip("The alignment scale effect to apply between each fish, it defines the alignment they have with each other.")]
        public float alignmentScale = 1.0f;
        [Tooltip("The cohesion scale effect to apply between each fish, coherence will make them more splitted or together.")]
        public float cohesionScale = 1.0f;
        [Tooltip("This will make the fishes force more themselves to stay inside the group area, prioritizing this above the other destinations.")]
        public bool forceGroupAreaDestination = false;
        [Tooltip("Swimming on local space means that the fishes will now swim on the local space of the group area, like the group area it's they parent. This must be selected before playing the scene.")]
        public bool swimOnLocalSpace = false;

        [CustomTitle("Collision Avoidance")]
        [Tooltip("The amount of avoidance to apply when colliding to something, usually multiplied by the collided point plus the normal vector of the collided point.")]
        public float collisionAvoidAmount = 20.0f;
        [Tooltip("The collision layer that the fishes will check for collisions")]
        public LayerMask collisionAvoidLayer;


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

        [CustomTitle("Attack")]
        public bool attackTarget = false;
        public float distanceToAttack = 5f;
        public float secondsAttacking = 3f;
        public bool keepAttacking = false;
        public GameObject targetToAttack;
        Transform targetToAttackTransform;

        [HideInInspector]
        public Vector3 groupAnchor;
        Transform flockingParent;

        // temp variables
        bool refreshVariables = false;
        float lastMinSpeed = 0.0f;
        float lastMaxSpeed = 0.0f;
        float lastMinForce = 0.0f;
        float lastMaxForce = 0.0f;
        float lastMinAcceleration = 0.0f;
        float lastMaxAcceleration = 0.0f;
        float lastMinTurnSpeed = 0.0f;
        float lastMaxTurnSpeed = 0.0f;
        int lastAgentsAmount = 0;

        static int fishIdEnum = 0;

        volatile bool requestedCalculation = false;
        float deltaTime;

        [HideInInspector]
        public List<FishBehaviour> behaviours = new List<FishBehaviour>();
        List<Transform> agentsTransforms = new List<Transform>();

        Transform myTransform;
        FlockCalculationRequest request = new FlockCalculationRequest();
        public Predator[] predators;

        public delegate FishBehaviour onUpdateFish(FishBehaviour behaviour);
        public onUpdateFish OnUpdateFishEvent;

        List<int> queueToClear = new List<int>();

        public static readonly Vector3 Vec3Forward = new Vector3(0.0f, 0.0f, 1.0f);
        public static readonly Vector3 Vec3Zero = new Vector3(0.0f, 0.0f, 0.0f);

        void Awake()
        {
            myTransform = transform;

            lastAgentsAmount = fishAmount;
            UpdateTempVariables();

            request.callback = OnFlockingCalcCompleted;

            groupAnchor = myTransform.position;

            if(followTarget)
            {
                if(target)
                {
                    groupAnchor = target.position;
                }
            }

            if (FlockMain.eventLock == null) FlockMain.eventLock = new AutoResetEvent(false);
            FlockMain.running = true;

            if (FlockMain.flockThreadCalc == null)
            {
                FlockMain.flockThreadCalc = new Thread(FlockMain.CalculationLoop);
                FlockMain.flockThreadCalc.Start();
            }

            flockingParent = new GameObject("FlockingAgents").transform;

            if(swimOnLocalSpace)
                flockingParent.position = groupAnchor;

            if (!followTarget) GeneratePath();

            float minX = myTransform.position.x - (swimmingAreaWidth / 2) + (groupAreaWidth / 2);
            float maxX = myTransform.position.x + (swimmingAreaWidth / 2) - (groupAreaWidth / 2);
            float minY = myTransform.position.y - (swimmingAreaHeight / 2) + (groupAreaHeight / 2);
            float maxY = myTransform.position.y + (swimmingAreaHeight / 2) - (groupAreaHeight / 2);
            float minZ = myTransform.position.z - (swimmingAreaDepth / 2) + (groupAreaDepth / 2);
            float maxZ = myTransform.position.z + (swimmingAreaDepth / 2) - (groupAreaDepth / 2);


            for (int i = 0; i < fishAmount; i++)
            {
                behaviours.Add(CreateAgent(agentsTransforms, i));
            }
            

            if(targetToAttack)
            {
                targetToAttackTransform = targetToAttack.transform;
            }
        }

        private void Start()
        {
            predators = FindObjectsOfType<Predator>();
            StartCoroutine(GroupAnchorRandomMovement());           
        }

        FishBehaviour CreateAgent(List<Transform> transforms, int i)
        {
            Transform agentTransform = Instantiate(prefab).transform;
            float scale = Random.Range(minScale, maxScale);
            agentTransform.localScale = new Vector3(scale, scale, scale);
            transforms.Add(agentTransform);
            agentTransform.position = groupAnchor;
            agentTransform.SetParent(flockingParent);

            FishBehaviour ab = new FishBehaviour();
            ab.acceleration = Random.Range(minAcceleration, maxAcceleration);
            ab.speed = Random.Range(minSpeed, maxSpeed);
            ab.force = Random.Range(minForce, maxForce);
            ab.turnSpeed = Random.Range(minTurnSpeed, maxTurnSpeed);
            ab.radius = fishSize;
            ab.scaredTime = 0.0f;
            ab.distFromNeighbour = neighbourDistance;
            ab.currentPos =   agentTransform.position ;
            ab.destination = followTarget ? target.position : agentTransform.position;
            agentTransform.position = ab.currentPos;
            ab.velocity = new Vector3(0, 0, 0);
            ab.distFromPredator = 18;
            ab.id = fishIdEnum++;
            ab.transform = agentTransform;

            return ab;
        }

        FishBehaviour CreatePureAgent(int i)
        {
            //float scale = Random.Range(minScale, maxScale);

            FishBehaviour ab = new FishBehaviour();
            ab.acceleration = Random.Range(minAcceleration, maxAcceleration);
            ab.speed = Random.Range(minSpeed, maxSpeed);
            ab.force = Random.Range(minForce, maxForce);
            ab.turnSpeed = Random.Range(minTurnSpeed, maxTurnSpeed);
            ab.radius = fishSize;
            ab.scaredTime = 0.0f;
            ab.distFromNeighbour = neighbourDistance;
            ab.currentPos = groupAnchor;
            ab.destination = followTarget ? target.position : groupAnchor;
            ab.velocity = new Vector3(0, 0, 0);
            ab.distFromPredator = 18;
            ab.id = fishIdEnum++;
            ab.transform = null;

            return ab;
        }

        public void KillFish(int index, int id)
        {
            if (queueToClear.Contains(id)) return;

            queueToClear.Add(id);

            Transform t = agentsTransforms[index];
            t.gameObject.SetActive(false);        
        }

        void UpdateTempVariables()
        {

            if (lastMinForce != minForce || lastMaxForce != maxForce || lastMaxSpeed != maxSpeed ||
                lastMinSpeed != minSpeed || lastMinAcceleration != minAcceleration || lastMaxAcceleration != maxAcceleration
                || lastMinTurnSpeed != minTurnSpeed || lastMaxTurnSpeed != maxTurnSpeed)
            {
                lastMinForce = minForce;
                lastMaxForce = maxForce;
                lastMinSpeed = minSpeed;
                lastMaxSpeed = maxSpeed;
                lastMinAcceleration = minAcceleration;
                lastMaxAcceleration = maxAcceleration;
                lastMinTurnSpeed = minTurnSpeed;
                lastMaxTurnSpeed = maxTurnSpeed;

                refreshVariables = true;
            }
        }

        Vector3 groupAnchorOffset;

        void Update()
        {
            if(swimOnLocalSpace) flockingParent.position = groupAnchor;

            if (frameSkipAmountCount >= frameSkipAmount) frameSkipAmountCount = 0;

            deltaTime = Time.deltaTime;
            UpdateTempVariables();

            if (requestedCalculation) FlockMain.eventLock.Set();

            UpdateParent();

            float minX = groupAnchor.x - (groupAreaWidth / 2);
            float maxX = groupAnchor.x + (groupAreaWidth / 2);
            float minY = groupAnchor.y - (groupAreaHeight / 2);
            float maxY = groupAnchor.y + (groupAreaHeight / 2);
            float minZ = groupAnchor.z - (groupAreaDepth / 2);
            float maxZ = groupAnchor.z + (groupAreaDepth / 2);

            Vector3 targetPosition = target ? target.position : Vector3.zero;

            Vector3 destination = targetPosition;
            bool reachedTarget = false;
            if (followTarget)
            {
                if (target && (targetPosition - groupAnchor).magnitude < 2f)
                {
                    if ((destination - groupAnchor).magnitude < 2f)
                    {
                        reachedTarget = true;
                    }
                }
            }
            else
                reachedTarget = true;

            for (int i = 0; i < lastAgentsAmount; i++)
            {
                FishBehaviour behaviour = behaviours[i];
                if (behaviour.transform == null) continue;

                Transform btransform = agentsTransforms[i];
                Vector3 btransformPosition = btransform.position;
                Vector3 bOldPosition = btransformPosition;
                Vector3 btransformForward = btransform.rotation * Vec3Forward;
                Quaternion btransformRotation = btransform.rotation;
              
                bOldPosition = btransformPosition;

                Vector3 targetDirection = behaviour.targetDirection;

                CheckForObstacles(behaviour, ref targetDirection, btransformPosition, btransformForward);

                Quaternion rotation = behaviour.targetDirectionMagnitude < 0.2f ? btransformRotation : Quaternion.LookRotation(targetDirection);
                btransformRotation = Quaternion.Slerp(btransformRotation, rotation, behaviour.turnSpeed * deltaTime);

                btransformPosition += btransformForward * deltaTime * behaviour.acceleration;
                behaviour.currentPos = btransformPosition;

                if(attackTarget)
                {
                    behaviour.seek = true;
                    behaviour.destination = targetToAttack.transform.position;
                }
                else
                {
                    bool lookForRandomPos = true;
                    if (forceGroupAreaDestination) lookForRandomPos = !behaviour.outOfBounds;

                    if (lookForRandomPos)
                    {
                        behaviour.seek = true;

                        Vector3 newDestination = behaviour.destination;
                        if ((newDestination - behaviour.currentPos).magnitude < 2f)
                        {
                            float xr = Random.Range(minX, maxX);
                            float yr = Random.Range(minY, maxY);
                            float zr = Random.Range(minZ, maxZ);

                            newDestination.x = xr;
                            newDestination.y = yr;
                            newDestination.z = zr;
                            behaviour.destination = newDestination;
                        }
                    }
                }

                behaviour.outOfBounds = false;
                if (btransformPosition.x > maxX || btransformPosition.x < minX ||
                    btransformPosition.y > maxY || btransformPosition.y < minY ||
                    btransformPosition.z > maxZ || btransformPosition.z < minZ)
                {
                    behaviour.destination = groupAnchor;
                    behaviour.outOfBounds = true;
                    behaviour.seek = true;

                    if (forceGroupAreaDestination)
                    {
                        rotation = Quaternion.LookRotation(groupAnchor - btransformPosition);
                        btransformRotation = Quaternion.Slerp(btransformRotation, rotation, behaviour.turnSpeed * deltaTime);
                    }
                }
            
                if (refreshVariables)
                {
                    behaviour.acceleration = Random.Range(minAcceleration, maxAcceleration);
                    behaviour.speed = Random.Range(minSpeed, maxSpeed);
                    behaviour.force = Random.Range(minForce, maxForce);
                    behaviour.turnSpeed = Random.Range(minTurnSpeed, maxTurnSpeed);
                }

                {
                    btransform.rotation = btransformRotation;
                    btransform.position = btransformPosition;
                }

                if(OnUpdateFishEvent != null)
                {
                    behaviour = OnUpdateFishEvent(behaviour);
                }
                behaviours[i] = behaviour;
            }

            refreshVariables = false;

            if (frameSkipAmountCount == 0)
            {
                if (!requestedCalculation)
                {       
                    for (int i = 0; i < queueToClear.Count; i++)
                    {
                        int behaviourIndex = behaviours.FindIndex(it => it.id == queueToClear[i]);

                        FishBehaviour behaviour = behaviours[behaviourIndex];

                        Transform t = behaviour.transform;
                        agentsTransforms.Remove(t);

                        behaviours.RemoveAt(behaviourIndex);
                        Destroy(t.gameObject, 2f);

                        fishAmount--;
                        lastAgentsAmount = fishAmount;
                    }

                    queueToClear.Clear();
                    
                    request.behaviours = new List<FishBehaviour>(behaviours);
                    request.predators = predators;
                    request.separationScale = separationScale;
                    request.alignmentScale = alignmentScale;
                    request.cohesionScale = cohesionScale;
                    request.forceGroupAreaDestination = forceGroupAreaDestination;

                    FlockMain.Request(request);
                    requestedCalculation = true;
                }
            }
            else
            {
                RefreshAgents();
            }

            frameSkipAmountCount++;
        }

        void CheckObstacle(Vector3 pos, Vector3 forwardVec, out RaycastHit hit, float lookAheadDistance, LayerMask collisionLayer, ref Vector3 targetDirection, int flag)
        {
            //forward cast
            if (Physics.Raycast(pos, forwardVec, out hit, lookAheadDistance, collisionAvoidLayer))
            {
                if (debugDraw)
                {
                    Debug.DrawLine(pos, hit.point, Color.green, 0.1f);
                    Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green, 0.1f);
                }

                targetDirection += hit.normal * collisionAvoidAmount;
            }
        }

        void CheckForObstacles(FishBehaviour ab, ref Vector3 targetDirection, Vector3 tposition, Vector3 tforward)
        {
            RaycastHit hit;
            Vector3 forward = tforward;
            Vector3 pos = tposition;

            //if (frameSkipAmountCount == 0)

            //forward cast
            CheckObstacle(pos, forward, out hit, lookAheadDistance, collisionAvoidLayer, ref targetDirection, 0);

            //left raycast
            Vector3 left = pos;
            left.x -= .5f;
            CheckObstacle(left, forward, out hit, lookSideDistance, collisionAvoidLayer, ref targetDirection, 1);

            //right raycast
            Vector3 right = pos;
            right.x += .5f;
            CheckObstacle(right, forward, out hit, lookSideDistance, collisionAvoidLayer, ref targetDirection, 2);
        }

        IEnumerator GroupAnchorRandomMovement()
        {
            yield return new WaitForEndOfFrame();

            groupAnchorOffset = groupAnchor;

            groupAnchorOffset.x -= groupAreaWidth / 2;
            groupAnchorOffset.y -= groupAreaHeight / 2;
            groupAnchorOffset.z -= groupAreaDepth / 2;

            groupAnchorOffset.x += groupAreaWidth * Mathf.Cos(Random.Range(-360, 360) * Mathf.Deg2Rad);
            groupAnchorOffset.y += groupAreaHeight * Mathf.Sin(Random.Range(-360, 360) * Mathf.Deg2Rad);
            groupAnchorOffset.z += groupAreaDepth * Mathf.Cos(Random.Range(-360, 360) * Mathf.Deg2Rad);

            StartCoroutine(GroupAnchorRandomMovement());
        }

        void UpdateParent()
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
                futurePosition = groupAnchor + vel * Time.deltaTime * 0.8f;
            }
            else if(followTarget)
            {
                if (target != null)
                {
                    Vector3 vel = (target.position - groupAnchor);
                    futurePosition = groupAnchor + vel * Time.deltaTime * 0.8f;
                }
            }

            futurePosition.x = Mathf.Clamp(futurePosition.x, minX, maxX);
            futurePosition.y = Mathf.Clamp(futurePosition.y, minY, maxY);
            futurePosition.z = Mathf.Clamp(futurePosition.z, minZ, maxZ);

            groupAnchor = futurePosition;
        }

        void RefreshAgents()
        {
            if (lastAgentsAmount < fishAmount)
            {
                float minX = myTransform.position.x - (swimmingAreaWidth / 2) + (groupAreaWidth / 2);
                float maxX = myTransform.position.x + (swimmingAreaWidth / 2) - (groupAreaWidth / 2);
                float minY = myTransform.position.y - (swimmingAreaHeight / 2) + (groupAreaHeight / 2);
                float maxY = myTransform.position.y + (swimmingAreaHeight / 2) - (groupAreaHeight / 2);
                float minZ = myTransform.position.z - (swimmingAreaDepth / 2) + (groupAreaDepth / 2);
                float maxZ = myTransform.position.z + (swimmingAreaDepth / 2) - (groupAreaDepth / 2);

                int toAdd = fishAmount - lastAgentsAmount;

                for (int i = 0; i < toAdd; i++)
                {
                    behaviours.Add(CreateAgent(agentsTransforms, i));
                }

                lastAgentsAmount = fishAmount;
            }
            else if (lastAgentsAmount > fishAmount)
            {
                int toRemove = lastAgentsAmount - fishAmount;

                for (int i = 0; i < toRemove; i++)
                {
                    Transform t = agentsTransforms[0];
                    t.gameObject.SetActive(false);

                    behaviours.RemoveAt(0);
                    agentsTransforms.RemoveAt(0);
                    Destroy(t.gameObject, 2f);
                }

                lastAgentsAmount = fishAmount;
            }
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

                    if((i + 1)== targetPositions.Length - 1)
                        Gizmos.DrawWireSphere(targetPositions[i + 1], 1f);
                }
            }
        }

        public void OnFlockingCalcCompleted(List<FishBehaviour> behaviours)
        {          
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (i < this.behaviours.Count)
                {
                    this.behaviours[i] = behaviours[i];
                }
            }

            requestedCalculation = false;
        }

        private void OnDestroy()
        {
            FlockMain.running = false;

            if(FlockMain.flockThreadCalc != null)
            {
                FlockMain.flockThreadCalc.Abort();
                FlockMain.flockThreadCalc = null;
            }
        }

        private void OnApplicationQuit()
        {
            FlockMain.running = false;

            if (FlockMain.flockThreadCalc != null)
            {
                FlockMain.flockThreadCalc.Abort();
                FlockMain.flockThreadCalc = null;
            }
        }

    }
}