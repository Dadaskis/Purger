using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPuncher : MonoBehaviour {

	[HideInInspector] public Transform camera;
	[HideInInspector] public Vector3 punchVector = Vector3.zero;
	public static CameraPuncher instance;

	void Start() {
		instance = this;
		camera = transform;
	}

	public static void Punch(Vector3 power) {
		instance.punchVector += power;
	}

	void Update () {
		punchVector = Vector3.Slerp (punchVector, Vector3.zero, Time.deltaTime * 5.0f);
		camera.localRotation = Quaternion.Slerp(camera.localRotation, Quaternion.Euler (punchVector), Time.deltaTime * 10.0f);
	}
}
