using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerControllerState {
	NORMAL,
	LADDER,
	WATER
}

namespace Events.PlayerController {
	class Grounded {}
}

public class PlayerController {

	private PlayerControllerState _state = PlayerControllerState.NORMAL;
	private PlayerControllerState _previousState = PlayerControllerState.NORMAL;
	public PlayerControllerState state {
		set {
			_previousState = _state;
			_state = value;
		}

		get {
			return _state;
		}
	}

	public PlayerControllerState previousState {
		get {
			return _previousState;
		}
	}

	public bool isWalking = true; 
	public float walkSpeed = 3.0f;
	public float runSpeed = 7.0f;
	public float jumpSpeed = 8.0f;
	public float jumpForwardSpeed = 18.0f;
	public float stickToGroundForce = 3.0f;
	public float gravityMultiplier = 2.0f;
	public FirstPersonHeadController mouseLook;
	private Transform m_Camera = null;

	public bool jump;
	private float m_YRotation;
	private Vector2 inputDirection;
	public Vector3 moveDir = Vector3.zero;
	public CharacterController characterController;
	private CollisionFlags m_CollisionFlags;
	private bool m_PreviouslyGrounded = false;
	private Vector3 m_OriginalCameraPosition = new Vector3();
	private float stepDistance = 1.0f;
	private float m_NextStep = 0.0f;
	public bool inAir = false;
	public bool inAirJumped = false;

	public bool enableMouseLook = true;
	public float targetHeight = 1.6f;
	public float crouchHeight = 0.9f;
	public float crouchSpeed = 2.0f;
	public bool holdCrouch = false;
	public float inAirMoveMultiplier = 0.3f;
	public bool inertionInAir = true;
	public bool enableLogic = true;

	private bool isCrouching = false;
	private bool isEnteredCrouching = false;
	private float previousY = 0.0f;
	private float startHeight = 0.0f;

	public bool isRunning = false;

	//public float stepSpeedMultiplier = 1.0f;
	//public float stepDistanceToPlaySound = 1.0f;
	//public float stepDistanceToPlaySoundOnRun = 2.0f;
	//public float stepVolume = 1.0f;
	public float stepSoundDelayOnWalk = 1.0f;
	public float stepSoundDelayOnCrouch = 1.0f;
	public float stepSoundDelayOnRun = 1.0f;
	private float stepSoundTimer = 0.0f;

	public float physicsPushPower = 1.0f;

	public float crouchDetectHeightMultiplier = 1.0f;
	public float crouchDetectRadiusMultiplier = 1.0f;
	private Transform transform;

	public const float FOV_SPEED = 3.0f;
	public FOVEffect cameraEffect;
	private float walkFOV = 3.0f;
	private float runFOV = 8.0f;

	private float climbingSpeed = 5.0f;
	private float climbingStopDistance = 0.1f;

	public float inAirTimer = 0.0f;

	public void Start(CharacterController controller, Transform player)
	{	
		m_Camera = player.Find ("CrouchHelper");
		mouseLook = new FirstPersonHeadController ();
		mouseLook.head = m_Camera.Find ("Head");
		mouseLook.player = player;
		characterController = controller;
		startHeight = characterController.height;
		stepDistance = 0f;
		m_NextStep = stepDistance / 2f;
		inAir = false;
		transform = player;
		cameraEffect = FOVEffects.RegisterEffect ();
	}

	private IEnumerator Climbing() {
		Transform head = mouseLook.head;
		CharacterController controller = characterController;
		Vector3 start = head.position + (Player.forward / 1.3f);
		Vector3 end = head.position + new Vector3 (0.0f, -1.3f, 0.0f) + (Player.forward / 1.3f);
		RaycastHit hit;
		if (Physics.Linecast (start, end, out hit) 
				&& !Physics.Linecast (head.position + (Player.forward * 0.1f), start) 
				&& !Physics.Linecast (hit.point, hit.point + new Vector3 (0.0f, 2.0f, 0.0f))) {
			if (hit.transform.GetComponent<Water> () == null && hit.normal.y > 0.5f) {
				controller.enabled = false;
				enableLogic = false;
				if (Player.weapon) {
					Player.weapon.Climbing ();
				}
				Vector3 target = hit.point + new Vector3 (0.0f, 0.0f, 0.0f);
				while (true) {
					Player.position = Vector3.Lerp (Player.position, target, climbingSpeed * Time.deltaTime);
					if (Vector3.Distance (Player.position, target) < climbingStopDistance) {
						controller.enabled = true;
						enableLogic = true;
						Player.controller.state = PlayerControllerState.NORMAL;
						break;
					}
					yield return new WaitForEndOfFrame ();
				}
			}
		}
	}
	
