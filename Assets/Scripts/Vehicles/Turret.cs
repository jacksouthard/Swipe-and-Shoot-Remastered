using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Rideable {
	public Transform lockedTarget;
	public bool automatic;

	ShootingController shooting;

	void Start() {
		shooting = GetComponentInChildren<ShootingController> ();
		shooting.SetEnabled (false);

		GetComponent<Health> ().onDeath += Die;

		if (automatic) {
			SetupTarget ();
		}
	}

	void Die() {
		if (mounter != null) {
			Dismount ();
		}
		GetComponent<Rigidbody> ().isKinematic = false;
		Destroy(GetComponent<Collider> ());
	}

	public override void Mount (GameObject _mounter) {
		if (automatic) {
			return;
		}

		base.Mount (_mounter);
		SetupTarget ();
	}

	void SetupTarget() {
		if (lockedTarget != null) {
			shooting.OverrideSwitchTargets (lockedTarget);
			shooting.UpdateTargetTag (lockedTarget.tag);
		} else if(mounter != null) {
			if (mounter.GetComponent<EnemyController> ()) {
				shooting.UpdateTargetTag ("Player");
			} else {
				shooting.UpdateTargetTag ("Enemy");
			}
		}

		if (shooting.targetTag != "Enemy") {
			tag = "Enemy";
		}
		shooting.SetEnabled (true);
	}

	public override void Dismount () {
		base.Dismount ();
		shooting.SetEnabled(false);
	}
}
