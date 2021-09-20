using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVEffect {
	public float addFOV = 0.0f;
}

public class FOVEffects : MonoBehaviour {

	private Camera camera;
	private float FOV;
	public List<FOVEffect> effects = new List<FOVEffect>();
	public static FOVEffects instance;

	void Awake () {
		camera = GetComponent<Camera> ();
		FOV = camera.fieldOfView;
		instance = this;
	}

	public static FOVEffect RegisterEffect() {
		FOVEffect effect = new FOVEffect ();
		instance.effects.Add (effect);
		return effect;
	}

	void Update() {
		camera.fieldOfView = FOV;
		foreach (FOVEffect effect in effects) {
			camera.fieldOfView += effect.addFOV;
		}
	}

	public IEnumerator RemoveProcess(FOVEffect effect, float speed) {
		while (true) {
			effect.addFOV = Mathf.Lerp (effect.addFOV, 0.0f, speed * Time.deltaTime);
			if (effect.addFOV < 0.05f) {
				break;
			}
			yield return new WaitForEndOfFrame ();
		}
		effects.Remove (effect);
	}

	public static void Remove(FOVEffect effect, float speed = 5.0f) {
		instance.StartCoroutine (instance.RemoveProcess (effect, speed));
	}

}
