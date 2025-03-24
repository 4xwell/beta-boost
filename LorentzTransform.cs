using Unity.Mathematics;
using UnityEngine;

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


/*

float beta   =  -player.Beta;
float3 b     =  -player.BetaVec;
float gamma  =   player.Gamma;
float3 g     =   gamma * b;
float factor =  (gamma - 1f) / (beta * beta);

// For small speeds, use identity to avoid precision issues.
if (beta < 1e-6f)
{
    lorentzMatrix = float4x4.identity;
    return;
}

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

// Construct the Lorentz boost matrix using the standard formula.
lorentzMatrix = new float4x4(
    new float4(
        gamma, -gamma * b.x, -gamma * b.y, -gamma * b.z),
    new float4(
        -gamma * b.x, 1f + factor * b.x * b.x, factor * b.x * b.y, factor * b.x * b.z),
    new float4(
        -gamma * b.y, factor * b.y * b.x, 1f + factor * b.y * b.y, factor * b.y * b.z),
    new float4(
        -gamma * b.z, factor * b.z * b.x, factor * b.z * b.y, 1f + factor * b.z * b.z)
);

*/


// using Unity.Mathematics;
// using UnityEngine;

// public class NewLorentzTransform3D : MonoBehaviour
// {
//     NewPlayerController player;
//     private float4x4 lorentzMatrix = float4x4.identity;

//     void Awake()
//     {
//         player = FindObjectOfType<NewPlayerController>();
//         if (!player)
//         {
//             Debug.LogError("LorentzTransform3D could not find a NewPlayerController!");
//             enabled = false;
//         }
//     }

//     void Update()
//     {
//         UpdateLorentzMatrix();
//     }

//     public float4x4 GetLorentzMatrix()
//     {
//         return lorentzMatrix;
//     }

//     private void UpdateLorentzMatrix()
//     {
//         // We want a boost by -v, so set b = - BetaVec
//         float3 b    = -player.BetaVec;
//         float beta  = -player.Beta;
//         float gamma = player.Gamma;

//         // For small speeds, just use identity (avoid floating inaccuracies)
//         if (beta < 1e-6f)
//         {
//             lorentzMatrix = float4x4.identity;
//             return;
//         }

//         float factor = (gamma - 1f) / (beta * beta);
//         float3 g = - gamma * b;

//         // Construct the Minkowski transformation matrix
//         lorentzMatrix = new float4x4(
//             // Col0
//             new float4(gamma, g.x, g.y, g.z),
//             // Col1
//             new float4(
//                 g.x,
//                 1f + factor * b.x * b.x,
//                 factor * b.x * b.y,
//                 factor * b.x * b.z
//             ),
//             // Col2
//             new float4(
//                 g.y,
//                 factor * b.y * b.x,
//                 1f + factor * b.y * b.y,
//                 factor * b.y * b.z
//             ),
//             // Col3
//             new float4(
//                 g.z,
//                 factor * b.z * b.x,
//                 factor * b.z * b.y,
//                 1f + factor * b.z * b.z
//             )
//         );

//         // Add identity so that diagonal becomes (gamma, 1+(...), 1+(...), 1+(...))
//         // L += float4x4.identity;

//         // lorentzMatrix = L;
//     }
// }


// using Unity.Mathematics;
// using UnityEngine;

// public class NewLorentzTransform3D : MonoBehaviour
// {
//     PlayerController player;
//     private float4x4 lorentzMatrix = float4x4.identity;

//     void Awake()
//     {
//         player = FindObjectOfType<PlayerController>();
//         if (!player)
//         {
//             Debug.LogError("LorentzTransform3D could not find a PlayerController!");
//             enabled = false;
//         }
//     }

//     void Update()
//     {
//         UpdateLorentzMatrix();
//     }

//     public float4x4 GetLorentzMatrix()
//     {
//         return lorentzMatrix;
//     }

//     /// <summary>
//     /// Constructs a Lorentz transformation matrix for a boost with speed β in the horizontal direction.
//     /// The matrix is built so that when applied to a 4–vector (t, x, y, z) (with t chosen as t = –β*(v̂·X_horiz)),
//     /// the transformed time coordinate vanishes. (Natural units: c = 1.)
//     /// </summary>
//     private void UpdateLorentzMatrix()
//     {
//         float beta = -player.Beta; 
//         float3 b = -player.BetaVec;
//         float gamma = player.Gamma;
//         // Vector3 vDir = player.VelocityDirection;

//         if (beta < 1e-3f)
//         {
//             lorentzMatrix = float4x4.identity;
//             return;
//         }

//         // For our transformation we boost by –β * vDir.
//         // float3 betaVec = -(vDir * beta);
//         float factor = (gamma - 1f) / (beta * beta);

//         // lorentzMatrix = float4x4.identity;
//         // // Our 4–vector ordering is: (time, x, y, z)
//         // lorentzMatrix.c0 = new float4(
// 		// 	gamma, 
// 		// 	-gamma * betaVec.x, 
// 		// 	-gamma * betaVec.y, 
// 		// 	-gamma * betaVec.z);
//         // lorentzMatrix.c1 = new float4(
// 		// 	-gamma * betaVec.x, 
// 		// 	1 + factor * betaVec.x * betaVec.x, 
// 		// 	factor * betaVec.x * betaVec.y, 
// 		// 	factor * betaVec.x * betaVec.z);
//         // lorentzMatrix.c2 = new float4(
// 		// 	-gamma * betaVec.y, 
// 		// 	factor * betaVec.y * betaVec.x, 
// 		// 	1 + factor * betaVec.y * betaVec.y, 
// 		// 	factor * betaVec.y * betaVec.z);
//         // lorentzMatrix.c3 = new float4(
// 		// 	-gamma * betaVec.z, 
// 		// 	factor * betaVec.z * betaVec.x, 
// 		// 	factor * betaVec.z * betaVec.y, 
// 		// 	1 + factor * betaVec.z * betaVec.z);

//         lorentzMatrix = new float4x4(
//             new float4(
//                 gamma - 1, 
//                 -gamma * b.x, 
//                 -gamma * b.y, 
//                 -gamma * b.z), new float4(
//                                 -gamma * b.x, 
//                                 factor * b.x * b.x, 
//                                 factor * b.x * b.y, 
//                                 factor * b.x * b.z),    new float4(
//                                                         -gamma * b.y, 
//                                                         factor * b.y * b.x, 
//                                                         factor * b.y * b.y, 
//                                                         factor * b.y * b.z),    new float4(
//                                                                                 -gamma * b.z, 
//                                                                                 factor * b.z * b.x, 
//                                                                                 factor * b.z * b.y, 
//                                                                                 factor * b.z * b.z)
//         );

//         lorentzMatrix += float4x4.identity;
//     }
// }

