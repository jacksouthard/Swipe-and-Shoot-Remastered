using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLooper : MonoBehaviour {
	int index = 0;
	float clipTimer;
	List<AudioClip> clips = new List<AudioClip>();
	AudioSource source;

	void Start () {
		source = GetComponent<AudioSource> ();

		Object[] objects = Resources.LoadAll ("AudioTemp");
		foreach (var obj in objects) {
			if (obj.GetType () == typeof(AudioClip)) {
				clips.Add (obj as AudioClip);
			}
		}

		PlayNext ();
	}

	void PlayNext () {
		if (index > clips.Count - 2) {
			index = 0;
		}
		index++;

		source.clip = clips [index];
		clipTimer = clips [index].length;
		source.Play ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			PlayNext ();
		} else {
			clipTimer -= Time.deltaTime;
			if (clipTimer <= 0f) {
				PlayNext ();
			}
		}
	}
}
