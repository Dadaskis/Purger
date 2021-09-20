using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDCrosshair : MonoBehaviour {

	[HideInInspector] public Transform crosshair0;
	[HideInInspector] public Transform crosshair1;
	[HideInInspector] public Transform crosshair2;
	[HideInInspector] public Transform crosshair3;

	private float crosshair0Pos;
	private float crosshair1Pos;
	private float crosshair2Pos;
	private float crosshair3Pos;

	private float _spread = 0.0f;

	public static HUDCrosshair instance;

	public void SetSpread(float spread) {
		_spread = spread;

		Vector3 pos0 = crosshair0.localPosition;
		pos0.x = crosshair0Pos - spread;
		crosshair0.localPosition = pos0;

		Vector3 pos1 = crosshair1.localPosition;
		pos1.x = crosshair1Pos + spread;
		crosshair1.localPosition = pos1;

		Vector3 pos2 = crosshair2.localPosition;
		pos2.y = crosshair2Pos - spread;
		crosshair2.localPosition = pos2;

		Vector3 pos3 = crosshair3.localPosition;
		pos3.y = crosshair3Pos + spread;
		crosshair3.localPosition = pos3;

		float limit = 20.0f;
		float border = 40.0f;
		if (_spread > limit) {
			float colorAlpha = _spread - limit;
			colorAlpha = 1.0f - (colorAlpha / border);
			//Debug.Log (colorAlpha);
			colorAlpha = Mathf.Clamp01 (colorAlpha);

			Color color0 = crosshair0.GetComponent<Image> ().color;
			color0.a = colorAlpha;
			crosshair0.GetComponent<Image> ().color = color0;

			Color color1 = crosshair1.GetComponent<Image> ().color;
			color1.a = colorAlpha;
			crosshair1.GetComponent<Image> ().color = color1;

			Color color2 = crosshair2.GetComponent<Image> ().color;
			color2.a = colorAlpha;
			crosshair2.GetComponent<Image> ().color = color2;

			Color color3 = crosshair3.GetComponent<Image> ().color;
			color3.a = colorAlpha;
			crosshair3.GetComponent<Image> ().color = color3;
		}
	}

	public float GetSpread() {
		return _spread;
	}

	public static float spread {
		get {
			return instance.GetSpread ();
		}

		set {
			instance.SetSpread (value);
		}
	}

	private bool _enable = true;
	public static bool enable {
		get {
			return instance._enable;
		}

		set {
			instance._enable = value;

			instance.crosshair0.gameObject.SetActive (value);
			instance.crosshair1.gameObject.SetActive (value);
			instance.crosshair2.gameObject.SetActive (value);
			instance.crosshair3.gameObject.SetActive (value);
		}
	}

	void Start () {
		crosshair0 = transform.Find ("Crosshair0");
		crosshair1 = transform.Find ("Crosshair1");
		crosshair2 = transform.Find ("Crosshair2");
		crosshair3 = transform.Find ("Crosshair3");

		crosshair0Pos = crosshair0.localPosition.x;
		crosshair1Pos = crosshair1.localPosition.x;
		crosshair2Pos = crosshair2.localPosition.y;
		crosshair3Pos = crosshair3.localPosition.y;

		instance = this;
	}

	void Update() {
		spread = Mathf.Lerp (spread, 0.0f, 5.0f * Time.deltaTime);
	}

}
