using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Threading;

namespace FishFlock
{
    public struct FlockCalculationRequest
    {
        public FlockMain.OnCalculatedBehaviours callback;
        public List<FishBehaviour> behaviours;
        public Predator[] predators;
        public float separationScale;
        public float alignmentScale;
        public float cohesionScale;
        public bool forceGroupAreaDestination;
    }

    public class FlockMain
    {
        public delegate void OnCalculatedBehaviours(List<FishBehaviour> fb);

        static Vector3 vzero = new Vector3(0.0f, 0.0f, 0.0f);
        public static volatile AutoResetEvent eventLock;
        public static volatile bool running = false;

        public static Thread flockThreadCalc;

        static bool has_request = false;
        static FlockCalculationRequest the_request;

        public static void Request(FlockCalculationRequest request)
        {
            the_request = request;
            has_request = true;
        }

        static Vector3 Steer(Vector3 desiredVelocity, Vector3 curVelocity, float maxForce)
        {
            Vector3 steer = desiredVelocity - curVelocity;
            if (MagnitudeVector(steer) >= maxForce) steer = ClampMagnitude(steer, maxForce);

            return steer;
        }

        static Vector3 desiredVelocity;
        static Vector3 Seek(Vector3 target, Vector3 curPos, Vector3 curVelocity, float maxSpeed, float maxForce)
        {
            //calculate desired velocity
            desiredVelocity = target - curPos;

            //normalize and multiply by max speed
            desiredVelocity = NormalizeVector(desiredVelocity);
            desiredVelocity *= maxSpeed;

            //calculate steering
            return Steer(desiredVelocity, curVelocity, maxForce);
        }

        static Vector3 Flee(Vector3 target, Vector3 curPos, Vector3 curVelocity, float maxSpeed, float maxForce)
        {
            //calculate desired velocity
            desiredVelocity = curPos - target;

            //normalize and multiply by max speed
            desiredVelocity = NormalizeVector(desiredVelocity);
            desiredVelocity *= maxSpeed;

            //calculate steering
            return Steer(desiredVelocity, curVelocity, maxForce);
        }

        static Vector3 Evade(Vector3 target, Vector3 curPos, Vector3 curVelocity, float maxSpeed, float maxForce)
        {
            Vector3 distance = curPos - target;
            float updatesAhead = distance.magnitude / maxSpeed;

            Vector3 futurePosition = curPos + curVelocity * updatesAhead;

            return Flee(futurePosition, curPos, curVelocity, maxSpeed, maxForce);
        }

        static Vector3 tempVec;
        static Vector3 Separate(FishBehaviour agent, List<FishBehaviour> otherAgents, int agentsCount, int index, Predator[] predators)
        {
            Vector3 sum = vzero;
            int count = 0;
            float desiredSeparation = agent.radius * agent.distFromNeighbour;

            for (int i = 0; i < agentsCount; i++)
            {
                if (i != index)
                {
                    FishBehaviour otherAgent = otherAgents[i];

                    Vector3 dist = agent.currentPos - otherAgent.currentPos;
                    float distmag = MagnitudeVector(dist);

                    System.Random r = new System.Random();

                    if (distmag <= 0)
                    {
                        tempVec.x = otherAgent.currentPos.x + (r.Next((int)(-desiredSeparation + (desiredSeparation / 2)), (int)(desiredSeparation - (desiredSeparation / 2))));
                        tempVec.y = otherAgent.currentPos.y + (r.Next((int)(-desiredSeparation + (desiredSeparation / 2)), (int)(desiredSeparation - (desiredSeparation / 2))));
                        tempVec.z = otherAgent.currentPos.z + (r.Next((int)(-desiredSeparation + (desiredSeparation / 2)), (int)(desiredSeparation - (desiredSeparation / 2))));

                        dist = agent.currentPos - tempVec;
                        distmag = MagnitudeVector(dist);
                    }

                    if (distmag > 0 && distmag < desiredSeparation)
                    {
                        Vector3 diff = dist;
                        diff = NormalizeVector(diff);
                        diff /= distmag;

                        sum += diff;
                        count++;
                    }
                }
            }

            desiredSeparation = agent.radius * agent.distFromPredator;

            if (count > 0 && sum != vzero)
            {
                sum /= count;

                sum = NormalizeVector(sum);
                sum *= agent.speed;

                return Steer(sum, agent.velocity, agent.force);
            }

            return vzero;
        }

        static Vector3 Align(FishBehaviour agent, List<FishBehaviour> otherAgents, int agentsCount, int index)
        {
            Vector3 sum = vzero;
            float desiredSeparation = agent.radius * agent.distFromNeighbour;
            int count = 0;

            for (int i = 0; i < agentsCount; i++)
            {
                FishBehaviour otherAgent = otherAgents[i];
                if (i != index)
                {
                    Vector3 dist = agent.currentPos - otherAgent.currentPos;
                    float distmag = MagnitudeVector(dist);

                    if (distmag > 0 && distmag < desiredSeparation)
                    {
                        sum += otherAgent.velocity;
                        count++;

                    }
                }
            }

            if (count > 0 && sum != vzero)
            {
                sum /= count;

                sum = NormalizeVector(sum);
                sum *= agent.speed;

                return Steer(sum, agent.velocity, agent.force);
            }

            return vzero;
        }


