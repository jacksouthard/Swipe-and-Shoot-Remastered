using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleHealth : Health {
	[Header("Vehicle")]
	public Transform smokeCenter;

	static bool prefabsSet = false;
	static GameObject smokeEffectPrefab;
	static GameObject fireEffectPrefab;
	static GameObject explosionEffectPrefab;

	GameObject smokeEffect;
	GameObject fireEffect;

	void Awake() {
		if (!prefabsSet) {
			prefabsSet = true;
			smokeEffectPrefab = Resources.Load ("SmokeEffect") as GameObject;
			fireEffectPrefab = Resources.Load ("FireEffect") as GameObject;
			explosionEffectPrefab = Resources.Load ("Explosion") as GameObject;
		}
	}

	public override void TakeDamage (float damage) {
		base.TakeDamage (damage);

		if (smokeEffect == null) {
			if ((health / maxHealth) <= 0.5f) {
				smokeEffect = (GameObject)Instantiate (smokeEffectPrefab, smokeCenter.position, Quaternion.identity);
				smokeEffect.GetComponent<EffectFollow> ().Init (smokeCenter);
			}
		}

		if (fireEffect == null) {
			if ((health / maxHealth) <= 0.25f) {
				fireEffect = (GameObject)Instantiate (fireEffectPrefab, smokeCenter.position, Quaternion.identity);
				fireEffect.GetComponent<EffectFollow> ().Init (smokeCenter);
			}
		}
	}

	public override void Die() {
		base.Die ();

		if (state != State.Alive) {
			return;
		}

		// test for player in vechicle
		Vehicle vechicle = GetComponent<Vehicle>();
		if (vechicle.driver) {
			GetComponentInChildren<PlayerController> ().ExitVehicle ();
		}

		gameObject.GetComponent<Rigidbody> ().drag = 0;
		GameObject explosion = (GameObject) Instantiate (explosionEffectPrefab, transform.Find("Center").position, Quaternion.identity);
		explosion.GetComponent<Explosion> ().Initiate (5f, 5000f);
		Destroy(vechicle);

		if (fireEffect != null) {
			fireEffect.GetComponent<EffectFollow> ().End ();
		}
		if (smokeEffect != null) {
			smokeEffect.GetComponent<EffectFollow> ().End ();
		}
	}
}
