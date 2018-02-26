using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killzone : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		Health otherHealth = other.GetComponentInParent<Health> ();
		if (otherHealth != null) {
			if (otherHealth.state == Health.State.Alive) {
				otherHealth.Die ();
				if (otherHealth.type == Health.Type.Player) {
					Destroy (other.GetComponentInParent<Rigidbody> ());
				}
			}
		} else {
			Destroy (other.gameObject);
		}
	}
}
