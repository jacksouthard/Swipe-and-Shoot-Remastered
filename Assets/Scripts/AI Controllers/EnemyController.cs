using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : AIController {
	public string defaultWeapon = "Random";
	public override string curWeaponName { get { return shooting.curWeaponName; } }

	public bool alerted;
	public float alertTime;
	public float alertRange;
	public float alertFactor;

	[Header("Control")]
	public bool moves = true;

	ShootingController shooting;
	float alertTimer;
	float originalActiveRange;
	EffectFollow currentVisualEffect;

	AudioSource deathSound;

	protected override void Init() {
		shooting = gameObject.GetComponentInChildren<ShootingController> ();

		prioritizesFirstTarget = true;

		base.Init ();

		backsUp = moves;
		originalActiveRange = activeRange;
		health.onHit = TriggerAlert;
		health.onSwipeDeath = TrySwapWeapons;
	}

	protected override void UpdateWeapon(string weaponName) {
		if (weaponName != "None") { //don't override with an empty weapon if we should have one
			defaultWeapon = weaponName;
		}
	}

	void Start() {
		deathSound = GetComponent<AudioSource> ();
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

		priorityRange = shooting.range;

		if(moves) {
			navAgent.stoppingDistance = Mathf.Max(shooting.range - 4, 3f);
		}
	}

	protected override bool IsValidVehicle(GameObject vehicle) {
		if (vehicle == null) {
			return false;
		}

		Rideable otherRideable = vehicle.GetComponentInParent<Rideable> ();
		return otherRideable != null && otherRideable.canBeMounted && otherRideable.isEnemyMountable && (!otherRideable.driver || otherRideable.tag == "Enemy");
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
		if (moves) {
			navAgent.stoppingDistance = (!IsValidVehicle(target.gameObject)) ? Mathf.Max(shooting.range - 4, 3f) : 0f;
		}
	}

	protected override void Activate () {
		if(moves) {
			base.Activate ();
		}

		shooting.SetEnabled (true);
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

		shooting.SetEnabled (false);
		shooting.canRotateParent = false;

		if (currentVisualEffect != null) {
			RemoveEffect ();
		}
	}

	public void TriggerAlert(float damage) {
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
		if (currentVisualEffect != null && currentVisualEffect.effectName == "AlertedEffect") {
			RemoveEffect ();
		}
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

		if (Spawner.spawners.ContainsKey("EnemySpawner")) {
			Spawner.spawners["EnemySpawner"].SpawnerObjectDespawn ();
		}

		deathSound.clip = AudioManager.instance.GetRandomEnemyDeathSound ();
		deathSound.Play ();

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

	void TrySwapWeapons() {
		if (shooting.hasWeapon && GameObject.FindObjectOfType<PlayerController>().TrySwapWeapons (shooting.GetWeaponData ())) {
			shooting.RemoveWeapon ();
		}
	}

	protected override void EnterVehicle (Rideable newVehicle) {
		base.EnterVehicle (newVehicle);
	}

	public override void EjectFromVehicle (Rigidbody rb) {
		base.EjectFromVehicle (rb);

		shooting.canRotateParent = true;
		shooting.gameObject.SetActive (true);
	}
}
