using UnityEngine;

/// <summary>
/// Measures distance between two objects A and B and compares to theoretical prediction
/// </summary>

public class TheoryCheck : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField] Transform        pointA;
	[SerializeField] Transform        pointB;
	[SerializeField] PlayerController player;

	[Header("Live values")]
    [SerializeField] float measured; // |A - B|
    [SerializeField] float expected; // L0 * sqrt(1 - beta^2 * cos^2(theta))
    [SerializeField] float relError; // (measured - expected)/expected

	Vector3  restVec;	    		// rest vector from A_0 to B_0
	float 	  L0;		    	 	 // rest length |A_0 - B_0|
	Renderer rendA, rendB;

	void Awake() 
	{
		pointA ??= transform;		// fallbacks
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
		// Capture current positions using renderer bounds
		var A1 = rendA.bounds.center;
        var B1 = rendB.bounds.center;
        measured = Vector3.Distance(A1, B1);

		// Calculate angle between rest vector and player direction
        float cosT = (L0 > 1e-6f) ? Mathf.Abs(Vector3.Dot(restVec / L0, player.VelDir)) : 0f;

		expected = L0 * Mathf.Sqrt(1f - player.Beta * player.Beta * cosT * cosT);	  // theoretical prediction
        relError = expected > 1e-9f ? Mathf.Abs(measured - expected) / expected : 0f; // relative error
        
		if (Input.GetKeyDown(KeyCode.M)) 											  // log measurement on 'M' keypress
		{
			var fps = 1f / Time.unscaledDeltaTime;
			float angleDeg = Mathf.Acos(cosT) * Mathf.Rad2Deg;
			Debug.Log($"β={player.Beta:F3}, fps={fps:F1}, θ={angleDeg:F2}, L_sim={measured:F4}, L_th={expected:F4}, error={relError:E2}");
		}
	}

	public float RelError => relError; 	  // expose error for GUI
}