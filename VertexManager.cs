using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

/// <summary>
/// Applies a Lorentz boost to mesh vertices for relativistic effects.
/// </summary>

public class VertexManager : MonoBehaviour
{
	[Header("Scene References")]
	public Transform 		envRoot;	// static environment parent
	public PlayerController player;		// provides β, γ and v̂
	public LorentzTransform lorentz;	// provides Lorentz matrix

	// Data structure to hold per-object mesh info.
	class MeshData
	{
		public MeshFilter	filter;		// Renderer component
		public Mesh 		workMesh;  // Mesh copy safe to modify
		public Vector3[] 	restWorld; // Cached rest-frame vertices in world space
	}
	
	readonly List<MeshData> meshes = new();	// list static meshes

	void Start()
	{
		// Auto‑set references if left empty
		player  = player  != null ? player  : FindObjectOfType<PlayerController>();
		lorentz = lorentz != null ? lorentz : FindObjectOfType<LorentzTransform>();
		envRoot = envRoot != null ? envRoot : transform;

		// Find all MeshFilter components in the environment
		foreach (var mf in envRoot.GetComponentsInChildren<MeshFilter>())
		{
			if (!mf) continue;

			var data 		= new MeshData { filter = mf };          // Create a new MeshData instance
			data.workMesh 	= mf.mesh = Instantiate(mf.sharedMesh);	// Duplicate the mesh
			var localVerts 	= data.workMesh.vertices;               // Fetch the vertices from the duplicate in local space

			// Store the local rest vertices in world space
			data.restWorld 	= new Vector3[localVerts.Length];       // Array to store rest frame world-space positions
			for (int i = 0; i < localVerts.Length; ++i)
				data.restWorld[i] = mf.transform.TransformPoint(localVerts[i]); // local → world
			meshes.Add(data);

			}
	}

	void Update()
	{
		// Get relativistic properties and Lorentz matrix
		var L	 = lorentz.Lambda;   	 	 	     // Lorentz matrix
		var pPos = (float3)player.transform.position; // Player position
		var b	 = player.BetaVec; 					 // β · v̂

		// For each mesh object, update every vertex
		foreach (var m in meshes)
		{
			var newVerts = new Vector3[m.restWorld.Length];
			for (int i = 0; i < m.restWorld.Length; i++)
			{
				// Vertex in rest frame S
				var relPos 	 = (float3)m.restWorld[i] - pPos; // Relative position
				var tRest 	 = math.dot(-b, relPos);  		 // Simultaneity time coordinate 
				var rest4 	 = new float4(tRest, relPos); 	  // X = (ct, x, y, z)
				// var rest4 	 = new float4(0, relPos); 	  // Crazy results w/o tRest!!

				// Boost to player frame S'
				var boost4 	 = math.mul(L, rest4);			 // X' = ΛX
				newVerts[i]  = m.filter.transform.InverseTransformPoint((Vector3)(pPos + boost4.yzw));
			}

			// Update mesh with new positions and recalculate bounds.
			m.workMesh.vertices = newVerts;
			m.workMesh.RecalculateBounds();
		}
	}
}

// // Create a new MeshData entry.
// MeshData data = new MeshData();
// data.filter = mf;

// // Create a runtime copy of the mesh to avoid modifying the original asset
// data.restMesh = Instantiate(mf.mesh);
// mf.mesh = data.restMesh;

// // Store the local rest vertices in world space
// Vector3[] localVerts = data.restMesh.vertices;
// data.restWorld = new Vector3[localVerts.Length];
// for (int i = 0; i < localVerts.Length; i++)
// {
//     // Convert each vertex from local space (of the mesh) to world space
//     data.restWorld[i] = mf.transform.TransformPoint(localVerts[i]);
// }
// meshObjects.Add(data);


// var newWorld = pPos + boost4.yzw;			// New world position
// Store mesh-local coordinates
// newVerts[i]  = m.filter.transform.InverseTransformPoint((Vector3)newWorld);


// Compute the vertex's position relative to player
// Vector3 relPos = m.restWorld[i] - pPos;
// Compute the time coordinate for simultaneity:
// float tEnv = Vector3.Dot(-b, relPos);
// NOTE: using -b since environment moves with -v relative player
// Build the 4-vector (t, x, y, z)
// float4 rest4 = new float4(0, relPos.x, relPos.y, relPos.z); // CRAZY WITHOUT tRest!!!
// Apply the Lorentz transformation
// float4 boost4 = math.mul(L, rest4);
// The transformed spatial coordinates are the new relative position
// Vector3 newRelPos = boost4.yzw;
// Convert the new relative position back to world space
// Vector3 newWorld = pPos + newRelPos;
// Finally, convert the world position back into the mesh object's local space
// newVerts[i] = m.meshFilter.transform.InverseTransformPoint(newWorld);

// var relPos = data.restWorld[i] - pPos;
// var timeCoord = math.dot(-betaVec, relPos);
// var rest4 = new float4(timeCoord, relPos);
// var boosted4 = math.mul(L, rest4);
// var newWorld = pPos + boosted4.yzw;         // swizzle yzw → float3
// newVertices[i] = data.meshFilter.transform.InverseTransformPoint(newWorld);