using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDOxygen : MonoBehaviour {

	[HideInInspector] public HUD.Value _value;
	public static HUDOxygen instance;

	public static float amount {
		set {
			if (instance == null) {
				return;
			}
			instance._value.amount = Mathf.Clamp01(value);
		}

		get {
			return instance._value.amount;
		}
	}

	void Start () {
		_value = GetComponentInChildren<HUD.Value> ();
		instance = this;
	}

}
