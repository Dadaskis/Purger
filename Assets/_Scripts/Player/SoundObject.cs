﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundObjectData {
	public AudioClip clip = null;
	public float volume = 1.0f; 
	public bool loop = false;
	public float destroyTime = -1.0f;
	public bool destroyAfterPlaying = true;
	public float minDistance = 2.0f;
	public float maxDistance = 10.0f;
	public float spatialBlend = 1.0f;
	public float pitch = 1.0f;
	public string type = "Default";
}

public class SoundObject : MonoBehaviour {

	public AudioSource source;
	public string type = "Default";

	void Start() {
		SoundManager.AddSoundObject (this);
	}

	void OnDestroy() {
		SoundManager.RemoveSoundObject (this);
	}

	public void SetClip(SoundObjectData data) {
		source.clip = data.clip;
		source.loop = data.loop;
		source.volume = data.volume;
		source.rolloffMode = AudioRolloffMode.Logarithmic;
		source.minDistance = data.minDistance;
		source.maxDistance = data.maxDistance;
		source.spatialBlend = data.spatialBlend;
		source.pitch = data.pitch;
		source.PlayOneShot (data.clip, data.volume);
		if (data.destroyTime > 0.0f || data.destroyAfterPlaying) {
			if (data.destroyAfterPlaying) {
				Destroy (gameObject, data.clip.length);
			} else {
				Destroy (gameObject, data.destroyTime);
			}
		}
	}

}
