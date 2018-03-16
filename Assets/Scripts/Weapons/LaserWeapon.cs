using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : Weapon {
	[Header("Laser")]
	public float accuracy;
	public GameObject bulletRayPrefab;

	protected override void Shoot () {
		base.Shoot ();

		foreach (var bulletSpawn in bulletSpawns) {
			Vector3 start = bulletSpawn.position;
			Vector3 end;
			Vector3 direction = Quaternion.Euler(new Vector3 (Random.Range(-accuracy, accuracy), Random.Range(-accuracy, accuracy), 0f)) * bulletSpawn.forward;

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
					hitGO.GetComponentInParent<Health> ().TakeDamage (damage, Health.DamageType.Bullets);
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
