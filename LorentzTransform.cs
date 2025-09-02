using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Contructs the Lorentz transformation matrix from the relativistic parameters.
/// </summary>

public class LorentzTransform : MonoBehaviour
{
	[SerializeField] PlayerController player;

	void Awake() => player ??= FindObjectOfType<PlayerController>(); // fallback

	public float4x4 Lambda
	{
		get
		{
			float beta = player.Beta;
			if (beta < 1e-6f) return float4x4.identity;	 // small β robustness

			float3 b 	 = player.BetaVec;
			float  gamma = player.Gamma;
			float3 g	 = gamma * b;
			float  f 	 = (gamma - 1f) / (beta * beta); // (γ - 1)/β² factor

			// column-major construction
			float4 c0 = new float4(gamma, g.x, 			g.y, 		  g.z);
			float4 c1 = new float4(g.x,   1f+f*b.x*b.x, f*b.x*b.y, 	  f*b.x*b.z);
			float4 c2 = new float4(g.y,   f*b.y*b.x, 	1f+f*b.y*b.y, f*b.y*b.z);
			float4 c3 = new float4(g.z,   f*b.z*b.x, 	f*b.z*b.y, 	  1f+f*b.z*b.z);

			return new float4x4(c0, c1, c2, c3);
		}
	}
}
