using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.AI;

public enum NPCSoldierWalkState {
	FRONT,
	SIDE
}

public class NPCSoldier : MonoBehaviour {

	private Animator animator;
	private bool crouching = false;
	[HideInInspector] public AnimationSystem animationSystem;
	private NavMeshAgent agent;
	private NPCSoldierWalkState walkState = NPCSoldierWalkState.FRONT;
	private Vector2 walk = Vector2.zero;
	private Vector2 walkTarget = Vector2.zero;
	[HideInInspector] public Character character;
	private NPCSoldierLogic logic;
	private Transform raycastPoint;
	public string faction {
		get {
			return character.faction;
		}
	} 

	private NPCFirearm _weapon;
	public NPCFirearm weapon {
		get {
			return _weapon;
		}

		set {
			_weapon = value;
			value.npc = logic;			
		}
	}

	EventData OnHealthChanged(EventData data) {

		//Events.Character.HealthChanged healthChanged = data.Get<Events.Character.HealthChanged> (0);
		if (character.health <= 0) {
			agent.enabled = false;
			animator.enabled = false;
			enabled = false;
		}

		return new EventData ();
	}

	void Start() {
		animator = GetComponent<Animator> ();

		animationSystem = gameObject.AddComponent<AnimationSystem> ();

		agent = gameObject.AddComponent<NavMeshAgent> ();
		agent.height = 2.0f;
		agent.radius = 0.25f;
		agent.speed = 2.5f;
		agent.acceleration = 100000.0f;
		agent.angularSpeed = 360.0f * 2.0f;

		character = gameObject.AddComponent<Character> ();
		//character.health = 1;
		character.healthChanged.AddListener (OnHealthChanged);
		character.faction = "Default";

		raycastPoint = transform
			.Find ("HumanRig")
			.Find ("root_ref.x")
			.Find ("spine_01_ref.x")
			.Find ("spine_02_ref.x")
			.Find ("neck_ref.x")
			.Find ("head_ref.x");
		character.head = raycastPoint;

		logic = new NPCSoldierLogic (this);
	}

