using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
	public AudioSource audio;

	public void Init(AudioClip audioClip, float pitch) {
		audio.clip = audioClip;
		audio.pitch = pitch;
		audio.Play ();
	}

	void OnCollisionEnter(Collision other) {
		Explosion.Create (transform.position, 5, 100000, 20);
		Destroy (gameObject);
	}
}
