using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _HUDTest : MonoBehaviour {
	void Start() {
		HUDElements.AddElement ("HealthArmorAmmoData");
		HUDElements.AddElement ("OxygenData");
		HUDElements.AddElement ("TimerData");
		HUDElements.AddElement ("TargetInfo");
		HUDElements.AddElement ("Crosshair");
		StartCoroutine (Init ());
	}

	IEnumerator Init() {
		yield return new WaitForEndOfFrame ();
		HUDAmmo.ammo = 12;
		HUDAmmo.reloadAmmo = 300;
		HUDTimer.seconds = 60 * 5;
		HUDTarget.SetTarget ("This is an example!");
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.G)) {
			HUDHealth.amount -= 0.1f;
		}

		if (Input.GetKeyDown (KeyCode.T)) {
			HUDHealth.amount = 1.0f;
		}

		if (Input.GetKeyDown (KeyCode.H)) {
			HUDArmor.amount -= 0.1f;
		}

		if (Input.GetKeyDown (KeyCode.Y)) {
			HUDArmor.amount = 1.0f;
		}

		if (Input.GetMouseButtonDown (0)) {
			if (HUDAmmo.ammo > 0) {
				HUDAmmo.ammo -= 1;
				HUDCrosshair.spread += 50.0f;
			}
		}

		if (Input.GetKeyDown (KeyCode.R)) {
			int takeAmmo = 12 - HUDAmmo.ammo;
			HUDAmmo.reloadAmmo -= takeAmmo;
			HUDAmmo.ammo = 12;
		}

		if (Input.GetKeyDown (KeyCode.J)) {
			HUDOxygen.amount -= 0.05f;
		}

		if (Input.GetKeyDown (KeyCode.U)) {
			HUDOxygen.amount = 1.0f;
		}
	}
}
