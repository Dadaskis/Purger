using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour {

	private Transform start;
	private Transform end;
	private Transform onStart;
	private Transform onEnd;
	private BoxCollider boxCollider;

	private bool isPlayerClimb = false;
	private float procent = 0.0f;
	private float speedProcent = 0.0f;
	private float speed = 2.0f;
	private float closeToOriginValue = 5.0f;
	private float moveSpeed = 7.0f;
	private float stopDistance = 0.1f;

	void Start () {

		start = transform.Find ("Start");
		end = transform.Find ("End");
		onStart = transform.Find ("OnStart");
		onEnd = transform.Find ("OnEnd");
		boxCollider = GetComponent<BoxCollider> ();

	}

	void OnTriggerEnter(Collider collider) {
		if (collider.transform.root.gameObject.tag == "Player" 
				&& !isPlayerClimb 
				&& (Player.controller.state == PlayerControllerState.NORMAL
					|| Player.controller.state == PlayerControllerState.WATER)
			) {
			Player.controller.state = PlayerControllerState.LADDER;
			HUDElements.RemoveElement ("OxygenData");
			Player.controller.enableLogic = false;
			isPlayerClimb = true;

			if (Player.weapon) {
				Player.weapon.TakeOff ();
			}

			float startY = start.position.y;
			float endY = end.position.y;
			float playerY = Player.position.y;

			endY -= startY;
			playerY -= startY;

			speedProcent = speed / Vector3.Distance (start.position, end.position);

			float playerProcent = playerY / endY;
			playerProcent = Mathf.Min (playerProcent, 1.0f - (speedProcent / closeToOriginValue));
			playerProcent = Mathf.Max (playerProcent, (speedProcent / closeToOriginValue));

			procent = playerProcent;
			StartCoroutine (GoingToProcent (procent));
		}
	}

	void Update() {
		if (!isPlayerClimb) {
			return;
		}

		if (InputManager.GetButton ("PlayerForward")) {
			procent += speedProcent * Time.deltaTime;
		} else if (InputManager.GetButton ("PlayerBackward")) {
			procent -= speedProcent * Time.deltaTime;
		} else if (InputManager.GetButtonDown ("PlayerJump")) {
			Player.controller.enableLogic = true;
			Player.controller.state = PlayerControllerState.NORMAL;
			isPlayerClimb = false;
			if (Player.weapon) {
				Player.weapon.TakeUp ();
			}
		}

		procent = Mathf.Clamp01 (procent);

		Player.position = Vector3.Lerp (start.position, end.position, procent);

		if (procent == 0.0f) {
			StartCoroutine (GoingToStart ());		
		} else if (procent == 1.0f) {
			StartCoroutine (GoingToEnd ());
		}
	}

	IEnumerator GoingToProcent (float procent) {
		isPlayerClimb = false;
		Vector3 targetPosition = Vector3.Lerp (start.position, end.position, procent);
		while (true) {
			Player.position = Vector3.Lerp (Player.position, targetPosition, moveSpeed * Time.deltaTime);
			if(Vector3.Distance(Player.position, targetPosition) < stopDistance) {
				isPlayerClimb = true;
				break;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator GoingToStart() {
		Player.controller.characterController.enabled = false;
		isPlayerClimb = false;
		while (true) {
			Player.position = Vector3.Lerp (Player.position, onStart.position, moveSpeed * Time.deltaTime);
			if(Vector3.Distance(Player.position, onStart.position) < stopDistance) {
				Player.controller.characterController.enabled = true;
				Player.controller.characterController.SimpleMove (new Vector3 ());
				Player.controller.moveDir = new Vector3 ();
				Player.controller.enableLogic = true;
				Player.controller.state = PlayerControllerState.NORMAL;
				if (Player.weapon) {
					Player.weapon.TakeUp ();
				}
				break;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator GoingToEnd() {
		Player.controller.characterController.enabled = false;
		isPlayerClimb = false;
		while (true) {
			Player.position = Vector3.Lerp (Player.position, onEnd.position, moveSpeed * Time.deltaTime);
			if(Vector3.Distance(Player.position, onEnd.position) < stopDistance) {
				Player.controller.characterController.enabled = true;
				Player.controller.characterController.SimpleMove (new Vector3 ());
				Player.controller.moveDir = new Vector3 ();
				Player.controller.enableLogic = true;
				Player.controller.state = PlayerControllerState.NORMAL;
				if (Player.weapon) {
					Player.weapon.TakeUp ();
				}
				break;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	//private float gizmosTimer = 0.0f;

	void OnDrawGizmos() {
		if (start == null || end == null || onStart == null || onEnd == null || boxCollider == null) {
			boxCollider = GetComponent<BoxCollider> ();
			if (boxCollider == null) {
				boxCollider = gameObject.AddComponent<BoxCollider> ();
				Renderer renderer = GetComponent<Renderer> ();
				boxCollider.bounds.Encapsulate (renderer.bounds);
				boxCollider.isTrigger = true;
			}

			Bounds bounds = boxCollider.bounds;

			start = transform.Find ("Start");
			if (start == null) {
				start = new GameObject ("Start").transform;
				start.parent = transform;
				start.localPosition = Vector3.zero;
				Vector3 position = start.position;
				position.y = transform.position.y - bounds.extents.y + bounds.center.z;
				start.position = position;
			}

			end = transform.Find ("End");
			if (end == null) {
				end = new GameObject ("End").transform;
				end.parent = transform;
				end.localPosition = Vector3.zero;
				Vector3 position = end.position;
				position.y = transform.position.y + (bounds.extents.y + bounds.center.z);
				end.position = position;
			}

			onStart = transform.Find ("OnStart");
			if (onStart == null) {
				onStart = new GameObject ("OnStart").transform;
				onStart.parent = transform;
				onStart.localPosition = Vector3.zero;
				Vector3 position = onStart.position;
				position.y = transform.position.y - bounds.extents.y + bounds.center.z;
				position.x += 1.0f;
				onStart.position = position;
			}

			onEnd = transform.Find ("OnEnd");
			if (onEnd == null) {
				onEnd = new GameObject ("OnEnd").transform;
				onEnd.parent = transform;
				onEnd.localPosition = Vector3.zero;
				Vector3 position = onEnd.position;
				position.y = transform.position.y + (bounds.extents.y + bounds.center.z);
				position.x += 1.0f;
				onEnd.position = position;
			}

			return;
		}

		//gizmosTimer += (Time.deltaTime * 0.01f);

		Gizmos.color = Color.white;
		Gizmos.DrawLine(start.position, end.position);
		Gizmos.DrawLine(onEnd.position, end.position);
		Gizmos.DrawLine(start.position, onStart.position);

		Gizmos.color = Color.red;
		Vector3 startPosition = start.position;
		Vector3 endPosition = end.position;
		float offset = 1.0f / Vector3.Distance (startPosition, endPosition);
		float currentValue = offset;
		Vector3 currentPosition = Vector3.Lerp (startPosition, endPosition, currentValue);
		while(Vector3.Distance(currentPosition, endPosition) >= 0.1f) {
			Gizmos.DrawCube (currentPosition, new Vector3 (0.2f, 0.2f, 0.2f));
			//float value = currentValue + gizmosTimer;
			//if (value > 1.0f) {
			//	value -= Mathf.Round (value);
			//}
			currentPosition = Vector3.Lerp (startPosition, endPosition, currentValue);
			currentValue += offset;
		}

		float sphereRadius = 0.15f;

		Gizmos.color = new Color (1.0f, 0.0f, 0.0f);
		Gizmos.DrawSphere (start.position, sphereRadius);

		Gizmos.color = new Color (1.0f, 0.3f, 0.3f);
		Gizmos.DrawSphere (onStart.position, sphereRadius);

		Gizmos.color = new Color (0.0f, 1.0f, 0.0f);
		Gizmos.DrawSphere (end.position, sphereRadius);

		Gizmos.color = new Color (0.3f, 1.0f, 0.3f);
		Gizmos.DrawSphere (onEnd.position, sphereRadius);
	}

}
