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
		rb = GetComponent<Rigidbody> ();
		nextPathUpdate = Time.time;

		if (!moves) {
			shooting.canRotateParent = true;
			if (navAgent != null) {
				Destroy (navAgent);
			}
		} else {
			navAgent.stoppingDistance = shooting.range - 4;
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

	// vehicles
	Rideable currentVehicle;
	Rigidbody rb;
	void OnCollisionEnter(Collision other) {
		if (other.collider.tag == "Vehicle") {
			Rideable newVehicle = other.gameObject.GetComponentInParent<Rideable> ();
			if(newVehicle != null && newVehicle.canBeMounted && newVehicle.isEnemyMountable) {
				EnterVehicle (newVehicle); //enter vehicle when you hit something tagged with vehicle
			}
		}
	}

	void EnterVehicle(Rideable newVehicle) {
		currentVehicle = newVehicle;
		rb.interpolation = RigidbodyInterpolation.None;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		currentVehicle.Mount (gameObject);

		shooting.canRotateParent = false;
		shooting.gameObject.SetActive (false);
	}

	public void EjectFromVehicle() {
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		rb.velocity = currentVehicle.GetComponent<Rigidbody> ().velocity;
			
		currentVehicle = null;

		shooting.canRotateParent = true;
		shooting.gameObject.SetActive (true);

		health.ResetColor ();
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
