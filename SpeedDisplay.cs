using UnityEngine;
using TMPro;

/// <summary>
/// Simple HUD for β, γ and physical speed v = β c.
/// </summary>

public class SpeedDisplay : MonoBehaviour
{
    [Header("UI Labels")]
    [SerializeField] TMP_Text speedText;
    [SerializeField] TMP_Text betaText;
    [SerializeField] TMP_Text gammaText;

    [Header("Scene Reference")]
    [SerializeField] PlayerController player;

    const float c = 299792458; // Speed of light in m/s.

    void Awake() => player = player != null ? player : FindObjectOfType<PlayerController>();

    void Update()
    {
        if (!player) return;

        var beta  = player.Beta;    // Beta factor (v/c)
        var b     = player.BetaVec; // Beta vector
        var gamma = player.Gamma;   // Lorentz factor
        var v     = beta * c;       // Physical speed

        speedText?.SetText($"v: {v:E2} m/s");
        betaText?.SetText($"β: {beta:F3}");
        gammaText?.SetText($"γ: {gamma:F3}");
    }
}
