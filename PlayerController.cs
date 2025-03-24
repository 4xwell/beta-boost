using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player control, physics, and camera.
/// </summary>

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
	#region Properties and Variables
	// ----- Movement Settings -----
	[Header("Movement Settings")]
	public float MoveSpeed = 5.0f;
	public float SprintSpeed = 7.0f;
	public float Acceleration = 10f;
	public float Friction = 5f;

	// ----- Jump and Gravity Settings -----
	[Header("Jump & Gravity")]
	public float JumpHeight = 1.2f;
	public float Gravity = -15f;
	public float BounceFactor = 0.6f;

	// ----- Ground Check Settings -----
	[Header("Ground Check")]
	public float GroundedOffset = 0.5f;
	public float GroundedRadius = 0.28f;
	public LayerMask GroundLayers;

	// ----- Camera and Rotation Settings -----
	[Header("Rotation & Camera")]
	[Range(0.0f, 0.3f)]
	public float RotationSmoothTime = 0.12f;
	public GameObject CinemachineCameraTarget;
	public float TopClamp = 70.0f;
	public float BottomClamp = -30.0f;
	public float CameraAngleOverride = 0.0f;
	public bool LockCameraPosition = false;

	// ----- Audio Settings -----
	[Header("Audio")]
	public AudioClip JumpingAudioClip;
	public AudioClip LandingAudioClip;
	public AudioClip[] FootstepAudioClips;
	[Range(0, 1)]
	public float FootstepAudioVolume = 0.5f;

	// ----- Private Components and Variables -----
	CharacterController controller;
	PlayerInput playerInput;
	Camera mainCamera;
	Transform graphicsTransform;

	Vector2 moveInput = Vector2.zero;
	Vector2 lookInput = Vector2.zero;
	bool jumpInput = false;
	bool sprintInput = false;

	Vector3 velocity = Vector3.zero;
	bool grounded = false;
	bool wasGrounded = false;
	float rotationVelocity;
	float cinemachineTargetYaw;
	float cinemachineTargetPitch;
	const float inputThreshold = 0.01f;
	private Vector3 previousPosition;

	bool IsCurrentDeviceMouse => playerInput.currentControlScheme == "KeyboardMouse";

	#endregion

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		playerInput = GetComponent<PlayerInput>();
		if (mainCamera == null)
			mainCamera = Camera.main;

		// This is the graphics mesh of the player
		graphicsTransform = transform.Find("Graphics");
		if (graphicsTransform == null)
			Debug.LogWarning("No child named 'Graphics' found on the Player.");
	}

	void Start()
	{
		cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
    	previousPosition = transform.position;
	}

	void Update()
	{
		GroundedCheck();
		ProcessInput();
		ProcessGravityAndJump();
		CameraRotation();

		controller.Move(velocity * Time.deltaTime);
		wasGrounded = grounded;

		// ------------ Rolling animation ------------
		// Advanced rolling animation using displacement
		if (graphicsTransform != null)
		{
			Vector3 currentPos = transform.position;
			Vector3 displacement = currentPos - previousPosition;
			previousPosition = currentPos; // Update for next frame

			// Only consider horizontal displacement
			Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
			if (horizontalDisplacement.sqrMagnitude > 0.0001f)
			{
				// ballRadius must match the visual model (0.5f here as an example)
				float ballRadius = 0.5f;
				// Compute the angle (in radians) that the ball should have rotated: distance / radius
				float angleRadians = -horizontalDisplacement.magnitude / ballRadius;
				float angleDegrees = angleRadians * Mathf.Rad2Deg;

				// The rotation axis is perpendicular to the displacement and up vector
				Vector3 rotationAxis = Vector3.Cross(horizontalDisplacement.normalized, Vector3.up);
				graphicsTransform.Rotate(rotationAxis, angleDegrees, Space.World);
			}
		}
		// Simple rolling animation
		// if (graphicsTransform != null)
		// {
		// 	Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
		// 	if (horizontalVelocity.magnitude > 0.001f)
		// 	{
		// 		float ballRadius = 0.5f;
		// 		float angleDegrees = (horizontalVelocity.magnitude * Time.deltaTime) / ballRadius * Mathf.Rad2Deg;
		// 		Vector3 rotationAxis = Vector3.Cross(Vector3.up, horizontalVelocity.normalized);
		// 		graphicsTransform.Rotate(rotationAxis, angleDegrees, Space.World);
		// 	}
		// }
	}

	// ----- Input Callbacks -----
	public void OnMove(InputValue value) 	{ moveInput = value.Get<Vector2>(); }
	public void OnLook(InputValue value) 	{ lookInput = value.Get<Vector2>(); }
	public void OnJump(InputValue value) 	{ jumpInput = value.isPressed; }
	public void OnSprint(InputValue value) 	{ sprintInput = value.isPressed; }

	void ProcessInput()
	{
		Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

		Vector3 cameraForward = mainCamera.transform.forward;
		cameraForward.y = 0;
		cameraForward.Normalize();
		Vector3 cameraRight = mainCamera.transform.right;
		cameraRight.y = 0;
		cameraRight.Normalize();

		float targetSpeed = sprintInput ? SprintSpeed : MoveSpeed;

		// Existing code: using cameraForward and cameraRight
		Vector3 desiredDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

		// Increase acceleration effect
		horizontalVelocity += desiredDirection * Acceleration * Time.deltaTime * 1.2f; // Multiply acceleration slightly

		// Limit to targetSpeed as before
		if (horizontalVelocity.magnitude > targetSpeed)
			horizontalVelocity = horizontalVelocity.normalized * targetSpeed;

		// Instead of hardcoding 10f, increase blending factor for velocity alignment with camera forward
		if (lookInput.sqrMagnitude > 0.01f)
		{
			float currentSpeed = horizontalVelocity.magnitude;
			horizontalVelocity = Vector3.Lerp(horizontalVelocity, cameraForward * currentSpeed, Time.deltaTime * 15f); // Increased multiplier
		}

		// // Accelerate or decelerate
		// if (moveInput.sqrMagnitude >= inputThreshold)
		// {
		// 	Vector3 desiredDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
		// 	horizontalVelocity += desiredDirection * Acceleration * Time.deltaTime;
		// 	if (horizontalVelocity.magnitude > targetSpeed)
		// 		horizontalVelocity = horizontalVelocity.normalized * targetSpeed;
		// }
		// else
		// {
		// 	horizontalVelocity = Vector3.MoveTowards(
		// 		horizontalVelocity, Vector3.zero, Friction * Time.deltaTime);
		// }

		// // Realign velocity with look direction
		// if (lookInput.sqrMagnitude > 0.01f)
		// {
		// 	float currentSpeed = horizontalVelocity.magnitude;
		// 	horizontalVelocity = Vector3.Lerp(
		// 		horizontalVelocity, cameraForward * currentSpeed, Time.deltaTime * 10f);
		// }

		velocity.x = horizontalVelocity.x;
		velocity.z = horizontalVelocity.z;

		// Smoothly rotate the player to match movement direction
		// Vector3 horizMove = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
		// if (horizMove.sqrMagnitude > 0.001f)
		// {
		// 	float targetRotation = Mathf.Atan2(horizMove.x, horizMove.z) * Mathf.Rad2Deg;
		// 	float smoothedRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, RotationSmoothTime);
		// 	transform.rotation = Quaternion.Euler(0f, smoothedRotation, 0f);
		// }
	}

	void ProcessGravityAndJump()
	{
		if (jumpInput && grounded)
		{
			velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
			if (JumpingAudioClip)
				AudioSource.PlayClipAtPoint(JumpingAudioClip, transform.position, FootstepAudioVolume);
			jumpInput = false;
		}

		if (!grounded)
		{
			velocity.y += Gravity * Time.deltaTime;
		}
		else
		{
			// Bounce if you land with downward velocity
			if (!wasGrounded && velocity.y < -1f)
			{
				velocity.y = -velocity.y * BounceFactor;
				if (LandingAudioClip)
					AudioSource.PlayClipAtPoint(LandingAudioClip, transform.position, FootstepAudioVolume);
			}
			if (velocity.y < 0f) velocity.y = 0f;
		}
	}

	// ----- Grounded Check -----
	void GroundedCheck()
	{
		Vector3 spherePos = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		grounded = Physics.CheckSphere(spherePos, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
	}

	// Ground check using SphereCast
	// void GroundedCheck()
	// {
	// 	// Start the sphere cast slightly above the player's feet
	// 	Vector3 sphereCastOrigin = transform.position + Vector3.up * 0.1f;
	// 	float sphereCastRadius = GroundedRadius;
	// 	float sphereCastDistance = GroundedOffset + 0.2f; // Increase distance slightly for uneven surfaces
	// 	RaycastHit hit;
	// 	grounded = Physics.SphereCast(sphereCastOrigin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, GroundLayers, QueryTriggerInteraction.Ignore);
	// }

	void CameraRotation()
	{
		if (lookInput.sqrMagnitude >= inputThreshold && !LockCameraPosition)
		{
			float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
			cinemachineTargetYaw += lookInput.x * deltaTimeMultiplier;
			cinemachineTargetPitch += lookInput.y * deltaTimeMultiplier;
			// float stickSensitivity = 0.01f; // for example
			// cinemachineTargetYaw   += lookInput.x * stickSensitivity * Time.deltaTime;
			// cinemachineTargetPitch += lookInput.y * stickSensitivity * Time.deltaTime;
		}

		cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
		cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

		CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
			cinemachineTargetPitch + CameraAngleOverride,
			cinemachineTargetYaw,
			0f
		);
	}

	static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f) angle += 360f;
		if (angle > 360f) angle -= 360f;
		return Mathf.Clamp(angle, min, max);
	}

	// ----- Public Relativistic Properties -----
	public Vector3 Velocity => velocity;
	public float	Beta 	 => Mathf.Clamp(velocity.magnitude / MoveSpeed, 0f, 0.99f);
	public float    Gamma	 => 1f / Mathf.Sqrt(1f - Beta * Beta);
	public Vector3 VelDir   => velocity.sqrMagnitude > 1e-6f ? velocity.normalized : Vector3.forward;
	public float3   BetaVec	 => VelDir * Beta;
	}

