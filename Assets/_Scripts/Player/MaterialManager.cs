using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//using UnityEngine.Rendering.PostProcessing;
using Newtonsoft.Json;

[System.Serializable]
public class MaterialManagerKeywordedDecal {
	public string keyword = "";
	public GameObject decal;
}

public class MaterialManager : MonoBehaviour {

	public GameObject genericDecal;
	public List<MaterialManagerKeywordedDecal> keywordedDecals;
	public Dictionary<string, GameObject> decals = new Dictionary<string, GameObject>();
	[HideInInspector] public List<string> keywords = new List<string>();

	public static MaterialManager instance;

	void Awake() {
		instance = this;
		foreach (MaterialManagerKeywordedDecal decal in keywordedDecals) {
			decals [decal.keyword] = decal.decal;
			if (!keywords.Contains (decal.keyword)) {
				keywords.Add (decal.keyword);
			}
		}
	}

	public static Material GetMaterialFromRaycast(RaycastHit hit) { 
		if (hit.triangleIndex == -1) {
			return null;
		}

		//MeshFilter filter = hit.transform.GetComponent<MeshFilter> ();
		Renderer renderer = hit.transform.GetComponent<Renderer> ();
		MeshCollider collider = hit.collider as MeshCollider;
		Mesh mesh = collider.sharedMesh;

		int[] triangleIndexes = new int[] {
			mesh.triangles[(hit.triangleIndex * 3)], 
			mesh.triangles[(hit.triangleIndex * 3) + 1],
			mesh.triangles[(hit.triangleIndex * 3) + 2]
		};

		int materialIndex = -1;
		for(int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++) {
			int[] triangles = mesh.GetTriangles (subMeshIndex);
			for (int index = 0; index < triangles.Length; index += 3) {
				if (triangles [index] == triangleIndexes[0] 
					&& triangles[index + 1] == triangleIndexes[1] 
					&& triangles[index + 2] == triangleIndexes[2]) {
					materialIndex = subMeshIndex;
					break;
				}
			}
		}

		if (materialIndex == -1) {
			return null;
		}

		Material material = null;
		//try {
			material = renderer.sharedMaterials [materialIndex];
		//} catch(System.Exception ex) {
		//	material = renderer.materials [materialIndex];
		//}

		return material;
	}

	public static void PlaceHitDecal(RaycastHit hit) {
		Material material = GetMaterialFromRaycast (hit);
		foreach (string keyword in instance.keywords) {
			if (material.name.Contains (keyword)) {
				GameObject decalObject = Instantiate (instance.decals [keyword]);
				decalObject.transform.position = hit.point;
				decalObject.transform.rotation = Quaternion.LookRotation (-hit.normal);
				_Decal.Decal decalData1 = decalObject.GetComponent<_Decal.Decal> ();
				decalData1.pushDistance = Random.Range (0.001f, 0.03f);
				_Decal.DecalBuilder.BuildAndSetDirty (decalObject.GetComponent<_Decal.Decal> ());
				break;
			}
		}

		GameObject decalObject1 = Instantiate (instance.genericDecal);
		decalObject1.transform.position = hit.point;
		decalObject1.transform.rotation = Quaternion.LookRotation (-hit.normal);
		_Decal.Decal decalData = decalObject1.GetComponent<_Decal.Decal> ();
		decalData.pushDistance = Random.Range (0.001f, 0.03f);
		_Decal.DecalBuilder.BuildAndSetDirty (decalData);
	}

}
