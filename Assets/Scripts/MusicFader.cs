using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicFader : MonoBehaviour {
	AudioSource music;
	float maxVolume;

	void Awake() {
		music = gameObject.GetComponent<AudioSource> ();
		maxVolume = music.volume;
		music.volume = 0f;

		StartCoroutine (AdjustVolume());
	}

	IEnumerator AdjustVolume() {
		while (true) {
			music.volume = (1 - SceneFader.curFadeAmount) * maxVolume;
			yield return new WaitForSecondsRealtime (Time.unscaledDeltaTime);
		}
	}
}
