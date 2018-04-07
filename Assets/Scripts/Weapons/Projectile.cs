using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
	public AudioSource soundEffect;
	GameObject model;
	ParticleSystem trail;

	public void Init(AudioClip audioClip, float pitch) {
		soundEffect.clip = audioClip;
		soundEffect.pitch = pitch;
		soundEffect.Play ();

		trail = GetComponent<ParticleSystem> ();
		model = transform.GetChild (0).gameObject;
	}

	void OnCollisionEnter(Collision other) {
		Explosion.Create (transform.position, 4, 50000, 10);
		Destroy (model);
		trail.Stop ();
		Destroy (gameObject, 2f);
	}
}
