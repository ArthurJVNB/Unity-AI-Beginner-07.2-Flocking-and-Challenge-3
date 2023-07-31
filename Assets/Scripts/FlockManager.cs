using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game
{
    public class FlockManager : MonoBehaviour
    {
        private enum FlockType
        {
            Flock,
            Flock_Course
        }

        public Flock[] AllFish { get; private set; }
        public GameObject[] AllFishAsGameObject { get; private set; }

        [Header("Flock Settings")]
        [SerializeField]
        private Flock fishPrefab;
        [SerializeField]
        private int numberOfFish = 20;
        [SerializeField]
        private Vector3 swimLimits = new(5, 5, 5);
        [SerializeField]
        private Transform goal;


        [Header("Fish Settings")]
        [SerializeField, Range(0f, 5f)]
        private float minSpeed;
        [SerializeField, Range(0f, 5f)]
        private float maxSpeed;
        [SerializeField, Range(0.1f, 10f)]
        private float rotationSpeed;
        [SerializeField, Range(1f, 10f)]
        private float neighborDistance;
        [SerializeField, Range(1f, 10f)]
        private float avoidanceDistance;

        [Header("Randomness")]
        [SerializeField, Range(0, 100)]
        private float chanceToChangeGoalPosition = 1;
        [SerializeField, Range(0, 100)]
        private int chanceToFlockUpdateSpeed = 10;
        [SerializeField, Range(0, 100)]
        private int chanceToFlockApplyRules = 30;


        [Header("EXTRA")]
        [SerializeField]
        private FlockType flockType;

        [Header("DEBUG")]
        [SerializeField]
        private bool printValues = true;
        [SerializeField]
        private bool showSwimLimits = true;
        [SerializeField]
        private bool showHeading = true;
        [SerializeField]
        private bool showCenter = true;
        [SerializeField]
        private bool showAvoidance = true;
        [SerializeField]
        private Color centerColor = new(0, 1, 0, .5f);
        [SerializeField]
        private Color avoidColor = new(1, 0, 0, .5f);
        [SerializeField]
        private Color headingColor = new(0, 0, 1, .5f);

        // Getters
        public Bounds SwimLimits => new(transform.position, swimLimits * 2f);
        public Vector3 GoalPosition => goal.position;
        public float MinSpeed => minSpeed;
        public float MaxSpeed => maxSpeed;
        public float RotationSpeed => rotationSpeed;
        public float NeighborDistance => neighborDistance;
        public float AvoidanceDistance => avoidanceDistance;
        public int ChanceToUpdateSpeed => chanceToFlockUpdateSpeed;
        public int ChanceToApplyRules => chanceToFlockApplyRules;

        private void Start()
        {
            AllFish = new Flock[numberOfFish];
            AllFishAsGameObject = new GameObject[numberOfFish];
            SpawnFishes();
            StartCheckChangingOfFlockType();
        }

        private void FixedUpdate()
        {
            if (Random.Range(0f, 100f) < chanceToChangeGoalPosition)
                goal.position = GetRandomPositionInsideSwimLimits();
        }

        private void SpawnFishes()
        {
            StartCoroutine(routine());

            IEnumerator routine()
            {
                GameObject flockContainer = new(gameObject.name + "'s" + " Container");

                for (int i = 0; i < AllFish.Length; i++)
                {
                    Vector3 position = GetRandomPositionInsideSwimLimits();

                    AllFish[i] = Instantiate(fishPrefab, position, Quaternion.identity, flockContainer.transform);
                    AllFishAsGameObject[i] = AllFish[i].gameObject;

                    AllFish[i].MyManager = this; // Flock.cs
                    AllFish[i].GetComponent<Flock_Course>().myManager = this; // Flock_Course.cs

                    AllFish[i].name = AllFish[i].name + " (" + i + ")";

                    SetFlockScript(AllFish[i].gameObject, flockType);

                    yield return null;
                }
            }
        }

        private Vector3 GetRandomPositionInsideSwimLimits()
        {
            return transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x),
                                                    Random.Range(-swimLimits.y, swimLimits.y),
                                                    Random.Range(-swimLimits.z, swimLimits.z));
        }

        private void SetFlockScript(GameObject flockToChange, FlockType flockType)
        {
            switch (flockType)
            {
                case FlockType.Flock:
                    flockToChange.GetComponent<Flock>().enabled = true;
                    flockToChange.GetComponent<Flock_Course>().enabled = false;
                    break;
                case FlockType.Flock_Course:
                    flockToChange.GetComponent<Flock>().enabled = false;
                    flockToChange.GetComponent<Flock_Course>().enabled = true;
                    break;
                default:
                    flockToChange.GetComponent<Flock>().enabled = true;
                    flockToChange.GetComponent<Flock_Course>().enabled = false;
                    break;
            }
        }

        private void StartCheckChangingOfFlockType()
        {
            StartCoroutine(routine());
            IEnumerator routine()
            {
                FlockType previousFlockType = flockType;

                while (true)
                {
                    if (flockType != previousFlockType)
                    {
                        previousFlockType = flockType;
                        foreach (GameObject fish in AllFishAsGameObject)
                        {
                            SetFlockScript(fish, flockType);
                        }
                    }

                    yield return new WaitForFixedUpdate();
                }
            }
        }



        private void OnDrawGizmos()
        {
            if (showSwimLimits)
            {
                Gizmos.DrawWireCube(transform.position, swimLimits * 2f);
            }

            if (AllFishAsGameObject == null) return;
            foreach (GameObject fish in AllFishAsGameObject)
            {
                if (fish == null) continue;

                IFlockValues fishValues;
                switch (flockType)
                {
                    case FlockType.Flock:
                        fishValues = fish.GetComponent<Flock>();
                        break;
                    case FlockType.Flock_Course:
                        fishValues = fish.GetComponent<Flock_Course>();
                        break;
                    default:
                        fishValues = fish.GetComponent<Flock>();
                        break;
                }

                // Values in text
                if (printValues)
                {
                    print("averageCenter: " + fishValues.AverageCenter + " | averageAvoidance: " + fishValues.Avoidance + " | direction: " + fishValues.Heading + " | name: " + fish.name);
                }

                if (showHeading)
                {
                    // Heading/Direction
                    Debug.DrawLine(fish.transform.position, fish.transform.position + fishValues.Heading, headingColor);
                }

                if (showCenter)
                {
                    // Center
                    Debug.DrawLine(fish.transform.position, fishValues.AverageCenter, centerColor);
                }

                if (showAvoidance)
                {
                    // Avoidance
                    if (fishValues.Avoidance != Vector3.zero)
                    {
                        // If avoidance was equal to Vector3.Zero, it would mean this fish is not avoiding anyone.
                        Debug.DrawLine(fish.transform.position, fish.transform.position + fishValues.Avoidance, avoidColor);
                    }
                }
            }
        }
    }
}
