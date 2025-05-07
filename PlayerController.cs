using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third‑person controller that exposes relativistic β, γ and β⃗.
/// </summary>

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
	#region Settings
	
	[Header("Movement")]
	[SerializeField] float moveSpeed 	= 40.0f;
	[SerializeField] float sprintSpeed 	= 50.0f;
	[SerializeField] float accel = 5f;
	// [SerializeField] float friction 	= 5f;

	[Header("Jump / gravity")]
	[SerializeField] float jumpHeight 	= 5.0f;
	[SerializeField] float gravity 		= -15f;
	[SerializeField] float BounceFactor = 0.5f;

	[Header("Ground check")]
	[SerializeField] float GroundedOffset = 0.1f;
	[SerializeField] float GroundedRadius = 2.0f;
	[SerializeField] LayerMask GroundLayers;

	[Header("Camera")]
	[SerializeField] GameObject cinemachineTarget;
	[SerializeField] float pitchMin 			= -30.0f;
	[SerializeField] float pitchMax 			= 70.0f;
	float yaw;
	float pitch;
	// [SerializeField, Range(0.0f, 0.3f)] float RotationSmoothTime = 0.12f;
	// [SerializeField] float cameraAngleOverride 	= 0.0f;
	// [SerializeField] bool lockCameraPosition   = false;

	[Header("Audio")]
	public AudioClip jumpClip;
	public AudioClip landClip;
	[SerializeField, Range(0.0f, 1.0f)] float audioVol = 0.5f;
	// public AudioClip[] FootstepAudioClips;

	// ----- Internals -----
	CharacterController controller;
	PlayerInput 		input;
	Camera 				mainCam;
	// Transform 			graphicsTransform;

	Vector3 velocity;
	Vector3 prevPos;
	Vector2 moveInput;
	Vector2 lookInput;
	bool 	jumpInput;
	bool 	runInput;
	bool 	grounded;
	bool 	wasGrounded;
	// const float inputThreshold = 0.01f;

	// bool 	jumpInput = false;
	// bool 	runInput  = false;
	// bool grounded = false;
	// bool wasGrounded = false;
	// float rotationVelocity;
	
	// bool IsCurrentDeviceMouse => input.currentControlScheme == "KeyboardMouse";

	#endregion


	void Awake()
	{
		controller 	= GetComponent<CharacterController>();
		input 		= GetComponent<PlayerInput>();
		mainCam 	= Camera.main;
		prevPos 	= transform.position;
		yaw = cinemachineTarget.transform.rotation.eulerAngles.y;

		// if (mainCamera == null)

		// This is the graphics mesh of the player
		// graphicsTransform = transform.Find("Graphics");
		// if (graphicsTransform == null)
		// Debug.LogWarning("No child named 'Graphics' found on the Player.");
	}

	// void Start()
	// {
	// 	yaw = cinemachineTarget.transform.rotation.eulerAngles.y;
	// 	prevPos = transform.position;
	// }


	void Update()
	{
		GroundCheck();
		ReadInput();
		GravityAndJump();
		CameraRotation();

		controller.Move(velocity * Time.deltaTime);
		wasGrounded = grounded;

		RollBall();
	}

	// ----- Input -----
	public void OnMove	(InputValue v) => moveInput = v.Get<Vector2>();
	public void OnLook	(InputValue v) => lookInput = v.Get<Vector2>();
	public void OnJump	(InputValue v) => jumpInput = v.isPressed;
	public void OnSprint(InputValue v) => runInput = v.isPressed;

	void ReadInput()
	{
		Vector3 horizVel = new Vector3(velocity.x, 0f, velocity.z);

		Vector3 camForward = mainCam.transform.forward;
		camForward.y = 0;
		camForward.Normalize();
		Vector3 camRight = mainCam.transform.right;
		camRight.y = 0;
		camRight.Normalize();

		float targetSpeed = runInput ? sprintSpeed : moveSpeed;

		// Existing code: using camForward and camRight
		Vector3 targetDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;

		// Increase acceleration effect
		horizVel += targetDir * accel * Time.deltaTime * 1.2f; // Multiply acceleration slightly

		// Limit to targetSpeed as before
		if (horizVel.magnitude > targetSpeed)
			horizVel = horizVel.normalized * targetSpeed;

		// Instead of hardcoding 10f, increase blending factor for velocity alignment with camera forward
		if (lookInput.sqrMagnitude > 0.01f)
		{
			float currentSpeed = horizVel.magnitude;
			horizVel = Vector3.Lerp(horizVel, camForward * currentSpeed, Time.deltaTime * 15f); // Increased multiplier
		}

		// // Accelerate or decelerate
		// if (moveInput.sqrMagnitude >= inputThreshold)
		// {
		// 	Vector3 targetDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
		// 	horizontalVelocity += targetDir * acceleration * Time.deltaTime;
		// 	if (horizontalVelocity.magnitude > targetSpeed)
		// 		horizontalVelocity = horizontalVelocity.normalized * targetSpeed;
		// }
		// else
		// {
		// 	horizontalVelocity = Vector3.MoveTowards(
		// 		horizontalVelocity, Vector3.zero, friction * Time.deltaTime);
		// }

		// // Realign velocity with look direction
		// if (lookInput.sqrMagnitude > 0.01f)
		// {
		// 	float currentSpeed = horizontalVelocity.magnitude;
		// 	horizontalVelocity = Vector3.Lerp(
		// 		horizontalVelocity, camForward * currentSpeed, Time.deltaTime * 10f);
		// }

		velocity.x = horizVel.x;
		velocity.z = horizVel.z;

		// Smoothly rotate the player to match movement direction
		// Vector3 horizMove = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
		// if (horizMove.sqrMagnitude > 0.001f)
		// {
		// 	float targetRotation = Mathf.Atan2(horizMove.x, horizMove.z) * Mathf.Rad2Deg;
		// float smoothedRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, RotationSmoothTime);
		// 	transform.rotation = Quaternion.Euler(0f, smoothedRotation, 0f);
		// }
	}

	void GravityAndJump()
	{
		if (jumpInput && grounded)
		{
			// velocity.y = Mathf.Sqrt(jumpHeight * -1f * gravity);
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
			if (jumpClip)
				AudioSource.PlayClipAtPoint(jumpClip, transform.position, audioVol);
			jumpInput = false;
		}

		if (!grounded)
		{
			velocity.y += gravity * Time.deltaTime;
		}
		else
		{
			// Bounce if you land with downward velocity
			if (!wasGrounded && velocity.y < -1f)
			{
				velocity.y = -velocity.y * BounceFactor;
				if (landClip)
					AudioSource.PlayClipAtPoint(landClip, transform.position, audioVol);
			}
			if (velocity.y < 0f) velocity.y = 0f;
		}
	}


	void GroundCheck()
	{
		Vector3 spherePos = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		grounded = Physics.CheckSphere(spherePos, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
	}

	void CameraRotation()
	{
		// if (lookInput.sqrMagnitude >= inputThreshold && !lockCameraPosition)
		if (lookInput.sqrMagnitude >= 0.01f)
		{
			float mult = input.currentControlScheme == "KeyboardMouse" ? 1f : Time.deltaTime;
			yaw 	 += lookInput.x * mult;
			pitch 	 += lookInput.y * mult;

			// float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
			// yaw += lookInput.x * deltaTimeMultiplier;
			// pitch += lookInput.y * deltaTimeMultiplier;
			// float stickSensitivity = 0.01f; // for example
			// yaw   += lookInput.x * stickSensitivity * Time.deltaTime;
			// pitch += lookInput.y * stickSensitivity * Time.deltaTime;
		}

		yaw = ClampAngle(yaw, float.MinValue, float.MaxValue);
		pitch = ClampAngle(pitch, pitchMin, pitchMax);

		cinemachineTarget.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
			// pitch + cameraAngleOverride, yaw, 0f);
	}

	static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f) angle += 360f;
		if (angle > 360f) angle -= 360f;
		return Mathf.Clamp(angle, min, max);
	}

	void RollBall()
	{
		// Rolling animation of character ball
		Transform gfx = transform.Find("Graphics");
		if (!gfx) return;

		Vector3 disp = transform.position - prevPos; prevPos = transform.position;
		Vector3 horiz = new(disp.x, 0, disp.z);
		if (horiz.sqrMagnitude < 1e-4f) return;

		float radius = 0.5f;
		float angle  = -horiz.magnitude / radius * Mathf.Rad2Deg;
		Vector3 axis = Vector3.Cross(horiz.normalized, Vector3.up);
		gfx.Rotate(axis, angle, Space.World);

	}

	// ----- Public Relativistic Properties -----
	public Vector3 Velocity => velocity;
	public float	Beta 	 => Mathf.Clamp(velocity.magnitude / moveSpeed, 0f, 0.99f);
	public float	Gamma	 => 1f / Mathf.Sqrt(1f - Beta * Beta);
	public Vector3 VelDir   => velocity.sqrMagnitude > 1e-6f ? velocity.normalized : Vector3.forward;
	public float3	BetaVec	 => VelDir * Beta;
}


// ------------ Rolling animation ------------
// Advanced rolling animation using displacement
// if (graphicsTransform != null)
// {
// 	Vector3 currentPos = transform.position;
// 	Vector3 displacement = currentPos - prevPos;
// 	prevPos = currentPos; // Update for next frame

// 	// Only consider horizontal displacement
// 	Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
// 	if (horizontalDisplacement.sqrMagnitude > 0.0001f)
// 	{
// 		// ballRadius must match the visual model (0.5f here as an example)
// 		float ballRadius = 0.5f;
// 		// Compute the angle (in radians) that the ball should have rotated: distance / radius
// 		float angleRadians = -horizontalDisplacement.magnitude / ballRadius;
// 		float angleDegrees = angleRadians * Mathf.Rad2Deg;

// 		// The rotation axis is perpendicular to the displacement and up vector
// 		Vector3 rotationAxis = Vector3.Cross(horizontalDisplacement.normalized, Vector3.up);
// 		graphicsTransform.Rotate(rotationAxis, angleDegrees, Space.World);
// 	}
// }
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