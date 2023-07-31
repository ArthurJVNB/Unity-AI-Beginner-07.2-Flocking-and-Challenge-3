using System.Collections;
using UnityEngine;

namespace _Game.Input
{
	public class InputManager : MonoBehaviour
	{
		public static Vector2 MouseDelta { get; private set; }
		public static Vector3 Movement { get; private set; }
		
		private InputActions input;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			input = new();
		}

		private void OnEnable() => input.Enable();

		private void OnDisable() => input.Disable();

		private void Update()
		{
			MouseDelta = input.Gameplay.Look.ReadValue<Vector2>();
			Movement = input.Gameplay.Movement.ReadValue<Vector3>();
		}
	}
}