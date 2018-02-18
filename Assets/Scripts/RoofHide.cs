using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoofHide : MonoBehaviour {
	MeshRenderer roofRenderer;
	Color originalColor;
	Color fadedColor;

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
	}

	void OnTriggerEnter (Collider coll) {
		if (coll.tag == "Player") {
			roofRenderer.material.color = fadedColor;
		}
	}

	void OnTriggerExit (Collider coll) {
		if (coll.tag == "Player") {
			roofRenderer.material.color = originalColor;
		}
	}
}
