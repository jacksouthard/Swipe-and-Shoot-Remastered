using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
	bool dead = false;

	public float maxHealth;
	public float regenSpeed;
	public float regenWait;

	[Header("Debug")]
	public float waitTimer;
	public float health;

	void Start () {
		health = maxHealth;
	}
	
	void Update () {
		if (!dead) {
			if (health < maxHealth) {
				if (waitTimer <= 0f) {
					// start regening
					health += regenSpeed;
					if (health > maxHealth) {
						health = maxHealth;
					}
				} else {
					// incriment timer
					waitTimer -= Time.deltaTime;
				}
			}
		}
	}

	public void TakeDamage (float damage) {
		health -= damage;
		if (health <= 0f) {
			Die ();
		}

		waitTimer = regenWait;
	}

	void Die () {
		dead = true;
		if (GetComponent<PlayerController> () != null) {
			// handel player death
			print ("Player Death");
		} else {
			Destroy (gameObject);
		}
	}
}
