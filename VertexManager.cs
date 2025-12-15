using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

/// <summary>
/// Applies a Lorentz boost to mesh vertices for relativistic effects.
/// 
/// FRAMES
/// 	S		rest-frame of the environment
/// 	S'		player's instant rest-frame
/// 	WORLD 	Unity scene space (S)
/// 	LOCAL 	local space of each mesh object
/// 
/// PIPELINE
/// 	1	convert LOCAL → WORLD
/// 	2	create X = (ct, x) in S
/// 	3	boost to X' = XΛ in S'
/// 	4	map to 3D WORLD
/// 	5	convert WORLD → LOCAL
/// </summary>

public class VertexManager : MonoBehaviour
{
	[Header("Scene references")]
	[SerializeField] Transform 		  envRoot;	// static environment parent
	[SerializeField] PlayerController player;  	// provides β, γ and v̂
	[SerializeField] LorentzTransform lorentz;  // provides Lorentz matrix Λ

	// mesh info data structure
	class MeshData
	{
		public MeshFilter filter;		 // renderer component
		public Mesh 	  workMesh; 	// mesh copy (safe to modify)
		public Vector3[]  worldRest; 	// rest vertices in WORLD
		public Vector3[]  localBoost; 	// boosted vertices in LOCAL
		public Matrix4x4  worldToLocal; // cached WORLD → LOCAL matrix
	}

	private readonly List<MeshData> meshes = new(); // list static meshes

	void Awake()
	{
		envRoot ??= transform;			// reference fallbacks
		player  ??= FindObjectOfType<PlayerController>();
		lorentz ??= FindObjectOfType<LorentzTransform>();
	}

	void Start()
	{
		// find all meshes in the environment
		foreach (var mf in envRoot.GetComponentsInChildren<MeshFilter>())
		{
			if (!mf) continue;

			var data = new MeshData { filter = mf };        // new MeshData instance
			data.workMesh = mf.mesh = Instantiate(mf.sharedMesh); // duplicate mesh
			data.workMesh.MarkDynamic();                           // dynamic update flag

			var localVerts = data.workMesh.vertices;          // LOCAL vertices from duplicate
			data.worldRest = new Vector3[localVerts.Length];  // allocate buffers
			data.localBoost = new Vector3[localVerts.Length];
			data.worldToLocal = mf.transform.worldToLocalMatrix; // WORLD → LOCAL matrix

			// LOCAL → WORLD
			for (int i = 0; i < localVerts.Length; ++i)
				data.worldRest[i] = mf.transform.TransformPoint(localVerts[i]);

			meshes.Add(data);

		}
		int meshCount = meshes.Count;
		int totalVerts = 0;
		foreach (var m in meshes) totalVerts += m.workMesh.vertexCount;

		Debug.Log($"Meshes: {meshCount}, Vertices: {totalVerts}");
	}

	void Update()
	{
		// get relativistic properties and Lorentz matrix
		var L 	 = lorentz.Lambda;                   // Lorentz matrix Λ
		var pPos = (float3)player.transform.position; // player position in S
		var b 	 = player.BetaVec;                   // β · v̂

		// boost each vertex in each mesh
		foreach (var m in meshes)
		{
			for (int i = 0; i < m.worldRest.Length; i++)
			{
				// create 4-vector from player to vertex in S
				var relPos	= (float3)m.worldRest[i] - pPos;
				var ct		= math.dot(-b, relPos);		// simultaneity slice
				var X 		= new float4(ct, relPos);    // X = (ct, x, y, z)

				// boost to S' → WORLD (3D) → LOCAL
				var Xprime	= math.mul(L, X);           // X' = ΛX
				var worldPrime = (Vector3)(pPos + Xprime.yzw);
				m.localBoost[i] = m.worldToLocal.MultiplyPoint3x4(worldPrime);
			}

			// Update mesh positions & recalculate bounds
			m.workMesh.vertices = m.localBoost;
			m.workMesh.RecalculateBounds();
		}
	}
}
