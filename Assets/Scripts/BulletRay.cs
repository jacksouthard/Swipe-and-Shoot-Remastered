using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRay : MonoBehaviour {
	LineRenderer lr;
	Transform flash;

	public void Init (Vector3 start, Vector3 end, float time) {
		Destroy (gameObject, time);

		lr = GetComponent<LineRenderer> ();
		Vector3[] positions = new Vector3[2];
		positions [0] = start;
		positions [1] = end;

		flash = transform.GetChild (0);
		flash.position = start;
//		Vector3 dir = (end - start).normalized;
		flash.transform.LookAt (end);

		lr.SetPositions (positions);
	}
}