	public Character GetCharacterRaycast(Vector3 direction) {
		RaycastHit hit;
		direction = direction.normalized;
		if (Physics.Raycast (raycastPoint.position + (direction * 0.1f), direction, out hit)) {
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer ("Character")) {
				Character character = hit.collider.GetComponent<Character> ();
				if (character == null) {
					character = hit.collider.GetComponentInParent<Character> ();
				}
				return character;
			}
		}
		return null;
	}

	public List<Character> GetVisibleCharacters() {
		List<Character> result = new List<Character> ();
		foreach (Character character in CharacterManager.characters) {
			if (character == null) {
				continue;
			}
			Character hitCharacter = GetCharacterRaycast (character.head.position - raycastPoint.position);
			if (hitCharacter) {
				result.Add (hitCharacter);
			}
		}
		return result;
	}

	void Update() {
		Vector3 target = agent.destination;
		Vector3 direction = (target - transform.position).normalized;
		Vector3 forward = transform.forward;
		Vector3 right = transform.right;
		Vector3 left = -transform.right;
		float angle = Vector3.Angle (forward, direction);
		float rightAngle = Vector3.Angle (right, direction);
		float leftAngle = Vector3.Angle (left, direction);
		float speed = agent.desiredVelocity.magnitude;
		float maxSpeed = agent.speed;
		walk = Vector2.Lerp (walk, walkTarget, 12.0f * Time.deltaTime);
		if (walkState == NPCSoldierWalkState.FRONT) {
			if (speed > maxSpeed * 0.5f) {
				if (angle < 15.0f && angle > -15.0f) {
					walkTarget = new Vector2 (0.0f, 1.0f);
				} else {
					walkTarget = new Vector2 (0.0f, -1.0f);
				}
			} else {
				walkTarget = new Vector2 (0.0f, 0.0f);
			}
		} else if (walkState == NPCSoldierWalkState.SIDE) {
			if (speed > maxSpeed * 0.5f) {
				//Debug.Log (angle);
				if (rightAngle < 15.0f && rightAngle > -15.0f) {
					walkTarget = new Vector2 (1.0f, 0.0f);
				} else {
					walkTarget = new Vector2 (-1.0f, 0.0f);
				}
			} else {
				walkTarget = new Vector2 (0.0f, 0.0f);
			}
		}
		animator.SetFloat ("WalkX", walk.x);
		animator.SetFloat ("WalkY", walk.y);
		logic.Update ();
	}

	public void LookAt(Vector3 position) {
		Vector3 dir = position - raycastPoint.position;
		dir = dir.normalized;
		//transform.forward
		float forwardAngle = Vector3.Angle(transform.right, dir);
		float upAngle = Vector3.Angle(transform.up, dir);

		//upAngle -= 90.0f;
		//forwardAngle -= 90.0f;

		upAngle *= 0.01f;
		forwardAngle *= 0.01f;

		upAngle -= 0.85f;
		forwardAngle -= 0.85f;

		upAngle = -upAngle;
		forwardAngle = -forwardAngle;

		//Debug.Log (forwardAngle + " " + upAngle);

		animator.SetFloat ("LookX", forwardAngle);
		animator.SetFloat ("LookY", upAngle);
	}

	IEnumerator CrouchAnimation() {
		//animator.SetFloat ("Crouch", 0.0f);
		//animator.SetFloat ("IdleState", 0.5f);
		animator.SetTrigger("CrouchStart");
		float crouchStartLength = animationSystem.GetLength ("HumanRig|CrouchStart");
		yield return new WaitForSeconds (crouchStartLength);
		//animator.SetFloat ("Crouch", 0.5f);
	}

	IEnumerator UncrouchAnimation() {
		animator.SetTrigger("CrouchEnd");
		float crouchEndLength = animationSystem.GetLength ("HumanRig|CrouchEnd");
		//animator.SetFloat ("Crouch", 1.0f);
		yield return new WaitForSeconds (crouchEndLength);
		animator.SetFloat ("WalkX", 0.0f);
		animator.SetFloat ("WalkY", 0.0f);
		animator.SetFloat ("IdleState", 0.0f);
	}

	public void Crouch() {
		if (crouching) {
			return;
		}
		crouching = true;
		StartCoroutine (CrouchAnimation ());
	}

	public void Uncrouch() {
		if (!crouching) {
			return;
		}
		crouching = false;
		StartCoroutine (UncrouchAnimation ());
	}

	public bool IsCrouching() {
		return crouching;
	}

	IEnumerator MoveForSeconds(Vector3 move, float seconds) {
		while (seconds > 0.0f) {
			agent.SetDestination(transform.position + move);
			seconds -= Time.deltaTime;
			yield return new WaitForEndOfFrame ();
		}
	}

	public void MoveFront(Vector3 move, float seconds = 0.0f) {
		//agent.Move (move);
		agent.updateRotation = true;
		agent.SetDestination(transform.position + move);
		//agent.Move(move);
		walkState = NPCSoldierWalkState.FRONT;
		if (seconds != 0.0f) {
			StartCoroutine (MoveForSeconds (move, seconds));
		}
	}

	public void MoveSide(Vector3 move, float seconds = 0.0f) {
		agent.updateRotation = false;
		agent.SetDestination(transform.position + move);
		//agent.Move(move);
		walkState = NPCSoldierWalkState.SIDE;
		if (seconds != 0.0f) {
			StartCoroutine (MoveForSeconds (move, seconds));
		}
	}

	public void MoveBack(Vector3 move, float seconds = 0.0f) {
		agent.updateRotation = false;
		agent.SetDestination(transform.position + move);
		//agent.Move(move);
		walkState = NPCSoldierWalkState.FRONT;
		if (seconds != 0.0f) {
			StartCoroutine (MoveForSeconds (move, seconds));
		}
	}

	IEnumerator RotateLookInSeconds(Vector3 target, float seconds) {
		while (seconds > 0.0f) {
			//transform.LookAt (target, Vector3.up);
			Quaternion rotation = transform.rotation;
			Quaternion originalRotation = rotation;
			rotation = Quaternion.LookRotation ((target - transform.position).normalized);
			Vector3 euler = rotation.eulerAngles;
			euler.x = 0.0f;
			euler.z = 0.0f;
			rotation = Quaternion.Euler (euler);
			rotation = Quaternion.Lerp (originalRotation, rotation, 5.0f * Time.deltaTime);
			transform.rotation = rotation;
			seconds -= Time.deltaTime;
			yield return new WaitForEndOfFrame ();
		}
	}

	public void RotateLook(Vector3 target, float seconds = 0.0f) {
		//transform.LookAt (target, Vector3.up);
		Quaternion rotation = transform.rotation;
		Quaternion originalRotation = rotation;
		rotation = Quaternion.LookRotation ((target - transform.position).normalized);
		Vector3 euler = rotation.eulerAngles;
		euler.x = 0.0f;
		euler.z = 0.0f;
		rotation = Quaternion.Euler (euler);
		rotation = Quaternion.Lerp (originalRotation, rotation, 5.0f * Time.deltaTime);
		transform.rotation = rotation;
		if (seconds != 0.0f) {
			StartCoroutine (RotateLookInSeconds (target, seconds));
		}
	}

	public void SetWeapon(string assetBundle) {
		WeaponManager.SetNPCWeapon (gameObject, assetBundle);
	}

}
