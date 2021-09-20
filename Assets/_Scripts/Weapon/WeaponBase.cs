using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour {

	[HideInInspector] public string assetBundle;

	public virtual void TakeUp() {}

	public virtual float TakeOff() {
		return 0.0f;
	}

	public virtual void Climbing() {}
}
