using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class StartGameLoadMenu : MonoBehaviour {
	
	void Start () {
		AssetBundle bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/mainmenu"));
		string path = bundle.GetAllScenePaths () [0];
		SceneManager.LoadScene (path);
	}

}
