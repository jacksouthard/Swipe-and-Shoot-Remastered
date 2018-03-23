using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Rideable {
	public bool onlyShootsVehicles;

	ShootingController shooting;

	void Start() {
		shooting = GetComponentInChildren<ShootingController> ();
		shooting.SetEnabled (false);

		GetComponent<Health> ().onDeath += Die;
	}

	void Die() {
		Dismount ();
		GetComponent<Rigidbody> ().isKinematic = false;
		Destroy(GetComponent<Collider> ());
	}

	public override void Mount (GameObject _mounter) {
		base.Mount (_mounter);

		if (mounter.GetComponent<EnemyController> ()) {
			shooting.UpdateTargetTag ((!onlyShootsVehicles) ? "Player" : "Vehicle");
			tag = "Enemy";
		} else {
			shooting.UpdateTargetTag ("Enemy");
		}
		shooting.SetEnabled (true);
	}

	public override void Dismount () {
		base.Dismount ();
		shooting.SetEnabled(false);
	}
}
