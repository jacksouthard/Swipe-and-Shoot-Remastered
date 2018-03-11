using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : MonoBehaviour {
	public float launchTime;
	ParticleSystem[] particles;

	float timer;

	void Start() {
		particles = GetComponentsInChildren<ParticleSystem> ();
	}

	public void Launch() {
		timer = launchTime;
		foreach (ParticleSystem jet in particles) {
			jet.Play ();
		}
	}

	void LateUpdate() {
		if (timer > 0f) {
			timer -= Time.deltaTime;
			if (timer <= 0f) {
				foreach (ParticleSystem jet in particles) {
					jet.Stop ();
				}
			}
		}
	}
}
