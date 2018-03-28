using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour {
	public void PlaySong() {
		GetComponent<AudioSource> ().Play ();
	}

	public void FadeOut() {
		SceneFader.FadeToColor (Color.white);
	}
}