        static Vector3 Cohesion(FishBehaviour agent, List<FishBehaviour> otherAgents, int agentsCount, int index)
        {
            Vector3 sum = vzero;
            float desiredSeparation = agent.radius * agent.distFromNeighbour;
            int count = 0;

            for (int i = 0; i < agentsCount; i++)
            {
                FishBehaviour otherAgent = otherAgents[i];

                if (i != index)
                {
                    Vector3 dist = agent.currentPos - otherAgent.currentPos;
                    float distmag = MagnitudeVector(dist);

                    if (distmag > 0 && distmag < desiredSeparation)
                    {
                        sum += otherAgent.currentPos;
                        count++;
                    }
                }
            }

            if (count > 0 && sum != vzero)
            {
                sum /= count;
                return Seek(sum, agent.currentPos, agent.velocity, agent.speed, agent.force);
            }

            return vzero;
        }

        public static void CalculationLoop()
        {
            while (running)
            {
                eventLock.WaitOne();     
                {
                    CalculateBehaviours();
                }
            }
        }

        static int behavioursLimitCount = 10;
        static int behavioursLimitC = 0;
        static void CalculateBehaviours()
        {
            if (!has_request) return;

            List<FishBehaviour> behaviours = the_request.behaviours;
            Predator[] predators = the_request.predators;
            float separationScale = the_request.separationScale;
            float alignmentScale = the_request.alignmentScale;
            float cohesionScale = the_request.cohesionScale;
            bool forceGroupAreaDestination = the_request.forceGroupAreaDestination;

            for (int i = 0; i < behaviours.Count; i++)
            {
                behavioursLimitC++;

                if (behavioursLimitC >= behavioursLimitCount)
                {
                    //Thread.Sleep(1);
                    behavioursLimitC = 0;
                }

                FishBehaviour agentBehaviour = behaviours[i];

                agentBehaviour.velocity = Vector3.zero;

                if(agentBehaviour.seek)
                {
                    Vector3 seekDestination = Seek(agentBehaviour.destination, agentBehaviour.currentPos, agentBehaviour.velocity, agentBehaviour.speed, agentBehaviour.force);

                    if (forceGroupAreaDestination)
                        agentBehaviour.velocity += seekDestination * (agentBehaviour.outOfBounds ? 3 : 1);
                    else
                        agentBehaviour.velocity += seekDestination;
                }

                if(!agentBehaviour.outOfBounds)
                {
                    for (int p = 0; p < predators.Length; p++)
                    {
                        Predator predator = predators[p];

                        float mag = MagnitudeVector(agentBehaviour.currentPos - predator.currentPosition);
                        if (mag <= predator.radius)
                        {
                            Evade(predator.currentPosition, agentBehaviour.currentPos, agentBehaviour.velocity, agentBehaviour.speed, agentBehaviour.force);
                        }
                    }
                }

                {
                    int behavioursCount = behaviours.Count;
                    Vector3 sep = Separate(agentBehaviour, behaviours, behavioursCount, i, predators);
                    Vector3 ali = Align(agentBehaviour, behaviours, behavioursCount, i);
                    Vector3 coh = Cohesion(agentBehaviour, behaviours, behavioursCount, i);

                    if (forceGroupAreaDestination)
                    {
                        if (!agentBehaviour.outOfBounds)
                        {
                            sep *= separationScale;
                            ali *= alignmentScale;
                            coh *= cohesionScale;
                        }
                        else
                        {
                            sep *= 0.5f;
                            ali *= 0.5f;
                            coh *= 0.5f;
                        }
                    }
                    else
                    {
                        sep *= separationScale;
                        ali *= alignmentScale;
                        coh *= cohesionScale;
                    }

                    agentBehaviour.velocity += sep;
                    agentBehaviour.velocity += ali;
                    agentBehaviour.velocity += coh;
                }

                agentBehaviour.targetDirection = (((agentBehaviour.currentPos + agentBehaviour.velocity * 1.4f) - agentBehaviour.currentPos)).normalized;
                agentBehaviour.targetDirectionMagnitude = agentBehaviour.targetDirection.magnitude;

                behaviours[i] = agentBehaviour;
            }

            has_request = false;
            the_request.callback(behaviours);      
        }

        static float MagnitudeVector(Vector3 v)
        {
            float x = Mathf.Pow(v.x, 2);
            float y = Mathf.Pow(v.y, 2);
            float z = Mathf.Pow(v.z, 2);

            return Mathf.Sqrt(x + y + z);
        }

        static Vector3 NormalizeVector(Vector3 v)
        {
            float length = GetLength(v.x, v.y, v.z);
            return v / length;
        }

        static float GetLength(float x, float y, float z)
        {
            return Mathf.Sqrt(x * x + y * y + z * z);
        }

        static Vector3 ClampMagnitude(Vector3 v, float maxLength)
        {
            Vector3 result = v;

            float sqrMagnitude = v.x * v.x + v.y * v.y + v.z * v.z;

            if (sqrMagnitude > maxLength * maxLength) result = NormalizeVector(result) * maxLength;

            return result;
        }
    }
}