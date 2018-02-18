using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoofHide : MonoBehaviour {
	public float fadeSpeed = 5f;

	MeshRenderer roofRenderer;
	Color originalColor;
	Color fadedColor;

	Color targetColor;

	void Start () {
		// get roofs
		for (int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild (i);
			if (child.name.Contains ("Roof")) {
				roofRenderer = child.GetComponent<MeshRenderer>();
			}
		}

		// set colors
		originalColor = roofRenderer.material.color;
		fadedColor = new Color (originalColor.r, originalColor.g, originalColor.b, 0.25f);

		targetColor = originalColor;
	}

	void LateUpdate() {
		roofRenderer.material.color = Color.Lerp (roofRenderer.material.color, targetColor, Time.deltaTime * fadeSpeed);
	}

	void OnTriggerEnter (Collider coll) {
		if (coll.tag == "Player") {
			targetColor = fadedColor;
		}
	}

	void OnTriggerExit (Collider coll) {
		if (coll.tag == "Player") {
			targetColor = originalColor;
		}
	}
}
