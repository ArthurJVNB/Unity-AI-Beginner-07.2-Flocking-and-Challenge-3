using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Game.Input;
using System;

namespace _Game
{
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Camera Settings")]
		[SerializeField, Range(0.1f, 89.9f)]
		private float maxPitchAngle = 89f;
		[SerializeField, Range(0.1f, 89.9f)]
		private float minPitchAngle = 89f;
		[SerializeField, Range(0.1f, 10f)]
		private float sensitivity = 1f;

		[Header("Movement Settings")]
		[SerializeField, Range(0f, 30f)]
		private float maxSpeed = 10f;

		private float pitch = 0;
		private float yaw = 0;

		private void Awake()
		{
			pitch = transform.rotation.eulerAngles.x;
			yaw = transform.rotation.eulerAngles.y;

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update()
		{
			Look();
		}

		private void LateUpdate()
		{
			Move();
		}

		private void Look()
		{
			Vector2 mouseDelta = InputManager.MouseDelta;

			// Yaw
			yaw += mouseDelta.x * sensitivity * Time.deltaTime;

			// Pitch
			pitch += mouseDelta.y * -sensitivity * Time.deltaTime;
			pitch = Mathf.Clamp(pitch, -minPitchAngle, maxPitchAngle);

			transform.rotation = Quaternion.Euler(pitch, yaw, transform.eulerAngles.z);
		}

		private void Move()
		{
			Vector3 input = InputManager.Movement;

			Vector3 forward = transform.forward * input.z;
			Vector3 right = transform.right * input.x;
			Vector3 up = transform.up * input.y;

			Vector3 movement = forward + right + up;
			transform.position += maxSpeed * Time.deltaTime * movement;
		}

	}
}
