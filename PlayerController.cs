using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third‑person controller that exposes relativistic β, γ and β⃗.
/// A modified version of the Unity Standard Assets ThirdPersonController.
/// </summary>

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
	#region Settings

	[Header("Movement")]
	[SerializeField] float moveSpeed 	= 40.0f;
	[SerializeField] float sprintSpeed 	= 50.0f;
	[SerializeField] float accel		= 5f;
	// [SerializeField] float friction 	= 5f;

	[Header("Boost")]
	[SerializeField] float boostDuration = 1.0f;
	[SerializeField] float boost 		 = 5f;
	private float boostTimer = 0; // countdown timer

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
	[SerializeField] float pitchMin = -30.0f;
	[SerializeField] float pitchMax = 70.0f;
	private float yaw;
	private float pitch;

	[Header("Audio")]
	private AudioClip jumpClip;
	private AudioClip landClip;
	private AudioClip boostClip;
	[SerializeField, Range(0.0f, 1.0f)] float audioVol = 0.5f;

	// ----- Internals -----
	private CharacterController controller;
	private PlayerInput 		input;
	private Camera 				mainCam;
	private AudioSource 		audioSource;

	private Vector3 velocity;
	private Vector3 prevPos;
	private Vector2 moveInput;
	private Vector2 lookInput;

	private bool jumpInput;
	private bool runInput;
	private bool grounded;
	private bool wasGrounded;

	#endregion


	void Awake()
	{
		controller 	= GetComponent<CharacterController>();
		input 		= GetComponent<PlayerInput>();
		mainCam 	= Camera.main;
		prevPos 	= transform.position;
		yaw 		= cinemachineTarget.transform.rotation.eulerAngles.y;
		audioSource 			= gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.volume 		= audioVol;
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

		// Instant beta snap helper
		var snap = DigitBeta(); 
		if (snap.HasValue) SnapBeta(snap.Value);
	}

	// ----- Input -----
	public void OnMove(InputValue v) => moveInput = v.Get<Vector2>();
	public void OnLook(InputValue v) => lookInput = v.Get<Vector2>();
	public void OnJump(InputValue v) => jumpInput = v.isPressed;
	// public void OnSprint(InputValue v) => runInput  = v.isPressed;
	public void OnSprint(InputValue v)
	{
		bool pressed = v.isPressed;
	    if (pressed && !runInput && grounded && boostTimer <= 0f)
		{
			Debug.Log($"Run input: {runInput}");
			boostTimer = boostDuration; // Start boost timer
			if (boostClip && !audioSource.isPlaying) audioSource.PlayOneShot(boostClip);
		}
		runInput = pressed;
	}
	// public void OnSprint(InputAction.CallbackContext ctx)
	// {
	// 	runInput = ctx.ReadValueAsButton(); // true while held
	//     if (ctx.started && grounded && boostTimer <= 0f)
	// 	{
	// 		Debug.Log($"Run input: {runInput}");
	// 		boostTimer = boostDuration; // Start boost timer
	// 		if (boostClip && !audioSource.isPlaying) audioSource.PlayOneShot(boostClip);
	// 	}
	// }

	void ReadInput()
	{
		Vector3 horizVel 	= new Vector3(velocity.x, 0f, velocity.z);
		Vector3 camForward 	= mainCam.transform.forward;
		camForward.y 		= 0;
		camForward.Normalize();
		Vector3 camRight 	= mainCam.transform.right;
		camRight.y 			= 0;
		camRight.Normalize();

		float targetSpeed 	 = runInput ? sprintSpeed : moveSpeed;

		// set target direction from camForward and camRight and accelerate towards it
		Vector3 targetDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
		horizVel 		 += targetDir * accel * Time.deltaTime;
		// horizVel 		 += targetDir * accel * Time.deltaTime * 1.2f;

		// Limit to targetSpeed
		if (horizVel.magnitude > targetSpeed) horizVel = horizVel.normalized * targetSpeed;

		// increase blending factor for velocity alignment with camera forward
		if (lookInput.sqrMagnitude > 0.01f)
		{
			float currentSpeed = horizVel.magnitude;
			horizVel = Vector3.Lerp(horizVel, camForward * currentSpeed, Time.deltaTime * 15f); // Increased multiplier
		}

		velocity.x = horizVel.x;
		velocity.z = horizVel.z;

		// boost if timer is active
		if (boostTimer > 0f)
		{
			Debug.Log($"Boosting! Timer: {boostTimer}");
			velocity.x += targetDir.x * boost * accel * Time.deltaTime;
			velocity.z += targetDir.z * boost * accel * Time.deltaTime;
			// velocity += targetDir * boost * accel * Time.deltaTime;
			boostTimer -= Time.deltaTime;
			// if (boostClip) AudioSource.PlayOneShot(boostClip);
			// if (boostClip) AudioSource.PlayClipAtPoint(boostClip, transform.position, audioVol);
			// AudioSource.PlayClipAtPoint(boostClip, transform.position, audioVol);
		}
		// if (runInput) Debug.Log($"Run input: {runInput}, Velocity: {velocity}");
	}

	// One-shot snap: set horizontal speed so |v|/moveSpeed = targetBeta
	void SnapBeta(float targetBeta) {
		var dir = VelDir.sqrMagnitude > 1e-6f ? VelDir : transform.forward;
		dir.y = 0f; dir.Normalize();
		var targetHoriz = dir * (targetBeta * moveSpeed);
		velocity.x = targetHoriz.x;
		velocity.z = targetHoriz.z;
	}
	
	// Map top-row Alpha0–Alpha9 to β = 0.i (only on frames a key was pressed)
	float? DigitBeta() {
		if (!Input.anyKeyDown) return null;
		int k0 = (int)KeyCode.Alpha0;
		for (int d = 0; d <= 9; d++) {
			if (Input.GetKeyDown((KeyCode)(k0 + d))) return 0.1f * d;
		}
		return null;
	}

	void GravityAndJump()
	{
		if (jumpInput && grounded)
		{
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
			if (jumpClip) AudioSource.PlayClipAtPoint(jumpClip, transform.position, audioVol);
			jumpInput = false;
		}

		if (!grounded) velocity.y += gravity * Time.deltaTime;
		else
		{
			// Bounce if you land with downward velocity
			if (!wasGrounded && velocity.y < -1f)
			{
				velocity.y = -velocity.y * BounceFactor;
				if (landClip) AudioSource.PlayClipAtPoint(landClip, transform.position, audioVol);
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
			yaw   	 += lookInput.x * mult;
			pitch 	 += lookInput.y * mult;
		}

		yaw   = ClampAngle(yaw, float.MinValue, float.MaxValue);
		pitch = ClampAngle(pitch, pitchMin, pitchMax);

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
		float angle = -horiz.magnitude / radius * Mathf.Rad2Deg;
		Vector3 axis = Vector3.Cross(horiz.normalized, Vector3.up);
		gfx.Rotate(axis, angle, Space.World);

	}

	// ----- Public Relativistic Properties -----
	public Vector3 Velocity => velocity;
	public float 	Beta 	 => Mathf.Clamp(velocity.magnitude / moveSpeed, 0f, 0.99f);
	public float 	Gamma 	 => 1f / Mathf.Sqrt(1f - Beta * Beta);
	public Vector3 VelDir 	=> velocity.sqrMagnitude > 1e-6f ? velocity.normalized : Vector3.forward;
	public float3 	BetaVec  => VelDir * Beta;

}

