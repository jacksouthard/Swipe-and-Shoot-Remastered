using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {
	public string defaultWeapon = "Random";

	[Header("Control")]
	public bool moves = true;
	public float activeRange;
	public float pathUpdateRate;
	public float durability; //max force before the enemy dies

	float nextPathUpdate;
	Transform player;
	NavMeshAgent navAgent;
	ShootingController shooting;

	void Awake() {
		player = GameObject.FindObjectOfType<PlayerController> ().transform;
		navAgent = gameObject.GetComponent<NavMeshAgent> ();
		shooting = gameObject.GetComponentInChildren<ShootingController> ();
		//adjust speed here (stopping distance, movement speed, angular speed, etc.)
	}

	void Start() {
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
	}

	void LateUpdate() {
		if (!moves) {
			return;
		}

		if (Time.time > nextPathUpdate) {
			UpdateTarget ();	
		}

		if (Vector3.Distance (transform.position, player.position) < navAgent.stoppingDistance - 0.5f) { //give a little room for error
			transform.position += (transform.position - player.position).normalized * navAgent.speed * Time.deltaTime / 2; //back up slower than they move normally
		}
	}

	void UpdateTarget() {
		float dist = Vector3.Distance (transform.position, player.position);
		if (dist < activeRange) {
			navAgent.enabled = true;
			navAgent.SetDestination (player.position);

			shooting.canRotateParent = (dist < navAgent.stoppingDistance);
		} else {
			navAgent.enabled = false;

			shooting.canRotateParent = false;
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
			gameObject.GetComponent<Health> ().Die ();
		}
	}

	public void Die() {
		gameObject.GetComponent<Rigidbody> ().isKinematic = false;

		if (moves) {
			Destroy (navAgent);
		}

		shooting.Die ();
		// notify spawner of death

		if (Spawner.instance != null) {
			Spawner.instance.EnemyDeath ();
		}

		Destroy(this);
	}
}
