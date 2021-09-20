using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDHealth : MonoBehaviour {

	[HideInInspector] public HUD.Value _value;
	public static HUDHealth instance;

	public static float amount {
		set {
			instance._value.amount = value;
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
