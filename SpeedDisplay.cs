using UnityEngine;
using TMPro;

/// <summary>
/// Simple GUI for β, γ, physical speed v = β c, fps and simulation error.
/// </summary>

public class SpeedDisplay : MonoBehaviour
{
	[Header("Scene Reference")]
	[SerializeField] PlayerController player;
	[SerializeField] TheoryCheck theory;

	[Header("UI Labels")]
	[SerializeField] TMP_Text speedText;    // speed in m/s
	[SerializeField] TMP_Text betaText;     // beta factor
	[SerializeField] TMP_Text gammaText;    // gamma factor
	[SerializeField] TMP_Text fpsText;      // frames per second
	[SerializeField] TMP_Text errorText;    // relative error from TheoryCheck

	private const float c = 299792458; 		 // Speed of light in m/s.
	private float deltaSmoothed;

	void Awake() 
	{										// reference fallbacks
		player ??= FindObjectOfType<PlayerController>();
		theory ??= FindObjectOfType<TheoryCheck>();
	} 

	void Update()
	{
		// calculate values
		var beta = player.Beta;				// beta factor
		var b = player.BetaVec;     		// beta vector
		var gamma = player.Gamma;   		// gamma factor
		var v = beta * c;           		// calculate speed

		deltaSmoothed = Mathf.Lerp(deltaSmoothed, Time.unscaledDeltaTime, 0.1f);
		float fps = 1f / Mathf.Max(1e-6f, deltaSmoothed);
		float err = theory ? theory.RelError : 0f;

		// Update UI text fields
		speedText?.SetText($"v: {v:E2} m/s");
		betaText?.SetText( $"β: {beta:F3}");
		gammaText?.SetText($"γ: {gamma:F3}");
		fpsText?.SetText(  $"{fps:F1} FPS");
		errorText?.SetText($"Error: {theory.RelError:E2}");

	}
}
