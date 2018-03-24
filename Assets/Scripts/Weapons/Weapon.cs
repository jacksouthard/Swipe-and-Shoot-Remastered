using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
	[Header("Stats")]
	public float fireRate;
	public float damage;
	public float range;
	public float speedMultiplier;

	[Header("Audio")]
	public AudioClip audioClip;
	public float audioPitch = 1;

	AudioSource reloadSound;
	List<Transform> targetsToIgnore = new List<Transform>();

	public float dps {
		get {
			if (bulletSpawns.Count == 0) {
				SetBulletSpawns ();
			}
			return (damage * bulletSpawns.Count) / fireRate;
		}
	}

	string targetTag;

	bool canFire;
	float fireRateTimer = 0f;
	protected List<Transform> bulletSpawns = new List<Transform> ();

	void SetBulletSpawns() {
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).name.Contains("BulletSpawn")) {
				bulletSpawns.Add (transform.GetChild (i));
			}
		}
	}

	void Start () {
		if (bulletSpawns.Count == 0) {
			SetBulletSpawns ();
		}

		reloadSound = GetComponent<AudioSource> ();
	}

	public void SetTarget(string target) {
		targetTag = target;
	}

	public void UpdateIgnoreList() {
		targetsToIgnore.Clear ();
		targetsToIgnore = GetComponentInParent<ShootingController> ().targetsToIgnore;
	}
	
	void Update () {
		if (canFire) {
			if (shouldFire) {
				Shoot ();
			}
		}

		if (!canFire) {
			fireRateTimer -= Time.deltaTime;
			if (fireRateTimer <= 0f) {
				canFire = true;
			}
		}
	}

	bool shouldFire {
		get {
			RaycastHit hitInfo;
			Physics.SphereCast (bulletSpawns [0].position, 0.5f, bulletSpawns[0].forward, out hitInfo, range, ~((1 << 2) | (1 << 11))); //ignore IgnoreRaycast and Projectile layers

			if (hitInfo.collider == null || targetsToIgnore.Contains(hitInfo.collider.transform)) {
				return false;
			}

			if (hitInfo.collider.tag != targetTag) {
				if (targetTag == "Player") {
					Rideable hitVehicle = hitInfo.collider.GetComponentInParent<Rideable> ();
					if (hitVehicle == null || !hitVehicle.shouldBeShotAt || !hitVehicle.driver) {
						return false;
					}
				} else {
					return false;
				}
			}

			Health otherHealth = hitInfo.collider.GetComponentInParent<Health> ();

			return (otherHealth == null || otherHealth.state != Health.State.Decaying);
		}
	}

	protected virtual void Shoot () {
		fireRateTimer = fireRate;
		canFire = false;
		if (reloadSound != null) {
			reloadSound.PlayDelayed(audioClip.length * audioPitch / 2); //wait for a bit
		}
	}
}
