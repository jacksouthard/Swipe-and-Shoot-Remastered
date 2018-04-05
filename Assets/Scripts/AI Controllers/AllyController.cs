using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyController : AIController {
	public string defaultWeapon = "Random";

	public override string curWeaponName { get { return shooting.curWeaponName; } }

	[Header("Control")]
	public bool moves = true;

	ShootingController shooting;
	PlayerController pc;

	protected override void Init() {
		shooting = gameObject.GetComponentInChildren<ShootingController> ();
		pc = GameObject.FindObjectOfType<PlayerController> ();

		base.Init ();

		backsUp = moves;
		GameManager.allEnemyTargets.Add (transform);
	}

	protected override void UpdateWeapon(string weaponName) {
		defaultWeapon = weaponName;
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
		shooting.SetWeapon (WeaponManager.instance.GetDataFromName(defaultWeapon));
	}

	protected override void UpdateTarget () {
		if (pc == null) {
			return;
		}

		base.UpdateTarget ();

		SetTargets ((shooting.target != null) ? shooting.target : pc.transform);
		if (moves) {
			if (shooting.target != null) {
				navAgent.stoppingDistance = Mathf.Max (shooting.range - 4, 3);
			} else if (pc.inVehicle && IsValidVehicle (pc.currentVehicle.gameObject)) {
				navAgent.stoppingDistance = 0f;
			} else {
				navAgent.stoppingDistance = 3f;
			}
		}
	}

	protected override bool IsValidVehicle(GameObject vehicle) {
		if (!base.IsValidVehicle(vehicle)) {
			return false;
		}

		return vehicle.GetComponentInParent<Rideable> ().driver; //only try to get in if the player is already in
	}

	protected override void Activate () {
		if(moves) {
			base.Activate ();
		}

		shooting.SetEnabled (true);
		shooting.canRotateParent = (moves) ? (dist < navAgent.stoppingDistance) : true;
	}

	protected override void Deactivate () {
		if (moves) {
			base.Deactivate ();
		}

		shooting.SetEnabled (false);
		shooting.canRotateParent = false;
	}

	public override void Die() {
		shooting.Die ();
		GameManager.allEnemyTargets.Remove (transform);

		base.Die ();
	}

	protected override void EnterVehicle (Rideable newVehicle) {
		base.EnterVehicle (newVehicle);
	}

	public override void EjectFromVehicle (Rigidbody rb, bool forceFallOver = false) {
		base.EjectFromVehicle (rb, forceFallOver);

		shooting.canRotateParent = true;
		shooting.gameObject.SetActive (true);
	}
}
