using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HUDElements : MonoBehaviour {

	public static HUDElements instance;

	public void Awake() {
		instance = this;
	}

	public static void AddElement(string name) {
		Transform checkObj = instance.transform.Find (name);
		if (checkObj != null) {
			return;
		}
		AssetBundle bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/hud"));
		GameObject prefab = bundle.LoadAsset<GameObject> (name);
		GameObject element = Instantiate (prefab, instance.transform);
		element.name = name;
		bundle.Unload (false);
	}

	public static void RemoveElement(string name) {
		Transform removeObj = instance.transform.Find (name);
		if (removeObj != null) {
			Destroy (removeObj.gameObject);
		}
	}

}
