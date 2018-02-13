using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenEffect : MonoBehaviour {
	Transform target;
	bool initiated = false;

	public void Init (Transform _target) {
		target = _target;
		initiated = true;
	}
	
	void Update () {
		if (initiated) {
			transform.position = target.position;
		}
	}
}
