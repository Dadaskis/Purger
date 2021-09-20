using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUD {

	public class Value : MonoBehaviour {

		private RectTransform size;
		private float minimum = 0.0f;

		private float _value = 1.0f;
		public float amount {
			set {
				_value = value;
			}

			get {
				return _value;
			}
		}

		private float visibleValue = 1.0f;
		private float speed = 5.0f;

		public bool vertical = false;
		
		void Start () {
			size = GetComponent<RectTransform> ();
			if (vertical) {
				minimum = size.rect.size.y;
			} else {
				minimum = size.rect.size.x;
			}
		}

		void Update () {
			visibleValue = Mathf.Lerp (visibleValue, _value, Time.deltaTime * speed);
			RectTransform.Axis axis = RectTransform.Axis.Horizontal;
			if (vertical) {
				axis = RectTransform.Axis.Vertical;
			}

			size.SetSizeWithCurrentAnchors (axis, Mathf.Lerp (0.0f, minimum, visibleValue));
		}
	
	}

}

