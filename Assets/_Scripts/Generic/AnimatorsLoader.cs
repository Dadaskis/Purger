using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class AnimatorsLoader {
	public static AssetBundle bundle;

	public static void Load() {
		bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/animators"));
	}
}
