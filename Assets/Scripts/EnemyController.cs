using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : AIController {
	public string defaultWeapon = "Random";

	[Header("Control")]
	public bool moves = true;
	public float durability; //max force before the enemy dies

	ShootingController shooting;

	protected override void Init() {
		shooting = gameObject.GetComponentInChildren<ShootingController> ();

		prioritizesFirstTarget = true;

		base.Init ();

		backsUp = moves;
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

		priorityRange = shooting.range;
	}

	protected override void UpdateTarget () {
		base.UpdateTarget ();
		SetTargets (GameManager.allEnemyTargets);
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

	void OnTriggerEnter(Collider other) {
		Rigidbody otherRb = other.gameObject.GetComponentInParent<Rigidbody> ();
		if (otherRb == null) {
			return;
		}

		float appliedForce = otherRb.mass * (otherRb.velocity.magnitude / Time.deltaTime);

		if (appliedForce >= durability) {
			PlayerController pc = otherRb.GetComponent<PlayerController> ();
			if (pc != null && pc.TrySwapWeapons (shooting.GetWeaponData ())) {
				shooting.RemoveWeapon ();
			}
			health.Die ();
		}
	}

	protected override void SwitchTargets () {
		shooting.OverrideSwitchTargets (target);
	}

	public override void Die() {
		shooting.Die ();
		// notify spawner of death

		if (Spawner.instance != null) {
			Spawner.instance.EnemyDeath ();
		}

		base.Die ();
	}
}
