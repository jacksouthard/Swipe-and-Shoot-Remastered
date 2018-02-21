using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parachute : Rideable {

	void Awake () {
		base.Initiate ();
	}

	void OnCollisionEnter (Collision coll) {
		if (coll.gameObject.layer == 10) {
			if (driver) {
				base.Dismount ();
			}
			Destroy (gameObject);
		}
	}
}
