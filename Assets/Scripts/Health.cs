using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour {
	public float maxHealth;
	public float regenSpeed;
	public float regenWait;
	public float dyingTimer;
	public float decayTimer;

	public enum State
	{
		Alive,
		Dying,
		Decaying
	};
	public State state = State.Alive;
	public float waitTimer;
	public float health;

	void Start () {
		health = maxHealth;
	}
	
	void Update () {
		if (state == State.Alive) {
			if (health < maxHealth) {
				if (waitTimer <= 0f) {
					// start regening
					health += regenSpeed * Time.deltaTime;
					if (health > maxHealth) {
						health = maxHealth;
					}
				} else {
					// incriment timer
					waitTimer -= Time.deltaTime;
				}
			}
		}

		if (state == State.Dying) {
			dyingTimer -= Time.deltaTime;
			if (dyingTimer <= 0f) {
				Decay ();
			}
		}

		if (state == State.Decaying) {
			decayTimer -= Time.deltaTime;
			if (decayTimer <= 0f) {
				Destroy (gameObject);
			}
		}
	}

	public void TakeDamage (float damage) {
		if (state == State.Alive) {
			health -= damage;
			if (health <= 0f) {
				Die ();
			}
		}

		waitTimer = regenWait;
	}

	void Die () {
		state = State.Dying;
		if (GetComponent<PlayerController> () != null) {
			// handel player death
			print ("Player Death");
			return;
		}

		// if enemy
		if (GetComponent<EnemyController>() != null) {
			Rigidbody rb = gameObject.AddComponent<Rigidbody> ();
			rb.mass = 80f; // 80 kg of enemy wieght

			GetComponent<NavMeshAgent> ().enabled = false;
			GetComponent<EnemyController> ().enabled = false;
			GetComponentInChildren<ShootingController> ().enabled = false;
		}
	}

	void Decay () {
		state = State.Decaying;

		// disable colliders
		if (GetComponent<Collider> () != null) {
			GetComponent<Collider> ().enabled = false;
		}

		// children colliders
		Collider[] childColls = GetComponentsInChildren<Collider>();
		foreach (var coll in childColls) {
			coll.enabled = false;
		}

		// set color to black
		MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
		foreach (var mr in mrs) {
			foreach (var mat in mr.materials) {
				mat.color = Color.black;
			}
		}
	}
}
