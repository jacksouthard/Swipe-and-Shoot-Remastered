using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
	public AudioSource soundEffect;

	public void Init(AudioClip audioClip, float pitch) {
		soundEffect.clip = audioClip;
		soundEffect.pitch = pitch;
		soundEffect.Play ();
	}

	void OnCollisionEnter(Collision other) {
		Explosion.Create (transform.position, 4, 50000, 10);
		Destroy (gameObject);
	}
}
