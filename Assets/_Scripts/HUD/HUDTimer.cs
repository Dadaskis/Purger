using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDTimer : MonoBehaviour {
	private Text text;

	[HideInInspector] public int _seconds = 0;
	public static int seconds {
		set {
			instance._seconds = value;
			instance.UpdateText ();
		}

		get {
			return instance._seconds;
		}
	}

	public static HUDTimer instance;

	public void UpdateText() {
		text.text = "";
		int visualSeconds = seconds;
		int hours = (visualSeconds / 60) / 60;
		visualSeconds -= hours * 60 * 60;
		int minutes = visualSeconds / 60;
		visualSeconds -= minutes * 60;

		if (hours < 10) {
			text.text += "0" + hours + ":";
		} else {
			text.text += hours + ":";
		}

		if (minutes < 10) {
			text.text += "0" + minutes + ":";
		} else {
			text.text += minutes + ":";
		}

		if (visualSeconds < 10) {
			text.text += "0" + visualSeconds;
		} else {
			text.text += visualSeconds;
		}
	}

	void Start () {
		instance = this;
		text = GetComponentInChildren<Text> ();
		StartCoroutine (Loop ());
	}

	IEnumerator Loop() {
		while (true) {
			yield return new WaitForSeconds (1.0f);
			if (seconds > 0) {
				seconds--;
			}
		}
	}
}
