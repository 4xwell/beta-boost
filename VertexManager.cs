using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

/// <summary>
/// Applies a Lorentz boost to mesh vertices for relativistic effects.
/// </summary>

public class VertexManager : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField] Transform 		  envRoot;		// static environment parent
	[SerializeField] PlayerController player;			// provides β, γ and v̂
	[SerializeField] LorentzTransform lorentz;    	// provides Lorentz matrix Λ

	// Data structure to hold per-object mesh info.
	class MeshData
	{
		public MeshFilter filter;			 // Renderer component
		public Mesh 	  workMesh;  		// Mesh copy safe to modify
		public Vector3[]  restWorld; 		// Cached rest-frame vertices in world space
	}
	
	private readonly List<MeshData> meshes = new();	// list static meshes

	void Awake() {
		envRoot ??= transform;				// fallbacks
		player  ??= FindObjectOfType<PlayerController>();
		lorentz ??= FindObjectOfType<LorentzTransform>();
	}

	void Start()
	{
		// Auto‑set references if left empty
		// player  = player  != null ? player  : FindObjectOfType<PlayerController>();
		// lorentz = lorentz != null ? lorentz : FindObjectOfType<LorentzTransform>();
		// envRoot = envRoot != null ? envRoot : transform;
		player  ??= FindObjectOfType<PlayerController>();
		lorentz ??= FindObjectOfType<LorentzTransform>();
		envRoot ??= transform;

		// Find all MeshFilter components in the environment
		foreach (var mf in envRoot.GetComponentsInChildren<MeshFilter>())
		{
			if (!mf) continue;

			var data 		= new MeshData { filter = mf };          // Create a new MeshData instance
			data.workMesh 	= mf.mesh = Instantiate(mf.sharedMesh); // Duplicate the mesh
			var localVerts 	= data.workMesh.vertices;               // Fetch the vertices from the duplicate in local space
			data.restWorld 	= new Vector3[localVerts.Length];       // Store the local rest vertices in world space

			// local → world
			for (int i = 0; i < localVerts.Length; ++i)
				data.restWorld[i] = mf.transform.TransformPoint(localVerts[i]);
			meshes.Add(data);
		}
	}

	void Update()
	{
		// Get relativistic properties and Lorentz matrix
		var L	 = lorentz.Lambda;                   // Lorentz matrix Λ
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
