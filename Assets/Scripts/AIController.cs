using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour {
	public float hash; //unique id for each AI object

	public float activeRange;
	public float pathUpdateRate;

	public bool backsUp = true;

	protected bool prioritizesFirstTarget; //use this for when you want to prioritize the player
	protected float priorityRange; //range within which the priority target takes priority
	protected float nextPathUpdate;
	protected NavMeshAgent navAgent;
	protected float dist;
	protected Health health;

	protected Transform target;

	public virtual string curWeaponName { get { return "None"; } }

	void Awake() {
		LoadFromCheckpoint ();
	}
		
	void LoadFromCheckpoint() {
		hash = LevelProgressManager.CalculateHash (transform.position);

		if (LevelProgressManager.curObjectiveId > 0) {
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
		health = gameObject.GetComponent <Health> ();
		health.onDeath += Die;

		UpdateTarget ();

		//adjust speed here (stopping distance, movement speed, angular speed, etc.)
	}

	void LateUpdate() {
		if (Time.time > nextPathUpdate) {
			UpdateTarget ();	
		}
			
		if (target == null) {
			return;
		}

		if (backsUp && Vector3.Distance (transform.position, target.position) < navAgent.stoppingDistance - 0.5f) { //give a little room for error
			transform.position += (transform.position - target.position).normalized * navAgent.speed * Time.deltaTime / 2; //back up slower than they move normally
		}
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
		gameObject.GetComponent<Rigidbody> ().isKinematic = false;

		if (navAgent != null) {
			Destroy (navAgent);
		}

		if (LevelProgressManager.instance != null) {
			LevelProgressManager.instance.EnemyDeath (hash);
		}

		Destroy(this);
	}
}
