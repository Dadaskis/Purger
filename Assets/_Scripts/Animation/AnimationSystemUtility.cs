using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationSystemUtility {

	public static void FindAnimations(AnimationSystem animationSystem, string prefabName) {
		AnimationsGetter.ResultData data = AnimationsGetter.GetAnimations (animationSystem.assetBundle, prefabName);
		List<AnimationData.NameIndexPair> pairs = new List<AnimationData.NameIndexPair> ();
		foreach (AnimationsGetter.NameIndexPair pair in data.nameByIndex) {
			AnimationData.NameIndexPair newPair = new AnimationData.NameIndexPair ();
			newPair.index = pair.index;
			newPair.name = pair.name;
			pairs.Add (newPair);
		}
		animationSystem.FindAnimations (data.clips, pairs);
	}

	public static void CreateBase(AnimationSystem animationSystem, string prefabName) {
		//animationSystem.animationDataList.Clear ();
		//animationSystem.triggerPointers.Clear ();
		foreach (AnimationClip clip in animationSystem.animatorOverride.animationClips) {
			//Debug.LogError (clip);
			//animationSystem.animationDataList
			if (clip != null) {
				AnimationData data = new AnimationData ();
				data.animatorClipName = clip.name;
				data.modelFindName = clip.name;
				animationSystem.animationDataList.Add (data);

				AnimationTriggerPointer pointer = new AnimationTriggerPointer ();
				pointer.animationName = clip.name;
				pointer.triggerName = clip.name;
				animationSystem.triggerPointers.Add (pointer);
			}
		}
		FindAnimations (animationSystem, prefabName);
	}

}