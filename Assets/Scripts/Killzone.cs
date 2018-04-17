using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killzone : MonoBehaviour {
	//static GameObject splash;

	const float delay = 0.75f;

	void OnTriggerEnter(Collider other) {
		//if (splash == null) {
		//	splash = Resources.Load ("Splash") as GameObject;
		//}

		Health otherHealth = other.GetComponentInParent<Health> ();
		if (otherHealth != null) {
			if (otherHealth.state == Health.State.Alive) {
				otherHealth.Die ();

				PlayerController pc = otherHealth.GetComponent<PlayerController> ();
				if (pc != null) {
					if (!pc.inVehicle) {
						StartCoroutine (DisableDelayed(other.GetComponentInParent<Rigidbody>()));
					} else {
						StartCoroutine (DisableDelayed(pc.currentVehicle.GetComponent<Rigidbody> ()));
						GameManager.instance.GameOver ("you died");
					}
				}
			}
		} else {
			Destroy (other.gameObject, delay);
		}

		//GameObject newSplash = (GameObject)Instantiate (splash, other.transform.position, Quaternion.identity);
		//Destroy (newSplash, 2f);
	}

	IEnumerator DisableDelayed(Rigidbody rb) {
		yield return new WaitForSeconds (delay);
		rb.isKinematic = true;
	}
}
