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

    // public TMP_Text speedText; // UI Text component for speed.
    // public TMP_Text betaText; // UI Text component for beta (v/c).
    // public TMP_Text lorentzText; // UI math readout.

    // private PlayerController player;
    // private LorentzTransform lorentzTransform;
    // private NewTransformManager relativisticSceneManager;

    const float c = 299792458; // Speed of light in m/s.
    // private readonly float c = 299792458; // Speed of light in m/s.


    // void Start()
    // {
    //     player = FindObjectOfType<PlayerController>();
    //     // lorentzTransform = FindObjectOfType<LorentzTransform>();
    //     // relativisticSceneManager = FindObjectOfType<NewTransformManager>();
    //     // float beta = playerController.Beta;
    // }

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
        // gammaText?.SetText($"γ      {gamma:F3}\nβ⃗     ({βvec.x:F2}, {βvec.y:F2}, {βvec.z:F2})");

        // Get the player's velocity and compute speed (m/s).
        // float speed = player.GetVelocity().magnitude;
        // Get the speed factor (β).
        // float beta = player.Beta;
        // float speed = player.Beta * c;
        // float moveSpeed = player.MoveSpeed;
        // Vector3 velocity = player.Velocity;
        // float betaX = Mathf.Clamp(velocity.x / moveSpeed, 0f, 0.99f);
        // float betaY = Mathf.Clamp(velocity.y / moveSpeed, 0f, 0.99f);
        // float betaZ = Mathf.Clamp(velocity.z / moveSpeed, 0f, 0.99f);
        // string speedExponential = $"{speed:E2}".Replace("E", " * 10^");

        // // Update the UI text fields.
        // // if (speedText != null)
        // //     speedText.text = $"βx: {betaX:F2}";
        // //     speedText.text += $"βy: {betaY:F2}";
        // //     speedText.text += $"βz: {betaZ:F2}";
        // if (speedText != null)
        //     speedText.text = $"Speed: {speed:E2}" + " m/s";
        //     // speedText.text = $"Speed: " + speedExponential + " m/s";
            
        // if (betaText != null)
        //     betaText.text = $"β: {beta:F2}";
        //     // betaText.text = $"β: {player.Beta:F2}";
        // if (lorentzTransform != null && lorentzText != null)
        // {
        //     // float betaValue = player.Beta;
        //     // Prevent division by zero or negative values.
        //     float gamma = 1.0f / Mathf.Sqrt(Mathf.Max(1 - beta * beta, 1e-6f));
        //     // float gamma = 1.0f / Mathf.Sqrt(Mathf.Max(1 - player.Beta * player.Beta, 1e-6f));
        //     // lorentzText.text = $"γ = {gamma:F3}\nFull transformation: {relativisticSceneManager.useFullTransformation}\nVelocity = {playerController.GetVelocity().magnitude}";
        //     lorentzText.text = $"γ = {gamma:F3}\nBetaVec = ({player.BetaVec.x:F2}, {player.BetaVec.y:F2}, {player.BetaVec.z:F2})";
        //     // lorentzText.text = $"γ = {gamma:F3}\nVelocity = {player.GetVelocity().magnitude}";

        // }

    }
}
            // float beta = lorentzTransform.GetLengthContraction();  // Note: This returns contraction factor; to get beta use speedFactor.
            // Since our LorentzTransform computes speedFactor internally (clamped), we can compute gamma from it:
            // gamma = 1/sqrt(1-beta^2). We assume beta here is the speed factor from Player.
            // (If you want to display beta, you could get it from the Player.)
            // if (player != null)
            // {
            //     // float betaValue = player.Beta;
            //     // Prevent division by zero or negative values.
            //     float gamma = 1.0f / Mathf.Sqrt(Mathf.Max(1 - player.Beta * player.Beta, 1e-6f));
            //     lorentzText.text = $"γ = {gamma:F3}\nFull transformation: {relativisticSceneManager.useFullTransformation}";
            // }
        // }
        // else
        // {
        //     lorentzText.text = " ";
        // }