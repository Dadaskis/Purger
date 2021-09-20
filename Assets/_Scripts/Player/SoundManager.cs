using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

[System.Serializable]
public class SoundType {
	public string typeName = "";
	public float volumeMultiplier = 1.0f;
}

[System.Serializable]
public class SoundMaterialType {
	public string typeName = "";
	public List<AudioClip> hitClipNames = new List<AudioClip> ();
	public List<AudioClip> walkClipNames = new List<AudioClip> ();
	public List<AudioClip> runClipNames = new List<AudioClip> ();
	public List<AudioClip> landingClipNames = new List<AudioClip> ();
}

public class SoundManager : MonoBehaviour {

	public static SoundManager instance;

	void Awake() {
		//InitializeClips ();
		instance = this;

		//try {
			//string json = System.IO.File.ReadAllText("Saves/Sound.settings");
		//} catch(System.Exception ex) {
			// ... fuck
		//}
	}

	public Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
	public Dictionary<string, float> clipVolumes = new Dictionary<string, float>();
	public Dictionary<string, SoundMaterialType> materialClips = new Dictionary<string, SoundMaterialType>();
	public GameObject soundObjectPrefab;
	public List<SoundType> types = new List<SoundType>();
	public List<SoundMaterialType> materialTypes = new List<SoundMaterialType>();
	public SoundMaterialType genericSound;

	private List<SoundObject> sounds = new List<SoundObject>();
	private Dictionary<string, float> typeVolume = new Dictionary<string, float>();

	public static void AddSoundObject(SoundObject obj) {
		instance.sounds.Add(obj);
	}

	public static void RemoveSoundObject(SoundObject obj) {
		instance.sounds.Remove (obj);
	}

	public static void ChangeVolumeOnType(string type, float volume) {
		foreach (SoundObject obj in instance.sounds) {
			if (obj.type == type) {
				obj.source.volume = volume;
			}
		}
		instance.typeVolume [type] = volume;
	}

	public static void ChangeVolumeOnTypeInversed(string type, float volume) {
		foreach (SoundObject obj in instance.sounds) {
			if (obj.type != type) {
				obj.source.volume = volume;
				instance.typeVolume [obj.type] = volume;
			}
		}
	}

	/*
	void InitializeClips() {
		clips = new Dictionary<string, AudioClip>();
		foreach (AudioClip sound in soundsList) {
			if (sound != null) {
				clips [sound.name] = sound;
			}
		}
		InitializeClipVolumes ();
		//InitializeMaterialClips ();
	}

	void InitializeClipVolumes() {
		foreach (KeyValuePair<string, AudioClip> clip in clips) {
			AudioClip obj = clip.Value;
			float volume = 1.0f;
			foreach (SoundType type in types) {
				if (obj.name.Contains (type.typeName)) {
					volume *= type.volumeMultiplier;
				}
			}
			clipVolumes [obj.name] = volume;
		}
	}

	void InitializeMaterialClips() {
		Material[] objects = Resources.LoadAll ("", typeof(Material)).Cast<Material>().ToArray();
		foreach (Material material in objects) {	
			foreach (SoundMaterialType type in materialTypes) {
				if (material.name.Contains (type.typeName)) {
					materialClips [material.name] = type;
					break;
				}
			}
		}
	}*/

	/*public static SoundObjectData GetBasicSoundObjectData(string soundName) {
		SoundObjectData data = new SoundObjectData ();
		AudioClip clip;
		if (instance.clips.TryGetValue (soundName, out clip)) {
			data.clip = clip;
			data.volume = instance.clipVolumes [soundName];
		}
		return data;
	}*/

	public static SoundMaterialType GetSoundMaterialType(string material) {
		SoundMaterialType type;
		if (instance.materialClips.TryGetValue (material, out type)) {
			return type;
		} 
		//Debug.LogError ("[SoundManager] Cant get sound material type: " + material);
		return instance.genericSound;
	}

	public static SoundMaterialType GetSoundMaterialType(Material material) {
		return GetSoundMaterialType (material.name);
	}

	public static SoundObject CreateSound(SoundObjectData soundData, Vector3 position = default(Vector3), Transform parent = null) {
		if (soundData.clip == null) {
			return null;
		}

		GameObject soundObject = GameObject.Instantiate (instance.soundObjectPrefab);
		SoundObject sound = soundObject.GetComponent<SoundObject> ();
		sound.SetClip (soundData);
		soundObject.transform.SetParent (parent);
		soundObject.transform.position = position;
		sound.type = soundData.type;
		//float volume;
		//if (instance.typeVolume.TryGetValue (sound.type, out volume)) { 
		//	sound.source.volume = volume;
		//}
		return sound;
	}

}
