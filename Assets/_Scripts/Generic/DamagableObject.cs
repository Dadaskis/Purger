using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagableObject : MonoBehaviour {

	public static List<DamagableObject> damagables = new List<DamagableObject> ();

	[HideInInspector] public bool damagable = true;
	public int priority = 0;

	public static void AddDamagable(DamagableObject obj) {
		damagables.Add (obj);
	}

	public static void RemoveDamagable(DamagableObject obj) {
		damagables.Remove (obj);
	}

	public virtual void Start() {
		AddDamagable (this);
		//Debug.Log (this);
	}

	public virtual void OnDamage(int damage, Vector3 damagable = default(Vector3)) { }

}
