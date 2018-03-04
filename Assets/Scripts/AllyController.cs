using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyController : AIController {
	public string defaultWeapon = "Random";

	[Header("Control")]
	public bool moves = true;

	ShootingController shooting;
	Transform player;

	protected override void Init() {
		shooting = gameObject.GetComponentInChildren<ShootingController> ();
		player = GameObject.FindObjectOfType<PlayerController> ().transform;

		base.Init ();

		backsUp = moves;
		GameManager.allEnemyTargets.Add (transform);
	}

	void Start() {
		nextPathUpdate = Time.time;

		if (!moves) {
			shooting.canRotateParent = true;
			if (navAgent != null) {
				Destroy (navAgent);
			}
		}

		if (defaultWeapon == "None") {
			return;
		}
		shooting.SetWeapon (WeaponManager.instance.WeaponDataFromName(defaultWeapon));
	}

	protected override void UpdateTarget () {
		base.UpdateTarget ();

		SetTargets ((shooting.target != null) ? shooting.target : player);
	}

	protected override void Activate () {
		if(moves) {
			base.Activate ();
		}

		shooting.enabled = true;
		shooting.canRotateParent = (moves) ? (dist < navAgent.stoppingDistance) : true;
	}

	protected override void Deactivate () {
		if (moves) {
			base.Deactivate ();
		}

		shooting.enabled = false;
		shooting.canRotateParent = false;
	}

	public override void Die() {
		shooting.Die ();
		GameManager.allEnemyTargets.Remove (transform);

		base.Die ();
	}
}
