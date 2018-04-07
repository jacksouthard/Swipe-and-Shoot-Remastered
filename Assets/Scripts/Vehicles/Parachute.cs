using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parachute : Rideable {

	void Awake () {
		base.Initiate ();
		engine = null; //the audio source on this object should not be our "engine"
	}

	void OnCollisionEnter (Collision coll) {
		if (coll.gameObject.GetComponentInParent<Rigidbody>() == null) {
			if (driver) {
				base.Dismount ();
			}
			Destroy (gameObject);
		}
	}
}
