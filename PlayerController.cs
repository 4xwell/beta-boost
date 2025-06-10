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
	[SerializeField] float accel 		= 5f;

	[Header("Jump / gravity")]
	[SerializeField] float jumpHeight 	= 5.0f;
	[SerializeField] float gravity 		= -15f;
	[SerializeField] float BounceFactor 	= 0.5f;

	[Header("Ground check")]
	[SerializeField] float GroundedOffset 	= 0.1f;
	[SerializeField] float GroundedRadius 	= 2.0f;
	[SerializeField] LayerMask GroundLayers;

	[Header("Camera")]
	[SerializeField] GameObject cinemachineTarget;
	[SerializeField] float pitchMin 	= -30.0f;
	[SerializeField] float pitchMax 	= 70.0f;
	float yaw;
	float pitch;

	[Header("Audio")]
	public AudioClip jumpClip;
	public AudioClip landClip;
	[SerializeField, Range(0.0f, 1.0f)] float audioVol = 0.5f;

	// ----- Internals -----
	CharacterController 	controller;
	PlayerInput 		input;
	Camera 			mainCam;

	Vector3 velocity;
	Vector3 prevPos;
	Vector2 moveInput;
	Vector2 lookInput;
	bool 	jumpInput;
	bool 	runInput;
	bool 	grounded;
	bool 	wasGrounded;

	#endregion


	void Awake()
	{
		controller 	= GetComponent<CharacterController>();
		input 		= GetComponent<PlayerInput>();
		mainCam 	= Camera.main;
		prevPos 	= transform.position;
		yaw 		= cinemachineTarget.transform.rotation.eulerAngles.y;
	}

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

		// using camForward and camRight to set target direction target
		Vector3 targetDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;

		// Increase acceleration effect
		horizVel += targetDir * accel * Time.deltaTime * 1.2f; // Multiply acceleration slightly

		// Limit to targetSpeed
		if (horizVel.magnitude > targetSpeed)
			horizVel = horizVel.normalized * targetSpeed;

		// increase blending factor for velocity alignment with camera forward
		if (lookInput.sqrMagnitude > 0.01f)
		{
			float currentSpeed = horizVel.magnitude;
			horizVel = Vector3.Lerp(horizVel, camForward * currentSpeed, Time.deltaTime * 15f); // Increased multiplier
		}

		velocity.x = horizVel.x;
		velocity.z = horizVel.z;
	}

	void GravityAndJump()
	{
		if (jumpInput && grounded)
		{
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
			// Bounce if you land with downward velocity and add sound clip
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
		if (lookInput.sqrMagnitude >= 0.01f)
		{
			float mult = input.currentControlScheme == "KeyboardMouse" ? 1f : Time.deltaTime;
			yaw 	  += lookInput.x * mult;
			pitch 	  += lookInput.y * mult;
		}

		yaw 	= ClampAngle(yaw, float.MinValue, float.MaxValue);
		pitch 	= ClampAngle(pitch, pitchMin, pitchMax);
		cinemachineTarget.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
	}

	static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f) angle += 360f;
		if (angle > 360f)  angle -= 360f;
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
