using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDAmmo : MonoBehaviour {

	private Text text;

	[HideInInspector] public int _ammo;
	public static int ammo {
		set {
			instance._ammo = value;
			instance.UpdateText ();
		}

		get {
			return instance._ammo;
		}
	}

	[HideInInspector] public int _reloadAmmo;
	public static int reloadAmmo {
		set {
			instance._reloadAmmo = value;
			instance.UpdateText ();
		}

		get {
			return instance._reloadAmmo;
		}
	}

	public void UpdateText() {
		text.text = _ammo + " / " + _reloadAmmo;
	}

	public static void NoWeapon() {
		instance.text.text = "- / -";
	}

	public static HUDAmmo instance;
	
	void Start () {
		text = GetComponent<Text> ();
		instance = this;
		NoWeapon ();
	}
}
