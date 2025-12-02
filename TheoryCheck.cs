using UnityEngine;

/// <summary>
/// Measures distance between two objects A and B and compares to theoretical prediction
/// </summary>

public class TheoryCheck : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField] PlayerController player;
	[SerializeField] Transform		  pointA;
	[SerializeField] Transform		  pointB;

	private Vector3	restVec; 	   // rest vector from A₀ to B₀
	private float	 L0;        	// rest length L₀ = |B₀ - A₀|
	private float	 measured; 		// L' = |B' - A'|
	private float	 expected;		// L' = L₀ * √(1 - β²*cos²θ)
	private float	 error;     	// (measured - expected)/expected
	private Renderer rendA, rendB; // renderers for A and B

	void Awake() 
	{
		pointA ??= transform;		// reference fallbacks
		pointB ??= transform;
		player ??= FindObjectOfType<PlayerController>();
	}

	void Start()
	{
		rendA = pointA.GetComponent<Renderer>();
		rendB = pointB.GetComponent<Renderer>();

		// Capture rest positions using renderer bounds (transforms not updated, only meshes)
		var A0 	= rendA.bounds.center;
		var B0 	= rendB.bounds.center;
		restVec = B0 - A0;
		L0   	= restVec.magnitude;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.M)) // log measurement on 'M' keypress
		{
			// measure current distance
			var A1 = rendA.bounds.center;
			var B1 = rendB.bounds.center;
			measured = (B1 - A1).magnitude;

			// calculate theoretical prediction
			float cosT = (L0 > 1e-6f) ? Vector3.Dot(restVec / L0, player.Vhat) : 0f;
			expected = L0 * Mathf.Sqrt(1f - player.Beta * player.Beta * cosT * cosT);

			// compute error
			error = expected > 1e-9f ? Mathf.Abs(measured - expected) / expected : 0f;

			var fps 	  = 1f / Time.unscaledDeltaTime;
			float angleDeg = Mathf.Acos(cosT) * Mathf.Rad2Deg;
			Debug.Log($"β = {player.Beta:F3}, fps = {fps:F1}, θ = {angleDeg:F2}, error = {error:E2}");
		}
	}

}