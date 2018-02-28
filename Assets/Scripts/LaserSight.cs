using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSight : MonoBehaviour {
	public float maxRange = 200f;
	LineRenderer lr;
	Vector3 lastPos;

	void Start () {
		lr = GetComponent<LineRenderer> ();
		lastPos = transform.position;
		UpdateLaser ();
	}

	void LateUpdate () {
		Vector3 diff = transform.position - lastPos;
		if (diff.magnitude > 0.005f) {
			UpdateLaser ();
		}

		lastPos = transform.position;
	}
	
	void UpdateLaser () {
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.forward, out hit, maxRange);

		if (hit.collider != null) {
			lr.positionCount = 2;	
			lr.SetPosition (0, transform.position);
			lr.SetPosition (1, hit.point);
		} else {
			lr.positionCount = 0;
		}
	}
}
