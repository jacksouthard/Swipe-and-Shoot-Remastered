using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : Weapon {
	[Header("Projectile")]
	public GameObject projectile;
	public float projectileSpeed;
	public float projectileLifeTime;

	protected override void Shoot () {
		base.Shoot ();

		foreach (var bulletSpawn in bulletSpawns) {
			GameObject newProjectile = (GameObject)Instantiate (projectile, bulletSpawn.position, bulletSpawn.rotation);

			Rigidbody projectileRb = newProjectile.GetComponent<Rigidbody> ();
			projectileRb.velocity = newProjectile.transform.forward * projectileSpeed;
			projectileRb.velocity += transform.GetComponentInParent<Rigidbody> ().velocity;

			Destroy (newProjectile, projectileLifeTime);
		}
	}
}
