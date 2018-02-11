using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
	[Header("Stats")]
	public float fireRate;
	public float damage;
	public float range;
	public float accuracy;
	public float speedMultiplier;
	public GameObject bulletRayPrefab;

	string targetTag;

	bool canFire;
	float fireRateTimer = 0f;
	List<Transform> bulletSpawns = new List<Transform> ();

	void Start () {
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).name.Contains("BulletSpawn")) {
				bulletSpawns.Add (transform.GetChild (i));
			}
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
			Physics.SphereCast (bulletSpawns [0].position, 0.5f, -bulletSpawns[0].right, out hitInfo, range);

			if (hitInfo.collider == null) {
				return false;
			}

			if (hitInfo.collider.tag != targetTag) {
				if (targetTag == "Player") {
					Vechicle hitVehicle = hitInfo.collider.GetComponentInParent<Vechicle> ();
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

	void Shoot () {
		fireRateTimer = fireRate;
		canFire = false;

		foreach (var bulletSpawn in bulletSpawns) {
			Vector3 start = bulletSpawn.position;
			Vector3 end;
			Vector3 direction = bulletSpawn.rotation * Vector3.left;
			direction = Quaternion.Euler(new Vector3 (Random.Range(-accuracy, accuracy), Random.Range(-accuracy, accuracy), 0f)) * direction;

			RaycastHit hit;
			Physics.Raycast(start, direction, out hit, range);

			//		if (true) {
			if (hit.collider == null) {
				// bullet misses
				end = bulletSpawn.position + (direction.normalized * range);

			} else {
				// bullet hit
				end = hit.point;

				// apply damage
				GameObject hitGO = hit.collider.gameObject;
				if (hitGO.GetComponentInParent<Health> () != null) {
					hitGO.GetComponentInParent<Health> ().TakeDamage (damage);
				} else if (hitGO.GetComponent<Health> () != null) {
					hitGO.GetComponent<Health> ().TakeDamage (damage);
				}

				// apply bullet force
				if (hitGO.GetComponent<Rigidbody> () != null) {
					// has rigid body
					Vector3 force = direction.normalized * damage * 3000f; // default force multiplier
					hitGO.GetComponent<Rigidbody> ().AddForceAtPosition (force, end);
				}
			}

			GameObject bulletRay = Instantiate (bulletRayPrefab, transform.position, Quaternion.identity, transform);
			bulletRay.GetComponent<BulletRay> ().Init (start, end, 0.05f);
		}
	}
}
