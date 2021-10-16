using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NPCFirearm : MonoBehaviour {

	public int burstAmmo = 3;
	public float burstTempo = 0.2f;
	public float burstDelay = 1.5f;
	public float burstSpread = 0.3f;
	public int minDamage = 1;
	public int maxDamage = 5;

	[HideInInspector] public NPCSoldierLogic npc;
	[HideInInspector] public string assetBundle;

	private Transform muzzlePos;
	private Transform ammoDropPos;
	private GameObject muzzlePrefab;
	private GameObject ammoDropPrefab;

	private static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle> ();

	private float timer = 0.0f;

	void Start() {
		muzzlePos = transform.Find ("MuzzlePos");
		ammoDropPos = transform.Find ("AmmoDropPos");

		AssetBundle bundle;
		if (!assetBundles.TryGetValue (assetBundle, out bundle)) {
			bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/" + assetBundle));
			assetBundles [assetBundle] = bundle;
		}

		if (muzzlePos != null) {
			muzzlePrefab = bundle.LoadAsset<GameObject> ("MuzzleFlash");
		}

		if (ammoDropPos != null) {
			ammoDropPrefab = bundle.LoadAsset<GameObject> ("AmmoDrop");
		}

		//bundle.Unload (false);
	}

	IEnumerator Shoot() {
		timer = -burstDelay;

		for (int ammo = 0; ammo < burstAmmo; ammo++) {
			GameObject muzzleFlash = Instantiate (muzzlePrefab, muzzlePos);
			muzzleFlash.transform.localPosition = Vector3.zero;

			GameObject ammoDrop = Instantiate (ammoDropPrefab, ammoDropPos);
			ammoDrop.transform.localPosition = Vector3.zero;
			ammoDrop.transform.SetParent (null, true);
			BoxCollider collider = ammoDrop.AddComponent<BoxCollider> ();
			collider.bounds.Encapsulate (ammoDrop.GetComponent<MeshRenderer> ().bounds);
			Rigidbody body = ammoDrop.AddComponent<Rigidbody> ();
			body.mass = 0.3f;
			body.AddExplosionForce (15.0f, ammoDropPos.position, 3.0f);

			int damage = Random.Range (minDamage, maxDamage);
			if (Vector3.Distance (transform.position, npc.enemy.transform.position) > 3.0f) {
				npc.enemy.Damage (damage, false);
			} else {
				npc.enemy.Damage (damage * 10, false);
			}
			yield return new WaitForSeconds (burstTempo);
		}
	}

	void Update() {
		if (npc.npc.character.health <= 0) {
			return;
		}
		if (timer > 0.0f) {
			Character enemy = npc.enemy;
			if (enemy != null) {
				Character check = npc.npc.GetCharacterRaycast (enemy.head.position - npc.npc.character.head.position);
				if (check != null) {
					StartCoroutine (Shoot ());
				}
			}
		}
		timer += Time.deltaTime;
	}

	void OnDrawGizmos() {
		if (muzzlePos == null || ammoDropPos == null) {
			muzzlePos = transform.Find ("MuzzlePos");
			ammoDropPos = transform.Find ("AmmoDropPos");
		}

		Gizmos.color = Color.red;
		Gizmos.DrawSphere (muzzlePos.position, 0.01f);
		Gizmos.DrawRay (muzzlePos.position, muzzlePos.forward);

		Gizmos.color = Color.green;
		Gizmos.DrawSphere (ammoDropPos.position, 0.01f);
		Gizmos.DrawRay (ammoDropPos.position, ammoDropPos.forward);
	}

}
