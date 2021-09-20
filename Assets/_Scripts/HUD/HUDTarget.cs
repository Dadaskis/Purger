using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDTarget : MonoBehaviour {

	private Text[] texts;
	private List<float> textsAlphas = new List<float>();
	private Image[] images;
	private List<float> imagesAlphas = new List<float>();

	public static HUDTarget instance;

	private Text targetText;

	private float transitionSpeed = 3.0f;
	private float waitSeconds = 3.0f;
	
	void Start () {
		texts = GetComponentsInChildren<Text> ();
		images = GetComponentsInChildren<Image> ();

		foreach (Text text in texts) {
			textsAlphas.Add (text.color.a);
		}

		foreach (Image image in images) {
			imagesAlphas.Add (image.color.a);
		}

		instance = this;

		targetText = transform.Find ("TargetText").GetComponent<Text> ();

		SetAlpha (0.0f);
	}

	public void SetAlpha(float alpha) {
		for (int index = 0; index < textsAlphas.Count; index++) {
			Color color = texts [index].color;
			color.a = Mathf.Lerp (0.0f, textsAlphas [index], alpha);
			texts [index].color = color;
		}

		for (int index = 0; index < imagesAlphas.Count; index++) {
			Color color = images [index].color;
			color.a = Mathf.Lerp (0.0f, imagesAlphas [index], alpha);
			images [index].color = color;
		}
	}

	public IEnumerator SetTargetProcess(string target) {
		targetText.text = target;
		float alpha = 0.0f;
		SetAlpha (0.0f);
		while (alpha < 0.98f) {
			alpha = Mathf.Lerp (alpha, 1.0f, transitionSpeed * Time.deltaTime);
			SetAlpha (alpha);
			yield return new WaitForEndOfFrame ();
		}
		yield return new WaitForSeconds (waitSeconds);
		while (alpha > 0.02f) {
			alpha = Mathf.Lerp (alpha, 0.0f, transitionSpeed * Time.deltaTime);
			SetAlpha (alpha);
			yield return new WaitForEndOfFrame ();
		}
		SetAlpha (0.0f);
	}

	public static void SetTarget(string target) {
		instance.StartCoroutine (instance.SetTargetProcess (target));
	}
}
