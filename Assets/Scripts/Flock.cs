using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game
{
	public class Flock : MonoBehaviour, IFlockValues
	{
		private readonly int f_speedOffset = Animator.StringToHash("f_speedOffset");
		private readonly int f_speedMultiplier = Animator.StringToHash("f_speedMultiplier");

		public FlockManager MyManager;

        #region IFlockValues
        public float Speed { get => speed ; private set => SetSpeed(value); }
		public float AverageNeighborhoodSpeed { get; private set; } = 0;
		public Vector3 AverageCenter { get; private set; } = Vector3.zero;
		public Vector3 Avoidance { get; private set; } = Vector3.zero;
		public Vector3 Heading { get; private set; } = Vector3.zero;
		#endregion

		private Flock[] neighbors = new Flock[0];
		private Animator animator;
		private float speed;

		private bool IsInsideAvoidDistance(Vector3 other) => Vector3.Distance(transform.position, other) < MyManager.AvoidanceDistance;
		private bool IsInsideSwimLimits => MyManager.SwimLimits.Contains(transform.position);
		private bool WillApplyRules => Random.Range(0, 100) < MyManager.ChanceToApplyRules;
		private bool WillUpdateSpeed => Random.Range(0, 100) < MyManager.ChanceToUpdateSpeed;

		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		private void Start()
		{
			animator.SetFloat(f_speedOffset, Random.Range(0f, 1f));
			UpdateSpeed();
		}

		private void Update()
		{
			MoveForward();
			RotateTowardsHeading();

			if (WillCollide(out RaycastHit hit))
			{
				GoAwayFromCollision(hit);
				return;
			}

			if (!IsInsideSwimLimits)
			{
				GoBackToSwimLimits();
				return;
			}

			if (WillApplyRules)
			{
				ApplyRules();
			}

		}

		private void FixedUpdate()
		{
			UpdateNeighbors();

			if (WillUpdateSpeed)
				UpdateSpeed();
		}

		/// <summary>
		/// Same as using <see cref="Speed"/> = <paramref name="newSpeed"/>.
		/// </summary>
		/// <param name="newSpeed"></param>
		private void SetSpeed(float newSpeed)
		{
			speed = newSpeed;
			animator.SetFloat(f_speedMultiplier, speed, 0.1f, Time.deltaTime);
		}

		private void UpdateSpeed()
		{
			float speed = Random.Range(MyManager.MinSpeed, MyManager.MaxSpeed);
			Speed = speed;
		}

		private bool WillCollide(out RaycastHit hitInfo)
		{
			return Physics.Raycast(transform.position, transform.forward, out hitInfo, MyManager.AvoidanceDistance);
		}

		private void UpdateNeighbors()
		{
			if (MyManager == null) return;

			List<Flock> neighbors = new List<Flock>();

			foreach (Flock fish in MyManager.AllFish)
			{
				if (fish == null) continue;
				if (fish == this) continue;

				float distance = Vector3.Distance(transform.position, fish.transform.position);
				if (distance < MyManager.NeighborDistance)
					neighbors.Add(fish);
			}

			this.neighbors = neighbors.ToArray();
		}

		private void GoAwayFromCollision(RaycastHit hitInfo)
		{
			Heading = Vector3.Reflect(transform.forward, hitInfo.normal);
			//RotateTowards(Heading);
		}

		private void GoBackToSwimLimits()
		{
			Speed = MyManager.MaxSpeed;
			Heading = MyManager.transform.position - transform.position;
			//RotateTowards(Heading);
		}

		private void ApplyRules()
		{
			if (neighbors == null) return;
			if (neighbors.Length == 0) return;

			AverageNeighborhoodSpeed = 0;
			AverageCenter = Vector3.zero;
			Avoidance = Vector3.zero;

			foreach (Flock neighbor in neighbors)
			{
				AverageNeighborhoodSpeed += neighbor.Speed;
				AverageCenter += neighbor.transform.position;

				if (IsInsideAvoidDistance(neighbor.transform.position))
				{
					Avoidance += transform.position - neighbor.transform.position;
				}
			}

			// Averaging Speed
			AverageNeighborhoodSpeed /= neighbors.Length;

			// Averaging Center
			AverageCenter /= neighbors.Length;
			AverageCenter += MyManager.GoalPosition - transform.position; // Vector towards goal position

			Speed = AverageNeighborhoodSpeed;

			// Calculating new heading
			Heading = AverageCenter + Avoidance - transform.position;

			//Vector3 heading = AverageCenter + Avoidance - transform.position;
			//if (heading == Vector3.zero) return; // Don't need to apply rotation because it's already rotated
			//RotateTowards(Heading);
		}

		private void MoveForward()
		{
			transform.Translate(0, 0, Speed * Time.deltaTime);
		}

		private void RotateTowards(Vector3 heading)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Heading), MyManager.RotationSpeed * Time.deltaTime);
		}

		private void RotateTowardsHeading()
		{
			if (Heading == Vector3.zero) return; // Don't need to apply rotation because it's already rotated
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Heading), MyManager.RotationSpeed * Time.deltaTime);
		}
	}
}
