using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

/// <summary>
/// Third‑person controller that exposes relativistic β, γ and β⃗.
/// A modified version of the Unity Standard Assets ThirdPersonController.
/// </summary>

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerController : MonoBehaviour
{
	#region[#213] Settings

	[Header("Movement")]
	[SerializeField] float moveSpeed = 80.0f;
	[SerializeField] float accel = 10f;
	[SerializeField] float turnRate = 15f;
	private Vector3 	  velocity;

	[Header("Jump / gravity")]
	[SerializeField] float jumpHeight = 15f;
	[SerializeField] float gravity = 30f;
	[SerializeField] float BounceFactor = 0.5f;

	[Header("Ground check")]
	[SerializeField] float groundedOffset = 0.1f;
	[SerializeField] float groundedRadius = 2.0f;
	[SerializeField] LayerMask groundLayers;

	[Header("Camera")]
	[SerializeField] GameObject cinemachineTarget;
	private Camera mainCam;
	private CinemachineFreeLook freeLook;
	private CinemachineFreeLook.Orbit[] orbitsDefault;
	public  float 		   baseSenseX;
	public  float 		   baseSenseY;

	public float LookSpeedX
	{
		// get => baseSenseX;
		set => freeLook.m_XAxis.m_MaxSpeed = freeLook.m_XAxis.m_MaxSpeed * value;
	}
	public float LookSpeedY
	{
		// get => baseSenseY;
		set => freeLook.m_YAxis.m_MaxSpeed = freeLook.m_YAxis.m_MaxSpeed * value;
	}
	public float LookSensitivity
	{
		get => baseSenseY;
		set => freeLook.m_YAxis.m_MaxSpeed = value;
	}

	[Header("UI")]
	[SerializeField] private SpeedDisplay ui;

	// ---------- Internals ----------
	private CharacterController controller;
	private Transform 	  graphics;
	private Vector3 	  prevPos;

	// ---------- Inputs ----------
	private PlayerInput   input;
	private Vector2 	  moveInput;
	private bool 		  jumpInput;

	// ---------- Physics ----------
	private bool 		  grounded;
	private bool 		  wasGrounded;
	private bool 		  flyMode;

	#endregion

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		input = GetComponent<PlayerInput>();
		graphics = transform.Find("Graphics");

		mainCam = Camera.main;
		freeLook = FindObjectOfType<Cinemachine.CinemachineFreeLook>();
		if (freeLook)
		{
			orbitsDefault = (CinemachineFreeLook.Orbit[])freeLook.m_Orbits.Clone();
			baseSenseX = freeLook.m_XAxis.m_MaxSpeed;
			baseSenseY = freeLook.m_YAxis.m_MaxSpeed;
		}
	}

	void Update()
	{
		GroundCheck();    // ground check
		MoveSteer();      // general movement
		GravityAndJump(); // vertical movement

		controller.Move(velocity * Time.deltaTime);
		wasGrounded = grounded;

		RollBall();       // rolling animation
	}


	// ---------- Input ----------
	public void OnMove(InputValue v)  => moveInput = v.Get<Vector2>();
	public void OnJump(InputValue v)  => jumpInput = v.isPressed;
	public void OnDigit(InputValue v) => SetBeta(v.Get<float>());
	public void OnFly(InputValue v)   => Flight();
	public void OnPause(InputValue v) => ui?.PauseScreen();


	// ---------- Movement ----------
	void MoveSteer()
	{
		// temporary velocity (contextual)
		Vector3 currentVel = flyMode ? velocity : new(velocity.x, 0f, velocity.z);
		float currentSpeed 	= currentVel.magnitude;
		float isMoving 		= moveInput.sqrMagnitude;
		// camera vectors
		Vector3 camForward = mainCam.transform.forward;
		Vector3 camRight   = mainCam.transform.right;

		if (!flyMode) { camForward.y = 0; camRight.y = 0; }

		// set target direction from camera and input
		Vector3 targetDir = (isMoving > 0.01f) ? (camForward * moveInput.y + camRight * moveInput.x).normalized : Vector3.zero;

		// limit to moveSpeed
		if (currentSpeed > moveSpeed) currentVel = currentVel.normalized * moveSpeed;

		// accelerate towards target direction
		if (isMoving > 0.01f) currentVel += targetDir * accel * Time.deltaTime;

		// align movement with camera
		// if (lookInput.sqrMagnitude > 0.01f)
		currentVel = Vector3.Lerp(currentVel, camForward * currentSpeed, Time.deltaTime * turnRate);

		// set new velocity
		if (flyMode) velocity = currentVel;
		else { velocity.x = currentVel.x; velocity.z = currentVel.z; }
	}

	void Flight()
	{
		flyMode = !flyMode; // Toggle fly mode and jump if grounded
		if (grounded) velocity.y = Mathf.Sqrt(jumpHeight * gravity);

		FlyCam();   	  // flight camera orbits
	}


	void SetBeta(float targetBeta)
	{
		// Determine direction of motion
		Vector3 dir = velocity.sqrMagnitude > 1e-6f ? Vhat : transform.forward;

		if (!flyMode) dir.y = 0; // Flatten direction when grounded
		dir.Normalize();

		// Calculate the target velocity vector and apply it
		Vector3 targetVel = dir * (targetBeta * moveSpeed);

		if (flyMode) velocity = targetVel;
		else { velocity.x = targetVel.x; velocity.z = targetVel.z; }
	}


	// ---------- Classical physics ----------
	void GravityAndJump()
	{
		if (flyMode) return;        // no gravity/jump in fly mode

		if (jumpInput && grounded) // jump if grounded
		{
			velocity.y = Mathf.Sqrt(jumpHeight * gravity);
			jumpInput  = false;
		}

		if (!grounded) velocity.y -= gravity * Time.deltaTime;  // fall if not grounded
		else
		{
			// Bounce if landing with downward velocity
			if (!wasGrounded && velocity.y < -1f) velocity.y = -velocity.y * BounceFactor;

			if (velocity.y <= 0f && !flyMode)
			{
				bool slopeAdjusted = false;
				Vector3 castOrigin   = transform.position + Vector3.up * (controller.height * 0.5f);
				float  castRadius    = controller.radius * 0.95f;
				float  castDistance  = controller.height * 0.5f + groundedOffset + 0.5f;

				if (Physics.SphereCast(castOrigin, castRadius, Vector3.down, out RaycastHit slopeHit, castDistance, groundLayers, QueryTriggerInteraction.Ignore))
				{
					if (slopeHit.normal.y > 0.001f)
					{
						Vector3 horizontalVel  = new(velocity.x, 0f, velocity.z);
						float  horizontalSpeed = horizontalVel.magnitude;

						if (horizontalSpeed > 1e-4f)
						{
							Vector3 slopeVel = Vector3.ProjectOnPlane(horizontalVel, slopeHit.normal);
							if (slopeVel.sqrMagnitude > 1e-6f)
							{
								slopeVel = slopeVel.normalized * horizontalSpeed;
								velocity.x = slopeVel.x;
								velocity.z = slopeVel.z;
								velocity.y = slopeVel.y;
								slopeAdjusted = true;
							}
						}
					}
				}

				if (!slopeAdjusted) velocity.y = 0f;
			}
			else if (velocity.y < 0f) velocity.y = 0f;
		}
	}


	void GroundCheck()
	{
		// ground check
		Vector3 spherePos = new(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
		grounded = Physics.CheckSphere(spherePos, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
	}


	// ---------- Camera ----------
	void FlyCam() // adjust camera orbits when flying
	{
		if (!freeLook || orbitsDefault == null) return;
		if (flyMode)
		{
			freeLook.m_Orbits[0].m_Height = 30f;    // top rig height
			freeLook.m_Orbits[0].m_Radius = 5f;     // top rig radius
			freeLook.m_Orbits[2].m_Height = -20f;   // bottom rig height
			freeLook.m_Orbits[2].m_Radius = 5f;     // bottom rig radius
		}
		else freeLook.m_Orbits = (CinemachineFreeLook.Orbit[])orbitsDefault.Clone(); // restore defaults
	}


	// ---------- Animation ----------
	void RollBall()
	{
		// Rolling animation of character ball
		if (!graphics) return;

		Vector3 disp 	 = transform.position - prevPos;
		prevPos 	 	 = transform.position;
		Vector3 horizVel = new(disp.x, 0, disp.z);
		if (horizVel.sqrMagnitude < 1e-4f) return;

		float radius  = 0.5f;
		float angle   = horizVel.magnitude / radius * Mathf.Rad2Deg;
		Vector3 axis = Vector3.Cross(Vector3.up, horizVel.normalized);
		graphics.Rotate(axis, angle, Space.World);
	}


	// ---------- Public Relativistic Properties ----------
	public Vector3 Velocity => velocity;
	public Vector3 Vhat 	=> velocity.normalized;
	public float 	Beta	 => Mathf.Clamp(velocity.magnitude / moveSpeed, 0f, 0.99f);
	public float3 	BetaVec	 => Vhat * Beta;
	public float 	Gamma	 => 1f / Mathf.Sqrt(1f - Beta * Beta);
}
