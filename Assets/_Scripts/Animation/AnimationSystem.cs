using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimationData {
	[HideInInspector] public AnimationSystem system;
	public string animatorClipName;
	public string modelFindName;
	public AnimationClip animation;

	public void Apply(ref AnimationClipOverrides overrides) {
		overrides [animatorClipName] = animation;
	}

	//#if UNITY_EDITOR

	public class NameIndexPair {
		public int index;
		public string name;
	}

	private int GetClip(string name, List<NameIndexPair> nameByIndex){
		foreach (NameIndexPair pair in nameByIndex) {
			if (pair.name.Contains (name)) {
				return pair.index;
			}
		}
		return -1;
	}

	private AnimationClip GetClipFromArray(string name, List<AnimationClip> clips, List<NameIndexPair> pairs) {
		int animationIndex = GetClip (name, pairs);
		if (animationIndex >= 0) {
			return clips [animationIndex];
		}
		return null;
	}

	public void FindAnimation(List<AnimationClip> clips, List<NameIndexPair> nameByIndex) {
		animation = GetClipFromArray (modelFindName, clips, nameByIndex);
	}

	//#endif
}

[System.Serializable]
public class AnimationTriggerPointer {
	public string triggerName;
	public string animationName;
}


// Animation events:
// AnimationSystemPlaySound - Play sound. String used for sound name, object used for AnimationSystemSoundSettings where is data for sound settings
// is provided such as minimum distance, maximum distance and spatial blend.
//
// AnimationSystemStartComplexTrigger - Overrides the ending of animation. Instead of waiting some seconds for the end, we are taking control
// when the end of animation will happen. Usable for chains of animations like in pump shotguns reload
//
// AnimationSystemEndComplexTrigger - We are saying that the end of animation is happen
public class AnimationSystem : MonoBehaviour {
	private Animator animator = null;
	[HideInInspector] public AnimatorOverrideController animatorOverride = null;
	private AnimationClipOverrides animationOverrides = null;
	[HideInInspector] public string assetBundle;
	private bool animationPlaying = false;
	[HideInInspector] public List<AnimationData> animationDataList = new List<AnimationData>();
	[HideInInspector] public List<AnimationTriggerPointer> triggerPointers = new List<AnimationTriggerPointer> ();
	private Dictionary<string, AnimationData> animationDataMap = new Dictionary<string, AnimationData>();
	private Dictionary<string, AnimationData> triggerAnimationMap = new Dictionary<string, AnimationData>();
	private bool complexTrigger = false;
	private List<AnimationOverMethodType> currentCallbacks;

	public delegate void AnimationOverMethodType ();

	public void AnimationSystemPlaySound(AnimationEvent animationEvent) {
		AnimationSystemSoundSettings soundSettings = animationEvent.objectReferenceParameter as AnimationSystemSoundSettings;
		if (soundSettings != null) {			
			SoundObjectData data = new SoundObjectData ();
			data.clip = animationEvent.objectReferenceParameter as AudioClip;
			data.minDistance = soundSettings.minDistance;
			data.maxDistance = soundSettings.maxDistance;
			data.spatialBlend = soundSettings.spatialBlend;
			SoundManager.CreateSound (data, new Vector3(), transform);
		}
	}

	public void AnimationSystemStartComplexTrigger() {
		complexTrigger = true;
		animationPlaying = true;
	}

	public void AnimationSystemEndComplexTrigger() {
		animationPlaying = false;
		complexTrigger = false;
		foreach (AnimationOverMethodType callback in currentCallbacks) {
			callback ();
		}
		currentCallbacks = null;
	}

	public void Initialize(string prefabName) {
		animator = GetComponent<Animator> ();
		animatorOverride = (AnimatorOverrideController) animator.runtimeAnimatorController;
		AnimationSystemUtility.CreateBase (this, prefabName);
		ApplyAnimations();
		InitializeAnimationDataMap ();
		InitializeTriggerAnimationMap ();
	}

	public void InitializeAnimationDataMap() {
		foreach (AnimationData data in animationDataList) {
			animationDataMap [data.animatorClipName] = data;
		}
	}

	public void InitializeTriggerAnimationMap() {
		foreach(AnimationTriggerPointer pointer in triggerPointers) {
			triggerAnimationMap [pointer.triggerName] = animationDataMap [pointer.animationName];
		}
	}

	IEnumerator AnimationPlaying(float seconds, List<AnimationOverMethodType> callbacks = default(List<AnimationOverMethodType>)) {
		animationPlaying = true;
		currentCallbacks = callbacks;
		yield return new WaitForSeconds (seconds);
		if (complexTrigger) {
			yield break;
		}
		animationPlaying = false;
		foreach (AnimationOverMethodType callback in currentCallbacks) {
			callback ();	 
		}
	}

	public void SetTrigger(string triggerName, AnimationOverMethodType callback) {
		List<AnimationOverMethodType> callbacks = new List<AnimationOverMethodType> ();
		callbacks.Add (callback);
		SetTrigger (triggerName, callbacks);
	}

	public void SetTrigger(string triggerName, List<AnimationOverMethodType> callbacks) {
		if (animationPlaying) {
			return;
		}
		AnimationData data;
		if (triggerAnimationMap.TryGetValue (triggerName, out data)) {
			if (data.animation != null) {
				StartCoroutine (AnimationPlaying (data.animation.length, callbacks));
				animator.SetTrigger (triggerName);
			}
		} else {
			Debug.LogError ("Animation System: Cant find trigger in triggerAnimationMap: " + triggerName);
		}
	}

	public void ApplyAnimations() {
		if (animatorOverride == null) {
			animatorOverride = new AnimatorOverrideController (animator.runtimeAnimatorController);
			animator.runtimeAnimatorController = animatorOverride;
		}
		if (animationOverrides == null) {
			animationOverrides = new AnimationClipOverrides (animatorOverride.overridesCount);
			animatorOverride.GetOverrides (animationOverrides);
		}
		foreach (AnimationData data in animationDataList) {
			data.system = this;
			data.Apply (ref animationOverrides);
		}
		animatorOverride.ApplyOverrides (animationOverrides);
	}

	public float GetLength(string name) {
		return triggerAnimationMap [name].animation.length;
	}

	//#if UNITY_EDITOR
	public void FindAnimations(List<AnimationClip> clips, List<AnimationData.NameIndexPair> nameByIndex) {
		foreach (AnimationData data in animationDataList) {
			data.FindAnimation (clips, nameByIndex);
		}
	}
	//#endif

}
