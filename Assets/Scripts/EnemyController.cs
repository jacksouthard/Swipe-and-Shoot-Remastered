using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {
	[Header("Control")]
	public float activeRange;
	public float pathUpdateRate;

	float nextPathUpdate;
	Transform player;
	NavMeshAgent navAgent;

	void Awake() {
		player = GameObject.FindObjectOfType<PlayerController> ().transform;
		navAgent = gameObject.GetComponent<NavMeshAgent> ();
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
		if (Vector3.Distance (transform.position, player.position) < activeRange) {
			navAgent.enabled = true;
			navAgent.SetDestination (player.position);
		} else {
			navAgent.enabled = false;
		}
	}
}
