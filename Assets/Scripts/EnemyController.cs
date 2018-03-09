using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : AIController {
	public string defaultWeapon = "Random";
	public bool alerted;
	public float alertTime;
	public float alertRange;
	public float alertFactor;

	[Header("Control")]
	public bool moves = true;
	public float durability; //max force before the enemy dies

	ShootingController shooting;
	float alertTimer;
	float originalActiveRange;
	EffectFollow currentVisualEffect;

	protected override void Init() {
		shooting = gameObject.GetComponentInChildren<ShootingController> ();

		prioritizesFirstTarget = true;

		base.Init ();

		backsUp = moves;
		originalActiveRange = activeRange;
		health.onHit = TriggerAlert;
	}

	void Start() {
		rb = GetComponent<Rigidbody> ();
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

		if(moves) {
			navAgent.stoppingDistance = shooting.range - 4;
		}
	}

	void Update() {
		if (alerted) {
			alertTimer -= Time.deltaTime;
			if (alertTimer <= 0) {
				Unalert ();
			}
		}
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

		if (dist < shooting.range) {
			SwitchEffects ("SightedEffect");
		} else {
			SwitchEffects ("AlertedEffect");
		}
	}

	protected override void Deactivate () {
		if (moves) {
			base.Deactivate ();
		}

		shooting.enabled = false;
		shooting.canRotateParent = false;

		if (currentVisualEffect != null) {
			RemoveEffect ();
		}
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

	public void TriggerAlert() {
		if (alerted) {
			return;
		}

		Alert ();
		Collider[] enemiesInRange = Physics.OverlapSphere (transform.position, alertRange, 1 << 8);
		foreach (Collider enemy in enemiesInRange) {
			EnemyController controller = enemy.GetComponentInParent<EnemyController> ();
			if (controller != null) {
				controller.Alert ();
			}
		}
	}

	public void Alert() {
		alerted = true;
		alertTimer = alertTime;

		if (health.state == Health.State.Alive) {
			activeRange = originalActiveRange + alertFactor;
		}
	}

	public void Unalert() {
		alerted = false;
		activeRange = originalActiveRange;
		if (currentVisualEffect.name == "AlertedEffect") {
			RemoveEffect ();
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

		if (currentVisualEffect != null) {
			RemoveEffect ();
		}
		// notify spawner of death

		if (Spawner.instance != null) {
			Spawner.instance.EnemyDeath ();
		}

		base.Die ();
	}

	void SwitchEffects(string effectName) {
		if (currentVisualEffect != null) {
			currentVisualEffect.End ();
		}
		currentVisualEffect = EffectFollow.Create (effectName, transform);
	}

	void RemoveEffect() {
		currentVisualEffect.End ();
		currentVisualEffect = null;
	}
}
