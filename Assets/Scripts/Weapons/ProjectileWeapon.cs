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
			//projectile object
			GameObject newProjectile = (GameObject)Instantiate (projectile, bulletSpawn.position, bulletSpawn.rotation);

			Rigidbody projectileRb = newProjectile.GetComponent<Rigidbody> ();
			projectileRb.velocity = newProjectile.transform.forward * projectileSpeed; //shoot forward
			projectileRb.velocity += transform.GetComponentInParent<Rigidbody> ().velocity; //take parent velocity into account

			Projectile projectileScript = newProjectile.GetComponent<Projectile> ();
			projectileScript.Init (audioClip, audioPitch);

			Destroy (newProjectile, projectileLifeTime); //destroy after a certain amount of time
		}
	}
}