	public void Update()
	{
		RotateView ();

		if (!enableLogic) {
			moveDir = new Vector3 ();
			moveDirection = new Vector2 ();
			jump = false;
			inAir = false;
			inAirJumped = false;
			inAirTimer = 0.0f;
			return;
		}

		jump = InputManager.GetButtonDown("PlayerJump");
		if (jump) {
			Player.instance.StartCoroutine (Climbing ());
		}

		if (jump && !characterController.isGrounded) {
			jump = false;
		}

		if (jump) {
			mouseLook.jumping = true;
		}

		previousY = characterController.transform.position.y - characterController.height / 2 - characterController.skinWidth;

		if (holdCrouch) {
			if (InputManager.GetButtonDown ("PlayerCrouch") == true) {
				if (isCrouching == false) {
					isCrouching = true;
					targetHeight = crouchHeight;
				} else {
					float radius = characterController.radius * crouchDetectRadiusMultiplier;
					float height = characterController.height * crouchDetectHeightMultiplier;
					bool isHitToOpaque = Physics.CheckBox (
						m_Camera.position + new Vector3 (0.0f, height + radius, 0.0f), 
						new Vector3 (radius, radius * 2.0f, radius)
					);
					if (!isHitToOpaque) {
						targetHeight = startHeight;
					} else {
						isCrouching = true;
					}
				}
			}
		} else {
			bool isPressedCrouching = InputManager.GetButton("PlayerCrouch");

			if (isPressedCrouching) {
				isEnteredCrouching = true;
			}

			if (isCrouching && !isPressedCrouching) {
				float radius = characterController.radius * crouchDetectRadiusMultiplier;
				float height = characterController.height * crouchDetectHeightMultiplier;
				bool isHitToOpaque = Physics.CheckBox (
					m_Camera.position + new Vector3 (0.0f, height + radius, 0.0f), 
					new Vector3 (radius, radius * 2.0f, radius)
				);
				if (!isHitToOpaque) {
					targetHeight = startHeight;
					isCrouching = false;
				} else {
					isCrouching = true;
				}
			} else if(isPressedCrouching) {
				targetHeight = crouchHeight;
				isCrouching = true;
			} 
		}

		characterController.height = Mathf.Lerp (characterController.height, targetHeight, 5.0f * Time.deltaTime);

		m_Camera.transform.position = Vector3.Lerp (
			m_Camera.transform.position, 
			new Vector3 (
				m_Camera.transform.position.x, 
				characterController.transform.position.y + targetHeight / 2.0f - 0.1f,
				m_Camera.transform.position.z
			),
			5.0f * Time.deltaTime
		);

		characterController.transform.position = Vector3.Lerp (
			characterController.transform.position,
			new Vector3 (
				characterController.transform.position.x,
				previousY + targetHeight / 2.0f + characterController.skinWidth,
				characterController.transform.position.z
			),
			5.0f * Time.deltaTime
		);

		stepSoundTimer += Time.deltaTime;
	}

	private void PlayLandingSound()
	{
		PlayFootStepAudio (true);
	}

	private float previousSpeed = 0.0f;

	private Vector2 moveDirection = new Vector2();

