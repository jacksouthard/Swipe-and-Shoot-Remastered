using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscortController : AIController {
	PlayerController pc;

	protected override void Init () {
		pc = GameObject.FindObjectOfType<PlayerController> ();

		base.Init ();

		if (tag != "Player") {
			enabled = false;
		}
	}

	public void Enable() {
		tag = "Player";
		enabled = true;
		GameManager.allEnemyTargets.Add (transform);
	}

	protected override bool IsValidVehicle(GameObject vehicle) {
		if (!base.IsValidVehicle(vehicle)) {
			return false;
		}

		return vehicle.GetComponentInParent<Rideable> ().driver; //only try to get in if the player is already in
	}

	protected override void UpdateTarget () {
		if (pc == null) {
			return;
		}

		base.UpdateTarget ();
		SetTargets (pc.transform);
		if (pc.inVehicle && IsValidVehicle (pc.currentVehicle.gameObject)) {
			navAgent.stoppingDistance = 0f;
		} else {
			navAgent.stoppingDistance = 3f;
		}
	}

	public override void Die() {
		GameManager.allEnemyTargets.Remove (transform);

		base.Die ();
	}
}
