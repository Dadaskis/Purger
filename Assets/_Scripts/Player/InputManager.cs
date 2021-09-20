using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//using UnityStandardAssets.Characters.FirstPerson;
using Newtonsoft.Json;

public class InputManager : MonoBehaviour {

	public class KeyPressedEvent : UnityEvent<KeyCode> {}
	public KeyPressedEvent onKeyPressed = new KeyPressedEvent();

	public InputData data;

	public static InputManager instance;

	public Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode> ();
	public Dictionary<string, string> normalNames = new Dictionary<string, string>();
	//public float mouseSensitivity = 2.0f;

	public void Load() {
		try {
			string json = System.IO.File.ReadAllText("Saves/Input.settings");
			keys = JsonConvert.DeserializeObject<Dictionary<string, KeyCode>>(json);
		} catch(System.Exception ex) {
			// ... fuck
			Debug.LogError(ex);
		}
	}

	public void Save() {
		try {
			string json = JsonConvert.SerializeObject(keys);
			System.IO.File.WriteAllText("Saves/Input.settings", json);
		} catch(System.Exception ex) {
			// What do you mean? What is exception? I dont know what is this either
			Debug.LogError(ex);
		}
	}

	public void Awake() {
		instance = this;
		foreach (InputKey key in data.keys) {
			keys.Add (key.name, key.key);
			normalNames.Add (key.name, key.normalName);
		}
		Load ();
	}
		
	void CheckKeys() {
		foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode))) {
			if (Input.GetKey (key)) {
				onKeyPressed.Invoke (key);
			}
		}
	}

	public void Update() {
		CheckKeys ();
	}

	public static bool GetButtonDown(string name) {
		return Input.GetKeyDown (instance.keys [name]);
	}

	public static bool GetButtonUp(string name) {
		return Input.GetKeyUp (instance.keys [name]);
	}

	public static bool GetButton(string name) {
		if (instance != null) {
			return Input.GetKey (instance.keys [name]);
		}
		return false;
	}

}
