﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour {
	public float hash; //unique id for each AI object

	public float activeRange;
	public float pathUpdateRate;

	public bool backsUp = true;
	public bool inVehicle;
	bool fallenOver;
	bool gettingUp;

	public float fallWaitTime;

	protected bool prioritizesFirstTarget; //use this for when you want to prioritize the player
	protected float priorityRange; //range within which the priority target takes priority
	protected float nextPathUpdate;
	protected NavMeshAgent navAgent;
	protected float dist;
	protected Health health;

	protected Transform target;

	public virtual string curWeaponName { get { return "None"; } }

	protected Rigidbody rb;

	void Awake() {
		DifficultyEnabler diff = GetComponent<DifficultyEnabler> ();
		if (diff != null) {
			if (diff.shouldDestroy) {
				Destroy (this);
				return;
			}
		}
		LoadFromCheckpoint ();
	}
		
	void LoadFromCheckpoint() {
		hash = LevelProgressManager.CalculateHash (transform.position);

		if (LevelProgressManager.hasMadeProgress) {
			if (LevelProgressManager.startingAIData.ContainsKey(hash)) {
				SavedAI data = LevelProgressManager.startingAIData [hash];
				transform.position = data.position;
				transform.rotation = Quaternion.Euler (0, data.angle, 0);
				UpdateWeapon (data.weaponName);
			} else if(LevelProgressManager.killedAIs.Contains(hash)) {
				Destroy (gameObject);
				return;
			}
		}

		this.Init ();
	}

	protected virtual void UpdateWeapon(string newWeapon) {}

	protected virtual void Init() {
		navAgent = gameObject.GetComponent<NavMeshAgent> ();

		rb = gameObject.GetComponent<Rigidbody> ();
		rb.constraints = RigidbodyConstraints.FreezeAll;

		health = gameObject.GetComponent <Health> ();
		health.onDeath += Die;
		health.onKnockedOver += FallOver;

		UpdateTarget ();

		//adjust speed here (stopping distance, movement speed, angular speed, etc.)
	}

	void LateUpdate() {
		if (fallenOver) {
			return;
		}

		if (Time.time > nextPathUpdate) {
			UpdateTarget ();	
		}
			
		if (target == null) {
			return;
		}

		if (navAgent == null) {
			return;
		}
			
		if (backsUp && Vector3.Distance (transform.position, target.position) < navAgent.stoppingDistance - 0.5f) { //give a little room for error
			transform.position += (transform.position - target.position).normalized * navAgent.speed * Time.deltaTime / 2; //back up slower than they move normally
		}
	}

	protected virtual bool IsValidVehicle(GameObject vehicle) {
		return vehicle != null && vehicle.tag == "Vehicle" && vehicle.GetComponentInParent<Rideable>().canBeMounted;
	}

	protected virtual void UpdateTarget () {
		nextPathUpdate = Time.time + pathUpdateRate;
	}

	protected void SetTargets(List<Transform> possibleTargets) {
		if (possibleTargets.Count == 0) {
			return;
		}
			
		float closestDistance = Mathf.Infinity;
		Transform closestTarget = null;

		for (int i = 0; i < possibleTargets.Count; i++) {
			float newDistance = Vector3.Distance (transform.position, possibleTargets [i].position);
			if (newDistance < closestDistance) {
				closestDistance = newDistance;
				closestTarget = possibleTargets [i];

				if (i == 0 && prioritizesFirstTarget && closestDistance < priorityRange) {
					break;
				}
			}
		}

		dist = closestDistance;

		if (target != closestTarget) {
			target = closestTarget;
			SwitchTargets ();
		}
			
		if (dist < activeRange) {
			Activate ();
		} else {
			Deactivate ();
		}
	}

	protected void SetTargets(Transform newTarget) {
		dist = Vector3.Distance (transform.position, newTarget.position);
		target = newTarget;

		if (dist < activeRange) {
			Activate ();
		} else {
			Deactivate ();
		}
	}

	protected virtual void Activate() {
		navAgent.enabled = true;
		navAgent.SetDestination (target.position);
	}

	protected virtual void Deactivate() {
		navAgent.enabled = false;
	}

	protected virtual void SwitchTargets () {}

	public virtual void Die() {
		rb.constraints = RigidbodyConstraints.None;

		if (navAgent != null) {
			Destroy (navAgent);
		}

		if (LevelProgressManager.instance != null) {
			LevelProgressManager.instance.EnemyDeath (hash);
		}

		health.onKnockedOver -= FallOver;
		Destroy(this);
	}

	void FallOver() {
		if (inVehicle) {
			return;
		}

		rb.constraints = RigidbodyConstraints.None;
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
		fallenOver = true;
		gettingUp = false;
		if (navAgent != null) {
			navAgent.enabled = false;
		}

		StartCoroutine (GetUp(transform.position.y));
	}

	IEnumerator GetUp(float originalHeight) {
		yield return new WaitForSeconds (fallWaitTime);

		if (inVehicle) {
			fallenOver = false;
			yield break;
		}

		if (navAgent != null) {
			yield return new WaitUntil (() => (navAgent.isOnNavMesh || rb.velocity.magnitude < 0.5f));
		}

		rb.velocity = Vector3.zero;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		gettingUp = true;
		while (gettingUp && Quaternion.Angle(transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f)) > 10f) {
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), Time.deltaTime * 20);
			yield return new WaitForEndOfFrame ();
		}
		rb.constraints = RigidbodyConstraints.FreezeAll;
		rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		float finalHeight = transform.position.y;
		transform.position = new Vector3 (transform.position.x, Mathf.Max(finalHeight, originalHeight), transform.position.z);

		if (navAgent != null && !navAgent.isOnNavMesh) {
			transform.position = new Vector3 (transform.position.x, (transform.position.y == finalHeight) ? originalHeight : finalHeight, transform.position.z);
		}

		transform.rotation = Quaternion.Euler (0f, transform.rotation.eulerAngles.y, 0f);
		gettingUp = false;
		fallenOver = false;
	}

	//vehicle stuff
	void OnCollisionEnter(Collision other) {
		if (IsValidVehicle (other.collider.gameObject)) {
			Rideable newVehicle = other.collider.gameObject.GetComponentInParent<Rideable> ();
			newVehicle.Mount (gameObject);
			EnterVehicle (newVehicle);
		}
	}

	protected virtual void EnterVehicle(Rideable newVehicle) {
		inVehicle = true;

		if (navAgent != null) {
			navAgent.enabled = false;
		}
	}

	public virtual void EjectFromVehicle(Rigidbody vehicleRb, bool forceFallOver = false) {
		rb.velocity = vehicleRb.velocity;
		inVehicle = false;

		if (forceFallOver || navAgent == null || (!navAgent.isOnNavMesh && vehicleRb.velocity.magnitude > 0.5f)) {
			FallOver ();
		} else {
			navAgent.enabled = true;
		}

		health.ResetColor ();
	}
}
