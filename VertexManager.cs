using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class VertexManager : MonoBehaviour
{
    [Tooltip("Parent object containing all meshes to be relativistically transformed.")]
    public Transform environmentRoot;

    [Tooltip("Reference to the player controlling the simulation.")]
    public PlayerController player;

    [Tooltip("Reference to the component computing the Lorentz boost matrix.")]
    public LorentzTransform lorentz;

    [Tooltip("The origin of the scene (defaults to the environmentRoot's position).")]
    public Vector3 sceneOrigin;

    // Data structure to hold per-object mesh info.
    class MeshData
    {
        public MeshFilter meshFilter;
        public Mesh runtimeMesh;              	// A runtime copy of the mesh (to avoid modifying the original asset)
        public Vector3[] originalWorldVertices; // The rest vertices in world space
    }

    // List of all objects (with meshes) to transform.
    List<MeshData> meshObjects = new List<MeshData>();

    void Start()
    {
        // Auto-find player and lorentz components if not assigned.
        if (player == null)
            player = FindObjectOfType<PlayerController>();
        if (lorentz == null)
            lorentz = FindObjectOfType<LorentzTransform>();

        // If no environmentRoot is assigned, assume this GameObject is the parent.
        if (environmentRoot == null)
            environmentRoot = this.transform;

        // Use the environmentRoot's position as the scene origin.
        sceneOrigin = environmentRoot.position;

        // Find all MeshFilter components under the environmentRoot.
        MeshFilter[] mfs = environmentRoot.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in mfs)
        {
            if (mf == null)
                continue;

            // Create a new MeshData entry.
            MeshData data = new MeshData();
            data.meshFilter = mf;

            // Create a runtime copy of the mesh to avoid modifying the shared asset.
            data.runtimeMesh = Instantiate(mf.mesh);
            mf.mesh = data.runtimeMesh;

            // Store the original (rest) vertices in world space.
            Vector3[] originalVertices = data.runtimeMesh.vertices;
            data.originalWorldVertices = new Vector3[originalVertices.Length];
            for (int i = 0; i < originalVertices.Length; i++)
            {
                // Convert each vertex from local space (of the mesh) to world space.
                data.originalWorldVertices[i] = mf.transform.TransformPoint(originalVertices[i]);
            }
            meshObjects.Add(data);
        }
    }

    void Update()
    {
        // Get the current Lorentz transformation matrix from your NewLorentzTransform3D.
        float4x4 L = lorentz.GetLorentzMatrix();
        Vector3 playerPos = player.transform.position;
        Vector3 betaVec = player.BetaVec;
        float gamma = player.Gamma;
        Vector3 offset = playerPos - sceneOrigin; // This offset converts world positions relative to the player.

        // For each mesh object, update every vertex.
        foreach (MeshData data in meshObjects)
        {
            Vector3[] newVertices = new Vector3[data.originalWorldVertices.Length];
            for (int i = 0; i < data.originalWorldVertices.Length; i++)
            {
                // Retrieve the vertex in its rest state (in world space).
                Vector3 restWorldPos = data.originalWorldVertices[i];

                // Compute the vertex's relative position in the scene:
                // (world position relative to the scene origin, minus the player's offset)
                Vector3 relativePos = (restWorldPos - sceneOrigin) - offset;

                // Compute the time coordinate for simultaneity:
                // t = gamma * dot(-betaVec, relativePos)
                float t_env = Vector3.Dot(-betaVec, relativePos);
				// ------------ OLD: causes over bounce of scene objects ------------
                // float t_env = gamma * Vector3.Dot(-betaVec, relativePos);

                // Build the 4-vector (t, x, y, z)
                float4 rest4 = new float4(t_env, relativePos.x, relativePos.y, relativePos.z);

                // Apply the Lorentz transformation.
                float4 transformed = math.mul(L, rest4);

                // The transformed spatial coordinates are the new relative position.
                Vector3 newRelativePos = new Vector3(transformed.y, transformed.z, transformed.w);

                // Convert the new relative position back to world space.
                Vector3 newWorldPos = playerPos + newRelativePos;

                // Finally, convert the world position back into the mesh object's local space.
                newVertices[i] = data.meshFilter.transform.InverseTransformPoint(newWorldPos);
            }

            // Update the mesh with the new vertex positions and recalculate bounds.
            data.runtimeMesh.vertices = newVertices;
            data.runtimeMesh.RecalculateBounds();
        }
    }
}