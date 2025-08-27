using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Contructs the Lorentz transformation matrix from the relativistic parameters.
/// </summary>

public class LorentzTransform : MonoBehaviour
{
	[SerializeField] PlayerController player;
	public float4x4 Lambda { get; private set; } = float4x4.identity;

	void Awake() => player ??= FindObjectOfType<PlayerController>(); // fallback

	void Update()
	{
		// Get relativistic properties
		var beta    = player.Beta;
		if (beta < 1e-6f) { Lambda = float4x4.identity; return; } // Avoid issues at small speeds
		var b       = player.BetaVec;
		var gamma   = player.Gamma;
		var g       = gamma * b;
		var f       = (gamma-1f)/(beta*beta); // factor for compactness

		// Construct the Minkowski transformation matrix
		Lambda = new float4x4(
			new float4(gamma, g.x, g.y, g.z),  // Col 0 (ct)
			new float4(                        // Col 1 (x)
				g.x,
				1f + f * b.x * b.x,
				f * b.x * b.y,
				f * b.x * b.z
			),
			new float4(                        // Col 2 (y)
				g.y,
				f * b.y * b.x,
				1f + f * b.y * b.y,
				f * b.y * b.z
			),
			new float4(                        // Col 3 (<y>)
				g.z,
				f * b.z * b.x,
				f * b.z * b.y,
				1f + f * b.z * b.z
			)
		);
	}

	public float4x4 GetLambda() => Lambda; 	   // legacy function

}