	public void FixedUpdate()
	{
		if (!enableLogic) {
			return;
		}

		//
		
		bool isGrounded = characterController.isGrounded;

		float speed;
		GetInput(out speed);
		ProgressStepCycle(speed);

		// always move along the camera forward as it is the direction that it being aimed at
		Vector3 desiredMove = transform.forward * inputDirection.y + transform.right * inputDirection.x;

		// get a normal for the surface that is being touched to move along it
		RaycastHit hitInfo;
		Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
			characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
		desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
		
		if (hitInfo.normal.y < 0.5f) {
			isGrounded = false;
			desiredMove = hitInfo.normal;
		}

		if (inAir && isGrounded)
		{
			EventManager.RunEventListeners<Events.PlayerController.Grounded> ();
			PlayLandingSound();
			moveDir.y = 0f;
			inAir = false;
			jump = false;
			inAirJumped = false;
			inAirTimer = 0.0f;
		}

		/*if (!isGrounded && !jumping && m_PreviouslyGrounded)
		{
			moveDir.y = 0f;
		}*/

		moveDir.x = desiredMove.x * speed;
		//moveDir.y = desiredMove.y * speed;
		moveDir.z = desiredMove.z * speed;

		if (isGrounded && inertionInAir)
		{
			moveDir.y = -stickToGroundForce;

			if (jump)
			{
				moveDir.y += jumpSpeed;
				if (isWalking || isRunning) {
					moveDir.x = desiredMove.x * jumpForwardSpeed;
					moveDir.z = desiredMove.z * jumpForwardSpeed;
				}
				PlayJumpSound();
				jump = false;
				inAir = true;
				inAirJumped = true;
			}
		}
		else
		{
			moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
			moveDir.x = characterController.velocity.x + (desiredMove.x * inAirMoveMultiplier);
			moveDir.z = characterController.velocity.z + (desiredMove.z * inAirMoveMultiplier);
			if (isCrouching) {
				moveDir.x -= (desiredMove.x * inAirMoveMultiplier) / 3.0f;
				moveDir.z -= (desiredMove.z * inAirMoveMultiplier) / 3.0f;
			}
			cameraEffect.addFOV = Mathf.Lerp (cameraEffect.addFOV, moveDir.magnitude * 0.8f, FOV_SPEED * Time.fixedDeltaTime);
			inAir = true;
			inAirTimer += Time.fixedDeltaTime;
		}

		if (moveDir.x + moveDir.z == 0.0f) {
			moveDirection = Vector2.Lerp (moveDirection, Vector2.zero, Time.fixedDeltaTime * 6.0f);
		} else {
			moveDirection = Vector2.Lerp (moveDirection, new Vector2 (moveDir.x, moveDir.z), Time.fixedDeltaTime * 10.0f);
		}
		m_CollisionFlags = characterController.Move (new Vector3 (moveDirection.x, moveDir.y, moveDirection.y) * Time.fixedDeltaTime);

		m_PreviouslyGrounded = characterController.isGrounded;
		previousSpeed = Mathf.Abs(characterController.velocity.y);
	}


	private void PlayJumpSound()
	{
		//m_AudioSource.clip = m_JumpSound;
		//m_AudioSource.Play();
		PlayFootStepAudio(true);
	}


	private void ProgressStepCycle(float speed)
	{
		float delay = 0.0f;

		if (Mathf.Abs(speed) < crouchSpeed) { 
			return;
		}

		if (isCrouching) {
			delay = stepSoundDelayOnCrouch;
		} else if (isWalking) {
			delay = stepSoundDelayOnWalk;
		} else {
			delay = stepSoundDelayOnRun;
		}

		if (stepSoundTimer > delay) {
			PlayFootStepAudio ();
			stepSoundTimer = 0.0f;
		}
	}


