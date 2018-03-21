using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
	[Header("Stats")]
	public float fireRate;
	public float damage;
	public float range;
	public float speedMultiplier;

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
	}

	public void SetTarget(string target) {
		targetTag = target;
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

			if (hitInfo.collider == null) {
				return false;
			}

			if (hitInfo.collider.tag != targetTag) {
				if (targetTag == "Player") {
					Vehicle hitVehicle = hitInfo.collider.GetComponentInParent<Vehicle> ();
					if (hitVehicle == null || !hitVehicle.driver) {
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
	}
}
