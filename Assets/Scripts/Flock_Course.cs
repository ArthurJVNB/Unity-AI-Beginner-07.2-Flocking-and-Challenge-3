using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game
{
    public class Flock_Course : MonoBehaviour, IFlockValues
    {
        public FlockManager myManager;
        public float speed;

        public float Speed => speed;
        public float AverageNeighborhoodSpeed => averageSpeed;
        public Vector3 AverageCenter => averageCenter;
        public Vector3 Avoidance => avoidance;
        public Vector3 Heading => heading;

        private float averageSpeed;
        private Vector3 averageCenter;
        private Vector3 heading;
        private Vector3 avoidance;

        private void Start()
        {
            speed = Random.Range(myManager.MinSpeed, myManager.MaxSpeed);
        }

        private void Update()
        {
            ApplyRules();
            transform.Translate(0, 0, Time.deltaTime * speed);
        }

        private void ApplyRules()
        {
            GameObject[] allFish;
            allFish = myManager.AllFishAsGameObject;

            Vector3 averageCenter = Vector3.zero;
            Vector3 avoidance = Vector3.zero;
            float averageSpeed = 0;
            float neighborDistance;
            int groupSize = 0;

            foreach (GameObject currentFish in allFish)
            {
                if (currentFish == null) continue; // Ignore any null value (because this array is fulfilled over time)
                if (currentFish == gameObject) continue; // Only calculate values if not self

                // Distance to neighbor
                neighborDistance = Vector3.Distance(gameObject.transform.position, currentFish.transform.position);

                // Calculate values only if inside neighbor radius
                if (neighborDistance <= myManager.NeighborDistance)
                {
                    groupSize++;

                    averageCenter += currentFish.transform.position;

                    // Avoid only if inside avoidance radius
                    if (neighborDistance < myManager.AvoidanceDistance)
                    {
                        avoidance += transform.position - currentFish.transform.position; // Vector away from the current fish.
                    }

                    Flock_Course anotherFlock = currentFish.GetComponent<Flock_Course>();
                    averageSpeed += anotherFlock.speed;
                }
            }

            if (groupSize > 0)
            {
                averageCenter /= groupSize;
                speed = averageSpeed / groupSize;

                Vector3 heading = (averageCenter + avoidance) - transform.position;
                if (heading != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), myManager.RotationSpeed * Time.deltaTime);
                }

                // Update IFlockValues
                this.averageSpeed = averageSpeed / groupSize;
                this.averageCenter = averageCenter;
                this.avoidance = avoidance;
                this.heading = heading;
            }
        }
    }
}
