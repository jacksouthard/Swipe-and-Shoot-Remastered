using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour {
	public AudioSource explosionSound;

	public void PlayExplosion() {
		explosionSound.Play ();
	}
}
