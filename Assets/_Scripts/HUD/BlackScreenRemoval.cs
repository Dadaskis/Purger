using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackScreenRemoval : MonoBehaviour {

	private Image image;
	private float startTime = 0.0f;
	private float alpha = 1.0f;

	void Start () {
		image = GetComponent<Image> ();
		startTime = Time.time;
	}

	void Update () {
		if (alpha > 0.05f) {
			Color color = new Color (0.0f, 0.0f, 0.0f, alpha);
			alpha = Mathf.Lerp (color.a, 0.0f, Time.deltaTime * 0.3f);
			image.color = color;
			image.rectTransform.SetAsLastSibling ();
			if (alpha < 0.05f) {
				alpha = 0.0f;
			}
		}
	}
}
