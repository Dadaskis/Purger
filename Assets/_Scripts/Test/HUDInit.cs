using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDInit : MonoBehaviour {

	void Start () {
		StartCoroutine (HUDInitialize ());
	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			//WeaponManager.SetPlayerWeapon ("weapon_tester");
			Player.SetWeapon("firearm_tester");
		}

		//if (Input.GetKeyDown (KeyCode.Alpha2)) {
			//WeaponManager.SetPlayerWeapon ("weapon_tester");
			//Player.SetWeapon("firearm_tester_shotgun");
		//}
	}

	IEnumerator HUDInitialize() {
		yield return new WaitForEndOfFrame();
		//HUDElements.AddElement ("HealthData");
		HUDElements.AddElement ("HealthArmorAmmoData");
		HUDElements.AddElement ("TargetInfo");
		yield return new WaitForEndOfFrame();
		HUDTarget.SetTarget ("Fool around!");

		HUDArmor.amount = Player.armor / Player.maxArmor;
		HUDHealth.amount = Player.health / Player.maxHealth;
		//yield return new WaitForEndOfFrame();
		//yield return new WaitForEndOfFrame();
		//WeaponManager.SetPlayerWeapon ("weapon_tester");

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		WeaponManager.SetHands ("default");
	}
}
