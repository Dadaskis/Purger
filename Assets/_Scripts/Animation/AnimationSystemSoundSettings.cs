using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create animation system sound settings", menuName = "New animation system sound settings")]
public class AnimationSystemSoundSettings : ScriptableObject {
	public float minDistance = 5.0f;
	public float maxDistance = 10.0f;
	public float spatialBlend = 1.0f;
}
