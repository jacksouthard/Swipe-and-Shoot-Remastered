using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {
	[Header("Control")]
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
		gameObject.GetComponentInChildren<ShootingController> ().SetWeapon (WeaponManager.instance.GetRandomData ());
	}

	void LateUpdate() {
		if (Time.time > nextPathUpdate) {
			UpdateTarget ();	
		}
	}

	void UpdateTarget() {
		float dist = Vector3.Distance (transform.position, player.position);
		if (dist < activeRange) {
			navAgent.enabled = true;
			navAgent.SetDestination (player.position);

			shooting.canRotate = (dist < navAgent.stoppingDistance);
		} else {
			navAgent.enabled = false;

			shooting.canRotate = false;
		}
	}

	void OnTriggerEnter(Collider other) {
		Rigidbody otherRb = other.gameObject.GetComponentInParent<Rigidbody> ();
		if (otherRb == null) {
			return;
		}

		float appliedForce = otherRb.mass * (otherRb.velocity.magnitude / Time.deltaTime);

		if (appliedForce >= durability) {
			gameObject.GetComponent<Health> ().Die ();
		}
	}
}
