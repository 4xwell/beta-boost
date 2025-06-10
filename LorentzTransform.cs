using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Contructs the Lorentz transformation matrix from the player's velocity parameters.
/// </summary>

public class LorentzTransform : MonoBehaviour
{
	public PlayerController player;
	public float4x4 Lambda { get; private set; } = float4x4.identity;

	void Awake()
	{
		player = FindObjectOfType<PlayerController>();
		if (player == null)
		{
			Debug.LogError("LorentzTransform could not find a PlayerController!");
			enabled = false;
		}
	}

	void Update()
	{
		// Get relativistic properties
		var beta    = player.Beta;
		if (beta < 1e-6f) { Lambda = float4x4.identity; return; } // Avoid small speed issues
		var b       = player.BetaVec;
		var gamma   = player.Gamma;
		var g       = gamma * b;
		var f       = (gamma-1f)/(beta*beta);			  // factor for compactness

		// Construct the transformation matrix
		Lambda = new float4x4(
			// Col 0 (ct)
			new float4(gamma, g.x, g.y, g.z),
			// Col 1 (x)
			new float4(
				g.x,
				1f + f * b.x * b.x,
				f * b.x * b.y,
				f * b.x * b.z
			),
			// Col 2 (y)
			new float4(
				g.y,
				f * b.y * b.x,
				1f + f * b.y * b.y,
				f * b.y * b.z
			),
			// Col 3 (z)
			new float4(
				g.z,
				f * b.z * b.x,
				f * b.z * b.y,
				1f + f * b.z * b.z
			)
		);
	}

	public float4x4 GetLambda() => Lambda; // legacy function
}