	private void PlayFootStepAudio(bool landing = false)
	{
		if (!characterController.isGrounded)
		{
			return;
		}

		//if(m_FootstepSounds.GetLength(0) > 0) {
		// pick & play a random footstep sound from the array,
		// excluding sound at index 0
		//    int n = Random.Range(1, m_FootstepSounds.Length);
		//     m_AudioSource.clip = m_FootstepSounds[n];
		//     m_AudioSource.PlayOneShot(m_AudioSource.clip);
		// move picked sound to index 0 so it's not picked next time
		//    m_FootstepSounds[n] = m_FootstepSounds[0];
		//    m_FootstepSounds[0] = m_AudioSource.clip;
		//}

		RaycastHit[] hits = Physics.RaycastAll (transform.position + new Vector3(0.0f, 0.5f, 0.0f), Vector3.down, 2.4f);
		RaycastHit neededHit = new RaycastHit ();
		bool neededHitFound = false;
		foreach (RaycastHit hit in hits) {
			if (hit.transform.root.tag != "Player") {
				if (hit.transform.GetComponent<Renderer> () == null) {
					continue;
				}
				if (hit.triangleIndex == -1) {
					continue;
				}
				neededHit = hit;
				neededHitFound = true;
				break;
			}
		}
		if (neededHitFound) {
			Material material = MaterialManager.GetMaterialFromRaycast (neededHit);
			if (material == null) {
				return;
			} 
			SoundMaterialType type = SoundManager.GetSoundMaterialType (material);
			if (type != null) {
				List<AudioClip> clips;
				if (landing) {
					clips = type.landingClipNames;
				} else if (isRunning) {
					clips = type.runClipNames;
				} else {
					clips = type.walkClipNames;
				}
				if (clips != null && clips.Count > 0) {
					AudioClip clip = clips [Random.Range (0, clips.Count)];
					if (clip != null) {
						SoundObjectData data = new SoundObjectData ();
						data.clip = clip;
						data.volume = 1.0f;
						data.spatialBlend = 0.0f;
						SoundManager.CreateSound (data, Vector3.zero, transform);
					}
				}
			}
		}
	}

	private void GetInput(out float speed)
	{
		if (!characterController.isGrounded) {
			speed = 0.0f;
			return;
		}

		// Read input
		float horizontal = 0.0f; 

		bool walking = false;
		FirstPersonHeadControllerSideWalk sideWalk = FirstPersonHeadControllerSideWalk.NONE;

		if (InputManager.GetButton ("PlayerLeft")) {
			horizontal -= 1.0f;
			walking = true;
			sideWalk = FirstPersonHeadControllerSideWalk.LEFT;
		}

		if (InputManager.GetButton ("PlayerRight")) {
			horizontal += 1.0f;
			walking = true;
			sideWalk = FirstPersonHeadControllerSideWalk.RIGHT;
		}

		float vertical = 0.0f;

		if (InputManager.GetButton ("PlayerBackward")) {
			vertical -= 1.0f;
			walking = true;
		}

		if (InputManager.GetButton ("PlayerForward")) {
			vertical += 1.0f;
			walking = true;
		}

		mouseLook.walking = walking;
		mouseLook.sideWalk = sideWalk;

		//bool waswalking = isWalking;

		isWalking = !InputManager.GetButton ("PlayerRun");

		isRunning = !isWalking;

		if (walking && isWalking) {
			cameraEffect.addFOV = Mathf.Lerp (cameraEffect.addFOV, walkFOV, FOV_SPEED * Time.fixedDeltaTime);
		} else if (walking && isRunning) {
			cameraEffect.addFOV = Mathf.Lerp (cameraEffect.addFOV, runFOV, FOV_SPEED * Time.fixedDeltaTime);
		} else if (!walking) {
			cameraEffect.addFOV = Mathf.Lerp (cameraEffect.addFOV, 0.0f, FOV_SPEED * Time.fixedDeltaTime);
		}

		if (horizontal == 0.0f && vertical == 0.0f) {
			speed = 0.0f;
		} else {
			speed = isWalking ? walkSpeed : runSpeed;
			speed = isCrouching ? crouchSpeed : speed;
		}
		inputDirection = new Vector2(horizontal, vertical);

		// normalize input if it exceeds 1 in combined length:
		if (inputDirection.sqrMagnitude > 1) {
			inputDirection.Normalize();
		}
	}


	private void RotateView()
	{
		//mouseLook.LookRotation (transform, m_Camera.transform);
		mouseLook.Update();
	}


	/*private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (!enableLogic) {
			return;
		}
		Rigidbody body = hit.collider.attachedRigidbody;
		//dont move the rigidbody if the character is on top of it
		if (m_CollisionFlags == CollisionFlags.Below)
		{
			return;
		}

		if (body == null || body.isKinematic)
		{
			return;
		}

		body.AddForceAtPosition (characterController.velocity * physicsPushPower, hit.point, ForceMode.Impulse);
	}*/

}
