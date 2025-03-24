using UnityEngine;
// using UnityEngine.UI;
using TMPro;

public class SpeedDisplay : MonoBehaviour
{
    public TMP_Text speedText; // UI Text component for speed.
    public TMP_Text betaText; // UI Text component for beta (v/c).
    public TMP_Text lorentzText; // UI math readout.

    private PlayerController playerController;
    private LorentzTransform lorentzTransform;
    // private NewTransformManager relativisticSceneManager;

    private readonly float c = 299792458; // Speed of light in m/s.


    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        lorentzTransform = FindObjectOfType<LorentzTransform>();
        // relativisticSceneManager = FindObjectOfType<NewTransformManager>();
        // float beta = playerController.Beta;
    }

    void Update()
    {
        if (playerController != null)
        {
            // Get the player's velocity and compute speed (m/s).
            // float speed = playerController.GetVelocity().magnitude;
            // Get the speed factor (β).
            float beta = playerController.Beta;
            float speed = playerController.Beta * c;
            float moveSpeed = playerController.MoveSpeed;
            Vector3 velocity = playerController.Velocity;
            float betaX = Mathf.Clamp(velocity.x / moveSpeed, 0f, 0.99f);
            float betaY = Mathf.Clamp(velocity.y / moveSpeed, 0f, 0.99f);
            float betaZ = Mathf.Clamp(velocity.z / moveSpeed, 0f, 0.99f);
            string speedExponential = $"{speed:E2}".Replace("E", " * 10^");

            // Update the UI text fields.
            // if (speedText != null)
            //     speedText.text = $"βx: {betaX:F2}";
            //     speedText.text += $"βy: {betaY:F2}";
            //     speedText.text += $"βz: {betaZ:F2}";
            if (speedText != null)
                speedText.text = $"Speed: {speed:E2}" + " m/s";
                // speedText.text = $"Speed: " + speedExponential + " m/s";
                
            if (betaText != null)
                betaText.text = $"β: {beta:F2}";
                // betaText.text = $"β: {playerController.Beta:F2}";
            if (lorentzTransform != null && lorentzText != null)
            {
                // float betaValue = playerController.Beta;
                // Prevent division by zero or negative values.
                float gamma = 1.0f / Mathf.Sqrt(Mathf.Max(1 - beta * beta, 1e-6f));
                // float gamma = 1.0f / Mathf.Sqrt(Mathf.Max(1 - playerController.Beta * playerController.Beta, 1e-6f));
                // lorentzText.text = $"γ = {gamma:F3}\nFull transformation: {relativisticSceneManager.useFullTransformation}\nVelocity = {playerController.GetVelocity().magnitude}";
                lorentzText.text = $"γ = {gamma:F3}\nBetaVec = ({playerController.BetaVec.x:F2}, {playerController.BetaVec.y:F2}, {playerController.BetaVec.z:F2})";
                // lorentzText.text = $"γ = {gamma:F3}\nVelocity = {playerController.GetVelocity().magnitude}";
            }

        }

            // float beta = lorentzTransform.GetLengthContraction();  // Note: This returns contraction factor; to get beta use speedFactor.
            // Since our LorentzTransform computes speedFactor internally (clamped), we can compute gamma from it:
            // gamma = 1/sqrt(1-beta^2). We assume beta here is the speed factor from PlayerController.
            // (If you want to display beta, you could get it from the PlayerController.)
            // if (playerController != null)
            // {
            //     // float betaValue = playerController.Beta;
            //     // Prevent division by zero or negative values.
            //     float gamma = 1.0f / Mathf.Sqrt(Mathf.Max(1 - playerController.Beta * playerController.Beta, 1e-6f));
            //     lorentzText.text = $"γ = {gamma:F3}\nFull transformation: {relativisticSceneManager.useFullTransformation}";
            // }
        // }
        // else
        // {
        //     lorentzText.text = " ";
        // }
    }
}