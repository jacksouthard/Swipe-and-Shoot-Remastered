using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRay : MonoBehaviour {
	bool live = false;

	float lifeTime;
	float timer;

	LineRenderer lr;
	Transform flash;

	public void Init (Vector3 start, Vector3 end, float time) {
		lifeTime = time;

		lr = GetComponent<LineRenderer> ();
		Vector3[] positions = new Vector3[2];
		positions [0] = start;
		positions [1] = end;

		flash = transform.GetChild (0);
		flash.position = start;
//		Vector3 dir = (end - start).normalized;
		flash.transform.LookAt (end);

		lr.SetPositions (positions);

		live = true;
	}
	
	void Update () {
		if (live) {
			lifeTime -= Time.deltaTime;
			if (lifeTime <= 0f) {
				Destroy (gameObject);
			}
		}
	}
}
