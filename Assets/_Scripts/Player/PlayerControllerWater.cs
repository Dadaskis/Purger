using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerWater  {

	public FirstPersonHeadController mouseLook;
	public CharacterController controller;
	public Transform head;

	private bool enabled = true;
	private float speed = 5.0f;
	private float climbingSpeed = 5.0f;
	private float climbingStopDistance = 0.1f;
	private float targetHeight = 0.9f;

	private Water _water;
	public Water water {
		set {
			_water = value;
		}

		get {
			return _water;
		}
	}

	private IEnumerator Climbing() {
		Vector3 start = head.position + (Player.forward / 1.3f);
		Vector3 end = head.position + new Vector3 (0.0f, -0.5f, 0.0f) + (Player.forward / 1.3f);
		RaycastHit hit;
		if (Physics.Linecast (start, end, out hit) && !Physics.Linecast (head.position + (Player.forward * 0.1f), start)) {
			if (hit.transform.GetComponent<Water> () == null && hit.normal.y > 0.5f) {
				controller.enabled = false;
				enabled = false;
				Vector3 target = hit.point + new Vector3 (0.0f, -0.5f, 0.0f);
				while (true) {
					Player.position = Vector3.Lerp (Player.position, target, climbingSpeed * Time.deltaTime);
					if (Vector3.Distance (Player.position, target) < climbingStopDistance) {
						controller.enabled = true;
						enabled = true;
						Player.StopController ();
						Player.controller.state = PlayerControllerState.NORMAL;
						HUDElements.RemoveElement ("OxygenData");
						if (Player.weapon) {
							Player.weapon.TakeUp ();
						}
						break;
					}
					yield return new WaitForEndOfFrame ();
				}
			}
		}
	}

	private IEnumerator OxygenChecker() {
		float value = 0.05f;
		int healthValue = 5;
		while(true) {
			yield return new WaitForSeconds (2.0f);
			if (Player.controller.state == PlayerControllerState.WATER) {
				if (Player.position.y < water.yLimit - 0.35f) {
					Player.oxygen -= value;
					if (Player.oxygen <= 0.0f) {
						Player.health -= healthValue;
					}
				} else {
					Player.oxygen += value * 3.0f;
				}
			} else {
				Player.oxygen += value * 3.0f;
			}
		}
	}

	public Vector3 velocity = Vector3.zero;

	public PlayerControllerWater() {
		Player.instance.StartCoroutine (OxygenChecker ());
	}

	public void Update() {
		mouseLook.Update ();

		controller.height = Mathf.Lerp (controller.height, targetHeight, 8.0f * Time.deltaTime);

		head.transform.position = Vector3.Lerp (
			head.transform.position, 
			new Vector3 (
				head.transform.position.x, 
				controller.transform.position.y + targetHeight / 2.0f - 0.1f,
				head.transform.position.z
			),
			8.0f * Time.deltaTime
		);

		if (!enabled) {
			return;
		}

		velocity = Vector3.Lerp (velocity, new Vector3(0.0f, -1.0f, 0.0f), 0.5f * Time.deltaTime);

		if (Player.position.y > water.yLimit + 0.1f) {
			Player.controller.state = PlayerControllerState.NORMAL;
			HUDElements.RemoveElement ("OxygenData");
			if (Player.weapon) {
				Player.weapon.TakeUp ();
			}
			return;
		}

		Vector3 moveDir = new Vector3 ();

		if (InputManager.GetButton ("PlayerForward")) {
			moveDir.z += 1.0f;
		} 

		if (InputManager.GetButton ("PlayerBackward")) {
			moveDir.z -= 1.0f;
		}

		if (InputManager.GetButton ("PlayerLeft")) {
			moveDir.x -= 1.0f;
		}

		if (InputManager.GetButton ("PlayerRight")) {
			moveDir.x += 1.0f;
		}

		if (Player.position.y < water.yLimit - 0.05f) {
			if (InputManager.GetButton ("PlayerJump")) {
				moveDir.y += 1.0f;
			}
		}

		if (InputManager.GetButton ("PlayerCrouch")) {
			moveDir.y -= 1.0f;
		}

		Vector3 forward = mouseLook.head.forward;
		Vector3 right = mouseLook.head.right;
		Vector3 up = mouseLook.head.up;

		if (moveDir.z == 1.0f) { 
			if (Player.position.y > water.yLimit - 0.05f && forward.y > 0.0f) {
				forward.y = 0.0f;
				forward = Vector3.Normalize (forward);
				if (InputManager.GetButton("PlayerJump")) {
					Player.instance.StartCoroutine (Climbing ());
				}
			}
		} else if(moveDir.z == -1.0f) {
			if (Player.position.y > water.yLimit - 0.05f && forward.y < 0.0f) {
				forward.y = 0.0f;
				forward = Vector3.Normalize (forward);
			}
		}

		moveDir *= speed;

		Vector3 targetVelocity = (forward * moveDir.z) + (right * moveDir.x) + (up * moveDir.y);
		if (targetVelocity.y > 0.0f && Player.position.y > water.yLimit - 0.05f) {
			targetVelocity.y = 0.0f;
		}

		velocity = Vector3.Lerp (velocity, targetVelocity, 2.0f * Time.deltaTime);

		if (velocity.y > 0.0f && Player.position.y > water.yLimit - 0.05f) {
			velocity.y = 0.0f;
		}

		controller.Move (velocity * Time.deltaTime);
	}

	public void OnDrawGizmos() {
		/*Gizmos.color = Color.white;
		Gizmos.DrawCube (Player.position + new Vector3 (0.0f, 2.0f, 0.0f), new Vector3(0.2f, 0.2f, 0.2f));
		Gizmos.DrawCube (Player.position + new Vector3 (0.0f, 2.0f, 0.0f) + (Player.forward / 2.0f), new Vector3(0.2f, 0.2f, 0.2f));
		Gizmos.DrawCube (Player.position + new Vector3 (0.0f, 1.2f, 0.0f) + (Player.forward / 2.0f), new Vector3(0.2f, 0.2f, 0.2f));
		Gizmos.DrawLine (Player.position + new Vector3 (0.0f, 2.0f, 0.0f), Player.position + new Vector3 (0.0f, 2.0f, 0.0f) + (Player.forward / 2.0f));
		Gizmos.DrawLine (Player.position + new Vector3 (0.0f, 2.0f, 0.0f) + (Player.forward / 2.0f), Player.position + new Vector3 (0.0f, 1.2f, 0.0f) + (Player.forward / 2.0f));*/
	}

}
