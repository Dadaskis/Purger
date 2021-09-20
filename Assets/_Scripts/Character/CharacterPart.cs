using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPart : DamagableObject {

	private Character character;
	public float multiplier = 1.0f;

	public override void Start() {
		base.Start ();
		character = GetComponentInParent<Character> ();
	}

	public override void OnDamage (int damage, Vector3 direction = default(Vector3)) {
		float dmg = ((float)damage) * multiplier;
		character.Damage (Mathf.RoundToInt(dmg), false);
		if (character.health <= 0) { 
			Rigidbody[] bodies = character.GetComponentsInChildren<Rigidbody> ();
			foreach (Rigidbody body in bodies) {
				body.isKinematic = false;
				body.AddForce (direction * 3.0f, ForceMode.Impulse);
				body.AddExplosionForce(50.0f, character.transform.position - direction, 15.0f);
			}
			RemoveDamagable (this);
		}
	}
}
