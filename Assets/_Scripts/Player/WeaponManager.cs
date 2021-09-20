using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WeaponManager {

	private Mesh hands;
	private Material[] handsMaterials;
	private AssetBundle previousBundle = null;
	private AssetBundle previousPlayerWeaponBundle = null;

	private Transform playerWeaponPlacement;

	public static WeaponManager instance;

	public void Start() {
		instance = this;
	}

	public static void InitializePlayerSide() {
		SetHands ("generic");
		instance.playerWeaponPlacement = 
			Player.
			instance.
			transform.
			Find ("CrouchHelper").
			Find ("Head").
			Find ("WalkEffects").
			Find ("Camera").
			Find ("Weapon");
	}

	public static void SetHands(string name) {
		name = "wply_hands_" + name;
		AssetBundle bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/" + name));
		GameObject prefab = bundle.LoadAsset<GameObject> ("HandsMesh");
		SkinnedMeshRenderer renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer> ();
		instance.hands = renderer.sharedMesh;
		instance.handsMaterials = renderer.sharedMaterials;
		if (instance.previousBundle != null) {
			instance.previousBundle.Unload (true);
		} 
		instance.previousBundle = bundle;
	}

	private static GameObject LoadPrefab(string name, string prefabName) {
		AssetBundle bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/" + name));
		GameObject prefab = bundle.LoadAsset<GameObject> (prefabName);
		bundle.Unload (false);
		return prefab;
	}

	public Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

	private static GameObject GetPrefab(string name, string prefabName) {
		GameObject prefab;
		if (instance.prefabs.TryGetValue (name, out prefab)) {
			return prefab;
		} else {
			prefab = LoadPrefab (name, prefabName);
			instance.prefabs [name] = prefab;
			return prefab;
		}
		return null; //??
	}

	public static WeaponBase SetPlayerWeapon(string name) {
		name = "wply_" + name;
		GameObject prefab = GetPrefab (name, "PlayerWeapon");
		if (instance.playerWeaponPlacement.Find ("PlayerWeapon") != null) {
			GameObject.Destroy (instance.playerWeaponPlacement.Find ("PlayerWeapon").gameObject);
		}
		GameObject weapon = GameObject.Instantiate (prefab, instance.playerWeaponPlacement);
		weapon.name = "PlayerWeapon";
		Transform hands = weapon.transform.Find ("Offset").Find ("Hands");
		SkinnedMeshRenderer renderer = hands.GetComponent<SkinnedMeshRenderer> ();
		renderer.sharedMesh = instance.hands;
		renderer.sharedMaterials = instance.handsMaterials;	
		WeaponBase weaponObj = weapon.GetComponent<WeaponBase> ();
		//weapon.GetComponent<Type> () = name;
		Firearm firearm = weaponObj as Firearm;
		if (firearm != null) {
			firearm.assetBundle = name;
		}
		return weaponObj;
	}

	public static Transform GetNPCWeaponPlacement(GameObject npc) {
		Transform origin = npc.transform;
		return origin
			.Find ("HumanRig")
			.Find ("root_ref.x")
			.Find ("spine_01_ref.x")
			.Find ("spine_02_ref.x")
			.Find ("shoulder_ref.r")
			.Find ("arm_ref.r")
			.Find ("forearm_ref.r");
	}

	public static WeaponBase SetNPCWeapon(GameObject npc, string name) {
		Transform weaponPlacement = GetNPCWeaponPlacement (npc);
		string originalName = name;
		name = "wnpc_" + name;
		GameObject prefab = GetPrefab (name, "NPCWeapon");
		if (weaponPlacement.Find ("NPCWeapon") != null) {
			GameObject.Destroy (weaponPlacement.Find ("NPCWeapon").gameObject);
		}
		GameObject weapon = GameObject.Instantiate (prefab, weaponPlacement);
		weapon.transform.localScale = new Vector3 (0.01f, 0.01f, 0.01f);
		weapon.transform.localRotation = Quaternion.Euler (90.0f, 0.0f, 0.0f);
		//weapon.transform.localPosition = new Vector3 (0.0f, 0.003253787f, 0.0f);
		weapon.transform.localPosition = new Vector3 (0.0f, 0.002657295f, 0.0f);
		weapon.name = "NPCWeapon";
		NPCSoldier npcObj = npc.GetComponent<NPCSoldier> ();
		npcObj.animationSystem.assetBundle = name;
		npcObj.animationSystem.Initialize ("Animation");
		NPCFirearm firearm = weapon.GetComponent<NPCFirearm> ();
		if (firearm != null) {
			firearm.assetBundle = name;
			npcObj.weapon = firearm;
		}
		return weapon.GetComponent<WeaponBase> ();
	}

	public static void SetPlayerNoWeapon() {
		Transform weapon = instance.playerWeaponPlacement.Find ("PlayerWeapon");
		if (weapon != null) {
			GameObject.Destroy (weapon.gameObject);
		}
	}



}
