using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {

	[HideInInspector] public BoxCollider boxCollider;

	public const float OFFSET = 0.9f;

	private float magnitudeMin = 1.0f;
	private float startSpeedMultiplier = 0.5f;
	private float pushUpPower = 5.0f;

	void Start() {
		boxCollider = gameObject.AddComponent<BoxCollider> ();
		boxCollider.isTrigger = true;
		boxCollider.bounds.Encapsulate (GetComponent<Renderer> ().bounds);
	}

	void OnTriggerStay(Collider collider) {
		if (collider.transform.root.gameObject.tag == "Player" && Player.controller.state == PlayerControllerState.NORMAL) {
			Bounds bounds = collider.bounds;
			if (Player.position.y < yLimit) {
				Player.controller.state = PlayerControllerState.WATER;
				Player.waterController.water = this;
				HUDElements.AddElement ("OxygenData");
				Player.controller.moveDir = new Vector3 ();
				Player.controller.jump = false;
				Player.controller.inAir = false;
				Player.controller.inAirJumped = false;
				Player.controller.inAirTimer = 0.0f;
				if (Player.weapon && Player.controller.previousState != PlayerControllerState.LADDER) {
					Player.weapon.TakeOff ();
				}
				Vector3 velocity = Player.controller.characterController.velocity;
				if (velocity.magnitude > magnitudeMin) {
					Player.waterController.velocity = velocity * startSpeedMultiplier;
				}
			} 
		} else {
			Rigidbody body = collider.attachedRigidbody;
			if (body != null) {
				body.AddForce (new Vector3 (0.0f, pushUpPower, 0.0f));
			}
		}
	}

	private float calculateYLimit() {
		Bounds bounds = boxCollider.bounds;
		return (transform.position.y + bounds.extents.y) - OFFSET;
	}

	public float yLimit {
		get {
			return calculateYLimit();
		}
	}

	void OnDrawGizmos() {
		#if false
		if (collider == null) {
			collider = GetComponent<BoxCollider> ();
			return;
		}
		Bounds bounds = collider.bounds;
		Gizmos.color = Color.white;
		Vector3 cubeSize = bounds.extents * 2.0f;
		cubeSize.y = 0.01f;
		Vector3 cubePosition = transform.position;
		cubePosition.y = yLimit;
		Gizmos.DrawCube (cubePosition, cubeSize);
		#endif
	}

}
