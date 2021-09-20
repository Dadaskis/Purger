using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenuLoadPackage : MonoBehaviour {

	public string[] paths;

	public void OnClick () {
		foreach (string path in paths) {
			AssetBundle bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/" + path));
			bundle.LoadAllAssets ();
		}
		//AssetBundle sceneBundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/testpropscene"));
		//string scenePath = sceneBundle.GetAllScenePaths () [0];
		//SceneManager.LoadScene (scenePath);
		SceneManager.LoadScene("TestPropScene/TestPropScene");
	}

}
