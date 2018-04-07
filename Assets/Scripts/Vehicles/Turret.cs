using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Turret : Rideable {
	public Transform lockedTarget;
	public bool automatic;

	protected override bool isEnemyTargetable { get { return isEnemyMountable; } }
	public override bool shouldBeShotAt { get { return true; } }

	ShootingController shooting;
	NavMeshObstacle obstacle;

	void Start() {
		shooting = GetComponentInChildren<ShootingController> ();
		shooting.SetEnabled (false);

		if (automatic) {
			SetupTarget ();
		}

		obstacle = GetComponent<NavMeshObstacle> ();
		obstacle.enabled = false;
	}

	protected override void Die() {
		GetComponent<Rigidbody> ().isKinematic = false;
		Destroy(GetComponent<Collider> ());
		shooting.Die ();

		base.Die ();
	}

	public override void Mount (GameObject _mounter) {
		if (automatic) {
			return;
		}

		base.Mount (_mounter);
		SetupTarget ();

		obstacle.enabled = true;
	}

	void SetupTarget() {
		if (lockedTarget != null) {
			shooting.OverrideSwitchTargets (lockedTarget);
			shooting.UpdateTargetTag (lockedTarget.tag);
		} else if(driver) {
			shooting.shouldUpdateTarget = true;
			if (riders[0].GetComponent<EnemyController> ()) {
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
		obstacle.enabled = false;
	}
}
