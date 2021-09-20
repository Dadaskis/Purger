using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
using System.IO;
	
public static class AnimationsGetter {

	public class NameIndexPair {
		public int index;
		public string name;
	}

	public class ResultData {
		public List<AnimationClip> clips;
		public List<NameIndexPair> nameByIndex;
	}

	public static ResultData GetAnimations(string assetBundle, string prefabName) {
		List<AnimationClip> clips = new List<AnimationClip>();// = AnimationUtility.GetAnimationClips ();

		//Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath (modelPath);
		AssetBundle bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/" + assetBundle));
		if (bundle == null) {
			Debug.LogError ("Bundle is null. Taking animators bundle instead");
			bundle = AnimatorsLoader.bundle;
		}
		Object[] objects = bundle.LoadAssetWithSubAssets (prefabName);
		bundle.Unload (false);
		//Object file = Resources.Load(system.modelPath);
		//AnimationClip[] loadedClips = AnimationUtility.GetAnimationClips(AssetDatabase.LoadAssetAtPath<GameObject>(system.modelPath));

		foreach (Object obj in objects) {
			AnimationClip clip = obj as AnimationClip;
			if (clip != null) {
				clips.Add (clip);
			}
		}

		List<NameIndexPair> nameByIndex = new List<NameIndexPair> ();
		int index = 0;
		foreach (AnimationClip clip in clips) {
			NameIndexPair pair = new NameIndexPair ();
			pair.name = clip.name;
			pair.index = index;
			index++;
			nameByIndex.Add (pair);
		}

		ResultData data = new ResultData ();
		data.clips = clips;
		data.nameByIndex = nameByIndex;
		return data;
	}

}