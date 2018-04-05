using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
	public AudioSource buildupAudio;
	public AudioSource mainAudio;

	public void PlayFromBeginning() {
		buildupAudio.Play ();
		mainAudio.PlayDelayed (buildupAudio.clip.length);
	}

	public void PlayMain() {
		mainAudio.Play ();
	}
}
