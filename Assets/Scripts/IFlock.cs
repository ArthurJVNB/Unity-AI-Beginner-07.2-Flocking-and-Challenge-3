using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game
{
	public interface IFlockValues
	{
		public float Speed { get; }
		public float AverageNeighborhoodSpeed { get; }
		public Vector3 AverageCenter { get; }
		public Vector3 Avoidance { get; }
		public Vector3 Heading { get; }
	}
}
