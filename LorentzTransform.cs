using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Contructs the Lorentz transformation matrix from the player's velocity parameters.
/// </summary>

public class LorentzTransform : MonoBehaviour
{
    PlayerController player;
    private float4x4 lorentzMatrix = float4x4.identity;

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
        UpdateLorentzMatrix();
    }

    public float4x4 GetLorentzMatrix()
    {
        return lorentzMatrix;
    }

    private void UpdateLorentzMatrix()
    {
        float beta = player.Beta;
        float3 b = player.BetaVec;
        float gamma = player.Gamma;
        float3 g = gamma * b;
        float factor = (gamma - 1f) / (beta * beta);

        // Return identity for small speed to avoid precision issues
        if (beta < 1e-6f)
        {
            lorentzMatrix = float4x4.identity;
            return;
        }

        // 2D - suppress vertical motion
        // b.y = 0f;
        // g.y = 0f;

        // Construct the Minkowski transformation matrix
        lorentzMatrix = new float4x4(
            // Col0
            new float4(gamma, g.x, g.y, g.z),
            // Col1
            new float4(
                g.x,
                1f + factor * b.x * b.x,
                factor * b.x * b.y,
                factor * b.x * b.z
            ),
            // Col2
            new float4(
                g.y,
                factor * b.y * b.x,
                1f + factor * b.y * b.y,
                factor * b.y * b.z
            ),
            // Col3
            new float4(
                g.z,
                factor * b.z * b.x,
                factor * b.z * b.y,
                1f + factor * b.z * b.z
            )
        );
    }
}


