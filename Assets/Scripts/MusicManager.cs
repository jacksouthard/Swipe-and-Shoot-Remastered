using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
	public AudioSource buildupAudio;
	public AudioSource mainAudio;
	public AudioSource secondaryAudio;

	public void PlayFromBeginning() {
		buildupAudio.Play ();
		mainAudio.PlayDelayed (buildupAudio.clip.length);
		if (secondaryAudio != null) {
			secondaryAudio.PlayDelayed(buildupAudio.clip.length);
			secondaryAudio.volume = 0f;
		}
	}

	public void PlayMain() {
		mainAudio.Play ();
		if (secondaryAudio != null) {
			secondaryAudio.Play ();
			secondaryAudio.volume = 0f;
		}
	}

	public void SadifyMusic() {
		StartCoroutine (Sadification());
		if (secondaryAudio != null) {
			secondaryAudio.Stop ();
		}
	}

	IEnumerator Sadification() {
		float p = mainAudio.pitch;
		float t = Time.fixedUnscaledDeltaTime / 5;
		while (p > 0) {
			mainAudio.pitch = p;
			p -= t;
			yield return new WaitForSecondsRealtime (t);
		}

		mainAudio.Stop ();
	}

	public void SecondarifyMusic() {
		StartCoroutine (Secondarify());
	}

	IEnumerator Secondarify() {
		float v = 0f;
		float t = Time.fixedUnscaledDeltaTime / 2;
		while (v < 0.25f) {
			secondaryAudio.volume = v;
			v += t;
			yield return new WaitForSecondsRealtime (t);
		}

		secondaryAudio.volume = 0.25f;
	}
}
