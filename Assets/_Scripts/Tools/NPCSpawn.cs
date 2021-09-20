using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPCSpawn : MonoBehaviour {

	public string assetBundle = "";
	public string weaponAssetBundle = "";

	public void Spawn () {
		//AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/animators"));
		if (WeaponManager.instance == null) {
			WeaponManager weaponManager = new WeaponManager ();
			weaponManager.Start ();
		}
		AnimatorsLoader.Load();
		AssetBundle bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/" + assetBundle));
		GameObject prefab = bundle.LoadAsset<GameObject> ("NPC");
		GameObject npc = Instantiate (prefab);
		npc.name = assetBundle;
		npc.transform.position = transform.position;
		npc.transform.localRotation = transform.localRotation;
		npc.transform.localScale = new Vector3 (0.9f, 0.9f, 0.9f);
		bundle.Unload (false);
		StartCoroutine (GiveWeapon (npc));
	}

	IEnumerator GiveWeapon(GameObject npc) {
		yield return new WaitForEndOfFrame ();
		WeaponManager.SetNPCWeapon (npc, weaponAssetBundle);
	}

	void Start() {
		Spawn ();
	}

	#if UNITY_EDITOR 
	private Mesh mesh;

	void OnDrawGizmos() {
		if (gameObject.name != "NPCSpawn") {
			gameObject.name = "NPCSpawn";
		}
		if (mesh == null) {
			mesh = AssetDatabase.LoadAssetAtPath<Mesh> ("Assets/_Tools/CharacterGizmos.obj");
			mesh.RecalculateNormals ();
			return;
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawWireMesh (mesh, transform.position, transform.rotation, transform.lossyScale);
		Gizmos.color = Color.white;
		Gizmos.DrawRay (transform.position, transform.forward * 1.0f);
		Gizmos.DrawCube (transform.position + (transform.forward * 1.0f), new Vector3(0.1f, 0.1f, 0.1f));
	}
	#endif

}
