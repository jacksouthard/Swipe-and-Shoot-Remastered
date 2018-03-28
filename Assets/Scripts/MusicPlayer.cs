using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {
	public void PlaySong() {
		GetComponent<AudioSource> ().Play ();
	}
}
