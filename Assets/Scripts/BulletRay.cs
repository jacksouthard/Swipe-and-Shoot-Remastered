using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRay : MonoBehaviour {
	public AudioSource audio;

	LineRenderer lr;
	Transform flash;

	bool inited = false;

	bool rayVisible = true;
	float rayVisibleTime = 0.05f;
	float audioTime;
	float timer = 0f;

	void Update () {
		if (inited) {
			if (rayVisible) {
				timer += Time.deltaTime;
				if (timer > rayVisibleTime) {
					lr.enabled = false;
					transform.Find ("Flash").gameObject.SetActive (false);
				}
			}
		}
	}

	public void Init (Vector3 start, Vector3 end, AudioClip audioClip, float pitch) {
		audio.clip = audioClip;
		audio.pitch = pitch;
		audio.Play ();

		audioTime = audio.clip.length; 
		Destroy (gameObject, audioTime);

		lr = GetComponent<LineRenderer> ();
		Vector3[] positions = new Vector3[2];
		positions [0] = start;
		positions [1] = end;

		flash = transform.GetChild (0);
		flash.position = start;
//		Vector3 dir = (end - start).normalized;
		flash.transform.LookAt (end);

		lr.SetPositions (positions);

		inited = true;
	}
}
