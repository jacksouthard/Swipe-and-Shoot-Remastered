using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour {
	public AudioSource explosionSound;
	public AudioSource doorSound;
	public AudioSource thrusterSound;

	public void PlayDoorSound() {
		doorSound.Play ();
	}

	public void PlayExplosion() {
		explosionSound.Play ();
	}

	public void Rethrust() {
		thrusterSound.Play ();
	}
}
