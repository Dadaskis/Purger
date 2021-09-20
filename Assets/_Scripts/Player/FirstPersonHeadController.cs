using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FirstPersonHeadControllerSideWalk {
	NONE,
	LEFT,
	RIGHT
}

public class FirstPersonHeadController {

	private Transform _head;
	public Transform head {
		set {
			_head = value;
			headRotation = head.localRotation;
			walkEffects = head.Find ("WalkEffects");
		}

		get {
			return _head;
		}
	}

	private Transform _player;
	public Transform player {
		set {
			_player = value;
			playerRotation = player.localRotation;
		}

		get {
			return _player;
		}
	}

	private Transform walkEffects;

	private float _mouseSensitivity = 5.0f;
	public float mouseSensitivity {
		set {
			_mouseSensitivity = value;
		}

		get {
			return _mouseSensitivity;
		}
	}

	private bool _walking = false;
	public bool walking {
		set {
			_walking = value;
		}

		get {
			return _walking;
		}
	}

	private FirstPersonHeadControllerSideWalk _sideWalk = FirstPersonHeadControllerSideWalk.NONE; 
	public FirstPersonHeadControllerSideWalk sideWalk {
		set {
			_sideWalk = value;
		} 

		get {
			return _sideWalk;
		}
	}

	private bool _jumping = false;
	private bool _jumpAnimation = false;
	public bool jumping {
		set {
			_jumping = value;
		}

		get {
			return _jumping;
		}
	} 

	private Quaternion playerRotation;
	private Quaternion headRotation;

	private float minimumHeadX = -89F;
	private float maximumHeadX = 89F;

	Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

		angleX = Mathf.Clamp (angleX, minimumHeadX, maximumHeadX);

		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

		return q;
	}

	private float transitionSpeed = 4.0f;
	private Vector3 onWalkPosition = new Vector3(0.0f, -0.05f, 0.0f);
	private Vector3 onJumpPosition = new Vector3(0.0f, -0.1f, 0.0f);
	private Quaternion sideRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 2.0f));

	private void Walking() {
		if (_jumpAnimation) {
			return;
		}
		walkEffects.localPosition = Vector3.Lerp (walkEffects.localPosition, onWalkPosition, transitionSpeed * Time.fixedDeltaTime);
	}

	private void Staying() {
		if (_jumpAnimation) {
			return;
		}
		walkEffects.localPosition = Vector3.Lerp (walkEffects.localPosition, Vector3.zero, transitionSpeed * Time.fixedDeltaTime);
	}

	private IEnumerator JumpAnimation() {
		_jumpAnimation = true;
		while (Vector3.Distance(walkEffects.localPosition, onJumpPosition) > 0.03f) {
			walkEffects.localPosition = Vector3.Lerp (walkEffects.localPosition, onJumpPosition, transitionSpeed * Time.fixedDeltaTime);
			yield return new WaitForEndOfFrame ();
		}
		_jumpAnimation = false;
	}

	private void Jumping() {
		Player.instance.StartCoroutine (JumpAnimation ());
	}

	private void SideWalkNone() {
		walkEffects.localRotation = Quaternion.Slerp (walkEffects.localRotation, Quaternion.identity, transitionSpeed * Time.fixedDeltaTime);
	}

	private void SideWalkLeft() {
		walkEffects.localRotation = Quaternion.Slerp (walkEffects.localRotation, sideRotation, transitionSpeed * Time.fixedDeltaTime);
	}

	private void SideWalkRight() {
		walkEffects.localRotation = Quaternion.Slerp (walkEffects.localRotation, Quaternion.Inverse(sideRotation), transitionSpeed * Time.fixedDeltaTime);
	}

	public void Update() {
		float xRot = Input.GetAxis ("Horizontal") * mouseSensitivity * Time.fixedDeltaTime;
		float yRot = Input.GetAxis ("Vertical") * mouseSensitivity * Time.fixedDeltaTime;

		playerRotation *= Quaternion.Euler (0f, yRot, 0f);
		headRotation *= Quaternion.Euler (-xRot, 0f, 0f);
		headRotation = ClampRotationAroundXAxis (headRotation);

		//player.localRotation = playerRotation;
		//head.localRotation = headRotation;

		player.localRotation = Quaternion.Slerp (player.localRotation, playerRotation,
			16.0f * Time.fixedDeltaTime);
		head.localRotation = Quaternion.Slerp (head.localRotation, headRotation,
			16.0f * Time.fixedDeltaTime);

		if (walking) {
			Walking ();
		} else {
			Staying ();
		}

		if (jumping) {
			jumping = false;
			Jumping ();
		}

		switch (sideWalk) {
		case FirstPersonHeadControllerSideWalk.NONE:
			SideWalkNone ();
			break;
		case FirstPersonHeadControllerSideWalk.LEFT:
			SideWalkLeft();
			break;
		case FirstPersonHeadControllerSideWalk.RIGHT:
			SideWalkRight ();
			break;
		}
	}

}
