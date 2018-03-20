using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killzone : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		Health otherHealth = other.GetComponentInParent<Health> ();
		if (otherHealth != null) {
			if (otherHealth.state == Health.State.Alive) {
				otherHealth.Die ();

				PlayerController pc = otherHealth.GetComponent<PlayerController> ();
				if (pc != null) {
					if (!pc.inVehicle) {
						other.GetComponentInParent<Rigidbody> ().isKinematic = true;
					} else {
						pc.currentVehicle.GetComponent<Rigidbody> ().isKinematic = true;
						GameManager.instance.GameOver ("you died");
					}
				}
			}
		} else {
			Destroy (other.gameObject);
		}
	}
}
