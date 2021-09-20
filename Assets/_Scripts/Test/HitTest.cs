using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTest : DamagableObject {

	private Renderer renderer;
	private Collider collider;

	public override void Start () {
		base.Start ();
		renderer = GetComponent<Renderer> ();
		collider = GetComponent<Collider> ();
	}

	public override void OnDamage (int damage, Vector3 direction = default(Vector3)) {
		renderer.enabled = false;
		base.damagable = false;
		collider.enabled = false;
	}

	void Update() {
		if(Input.GetKeyDown(KeyCode.Alpha9)) {
			renderer.enabled = true;
			base.damagable = true;
			collider.enabled = true;
		}
	}
}
