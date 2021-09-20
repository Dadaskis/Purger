using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerSpawn : MonoBehaviour {
	
	public void Spawn () {
		AssetBundle bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/player"));
		GameObject prefab = bundle.LoadAsset<GameObject> ("Player");
		GameObject player = Instantiate (prefab);
		player.name = "Player";
		player.transform.position = transform.position + new Vector3(0.0f, 0.8f, 0.0f);
		player.transform.localRotation = transform.localRotation;
		bundle.Unload (false);
	}

	void Start() {
		if (FindObjectsOfType<PlayerSpawn> ().GetLength (0) == 1) {
			Spawn ();
		}
	}

	#if UNITY_EDITOR 
	private Mesh mesh;

	void OnDrawGizmos() {
		if (gameObject.name != "PlayerSpawn") {
			gameObject.name = "PlayerSpawn";
		}
		if (mesh == null) {
			mesh = AssetDatabase.LoadAssetAtPath<Mesh> ("Assets/_Tools/CharacterGizmos.obj");
			mesh.RecalculateNormals ();
			return;
		}
		Gizmos.color = Color.green;
		Gizmos.DrawWireMesh (mesh, transform.position, transform.rotation, transform.lossyScale);
		Gizmos.color = Color.white;
		Gizmos.DrawRay (transform.position, transform.forward * 1.0f);
		Gizmos.DrawCube (transform.position + (transform.forward * 1.0f), new Vector3(0.1f, 0.1f, 0.1f));
	}
	#endif
}
