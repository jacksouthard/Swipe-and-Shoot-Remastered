using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectFollow : MonoBehaviour {
	Transform target;
	bool initiated = false;

	public void Init (Transform _target) {
		target = _target;
		initiated = true;
	}
	
	void LateUpdate () {
		if (initiated) {
			transform.position = target.position;
		}
	}

	public void End() {
		GetComponent<ParticleSystem> ().Stop ();
		Destroy (gameObject, 1.0f); //wait before actually destroying
	}
}
