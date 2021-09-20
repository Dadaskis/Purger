using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _NPCWalkTest_CameraFollower : MonoBehaviour {

	private Transform NPC;
	private Transform camera;
	private List<Vector3> offsets = new List<Vector3>();
	private int cameraIndex = 1;
	private Vector3 currentPosition = new Vector3();
	private Transform lookAt;
	private NPCSoldier NPCScript;
	private bool goingFront = true;
	
	void Start () {
		StartCoroutine (Find ());

		offsets.Add (new Vector3 (0.0f, 3.0f, 0.0f));
		offsets.Add (new Vector3 (3.0f, 1.5f, 0.0f));
		offsets.Add (new Vector3 (0.0f, 1.5f, 3.0f));
		offsets.Add (new Vector3 (0.0f, 1.5f, -3.0f));

		currentPosition = offsets [1];

		lookAt = GameObject.Find ("LookAt").transform;
	}

	IEnumerator Find() {
		yield return new WaitForEndOfFrame ();
		NPC = GameObject.Find ("npc_generic").transform;
		NPCScript = NPC.GetComponent<NPCSoldier> ();
		camera = GameObject.Find ("__Camera").transform;
		camera.position = currentPosition;
		camera.LookAt (NPC.position + new Vector3(0.0f, 1.5f, 0.0f));
		WeaponManager.SetNPCWeapon (NPC.gameObject, "firearm_tester");
	}

	void Update () {
		if (camera == null || NPC == null) {
			return;
		}

		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			cameraIndex++;
			if (cameraIndex > offsets.Count - 1) {
				cameraIndex = 0;
			}
		}

		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			cameraIndex--;
			if (cameraIndex < 0) {
				cameraIndex = offsets.Count - 1;
			}
		}

		if (Input.GetKeyDown (KeyCode.C)) {
			if (!NPCScript.IsCrouching ()) {
				NPCScript.Crouch ();
			} else {
				NPCScript.Uncrouch ();
			}
		}
			
		//if (Input.GetKeyDown (KeyCode.Alpha3)) {
		//	WeaponManager.SetNPCWeapon (NPC.gameObject, "firearm_tester");
		//}

		//if (Input.GetKeyDown (KeyCode.Alpha4)) {
		//	WeaponManager.SetNPCWeapon (NPC.gameObject, "firearm_tester_pistol");
		//}

		if (Input.GetKeyDown (KeyCode.W)) {
			NPCScript.MoveFront(Vector3.forward * 10.0f);
			goingFront = true;
		}

		if (Input.GetKeyDown (KeyCode.A)) {
			NPCScript.MoveSide(-Vector3.right * 10.0f);
			goingFront = false;
		}

		if (Input.GetKeyDown (KeyCode.S)) {
			//NPCScript.MoveFront(-Vector3.forward * 10.0f);
			NPCScript.MoveBack(-Vector3.forward * 10.0f);
			goingFront = true;
		}

		if (Input.GetKeyDown (KeyCode.D)) {
			NPCScript.MoveSide(Vector3.right * 10.0f);
			goingFront = false;
		}

		currentPosition = Vector3.Slerp (currentPosition, offsets [cameraIndex], 3.0f * Time.deltaTime);
		camera.position = NPC.position + currentPosition;
		camera.LookAt (NPC.position + new Vector3(0.0f, 1.5f, 0.0f));
		NPCScript.LookAt (lookAt.position);
	}

	void OnDrawGizmos() {
		if (lookAt == null) {
			lookAt = GameObject.Find ("LookAt").transform;
		}
		Gizmos.color = Color.white;
		Gizmos.DrawSphere (lookAt.position, 0.3f);
	}

}
