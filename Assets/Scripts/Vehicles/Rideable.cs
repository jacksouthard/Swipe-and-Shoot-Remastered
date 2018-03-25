using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rideable : MonoBehaviour {
	[HideInInspector]
	public float hash;

	public bool dismountable;
	public bool controllable;
	public bool isEnemyMountable;
	protected virtual bool isEnemyTargetable { get { return false; } }

	public bool driver { get { return riders[0] != null; } }
	public List<Transform> seats;

	public int remainingSeats { get; private set; }

	float reentryWait = 1.0f;
	float nextEnterTime;

	[Header("Exit")]
	public GameObject exitingVehicle;
	public float spawnDelay;

	protected GameObject[] riders;
	protected AudioSource engine;

	public virtual bool shouldBeShotAt { get { return false; } }
	public virtual bool saves { get { return false; } }

	public int ridersRequiredForObjective = 1; //if this is an objective, how many riders do you need for it to count
	bool isObjective = false;

	[HideInInspector]
	public Rigidbody rb;
	Health health;

	void Awake () {
		Initiate ();
	}

	public void Initiate () {
		if (saves) {
			LoadFromCheckpoint ();
		}

		rb = GetComponent<Rigidbody> ();
		engine = GetComponent<AudioSource> ();

		// init mounting stuff
		for (int i = 0; i < seats.Count; i++) {
			EnableHands (i, false);
		}
		riders = new GameObject[seats.Count];
		remainingSeats = seats.Count;

		if (isEnemyTargetable) {
			GameManager.allEnemyTargets.Add (transform);
		}

		health = GetComponent<Health> ();
		if (health != null) {
			health.onDeath += Die;
		}
	}

	void LoadFromCheckpoint() {
		hash = LevelProgressManager.CalculateHash (transform.position);

		if (LevelProgressManager.hasMadeProgress) {
			if (LevelProgressManager.startingVehicleData.ContainsKey(hash)) {
				SavedVehicle data = LevelProgressManager.startingVehicleData [hash];
				transform.position = data.position;
				transform.rotation = data.rotation;
			} else {
				Destroy (gameObject);
				return;
			}
		}
	}

	public bool canBeMounted { get { return (Time.time >= nextEnterTime) && Vector3.Dot(Vector3.up, transform.up) > 0 && remainingSeats > 0; } }

	public virtual void Mount (GameObject _mounter) {
		int index = 0;
		while(index < riders.Length && riders[index] != null) {
			if (riders [index] == _mounter) {
				return;
			}
			index++;
		}

		if (index == riders.Length) {
			return;
		}

		remainingSeats--;

		if (isEnemyTargetable && remainingSeats == 0) {
			GameManager.allEnemyTargets.Remove (transform);
		}

		// universal
		GameObject newMounter = _mounter;
		newMounter.GetComponent<BoxCollider> ().enabled = false;
		newMounter.GetComponent<Rigidbody> ().isKinematic = true;

		newMounter.transform.parent = seats[index];
		newMounter.transform.localPosition = Vector3.zero;
		newMounter.transform.localRotation = Quaternion.identity;
		riders[index] = newMounter;

		EnableHands (index, true);

		bool riderIsPlayer = newMounter.GetComponentInParent<PlayerController> () != null;
		if (riderIsPlayer) {
			rb.interpolation = RigidbodyInterpolation.Extrapolate;
		}

		AIController ai = newMounter.GetComponentInParent<AIController>();
		if (ai != null) {
			ai.enabled = false;
		}

		if (isObjective && ridersRequiredForObjective == (seats.Count - remainingSeats)) {
			isObjective = false;
			this.CompleteObjective ();
		}

		if (engine != null && index == 0) {
			engine.Play ();
			engine.pitch = 0;
		}

		if (health != null) {
			health.UpdateRenderersNextFrame ();
		}

		ShootingController shooting = newMounter.GetComponentInChildren<ShootingController> ();
		if (shooting != null) {
			shooting.canRotateParent = false;
			if(seats[index].Find("Hands") != null) {
				shooting.gameObject.SetActive (false); //only disable the weapon if the hands will appear somewhere else
			}
		}
	}

	public virtual void Dismount () {
		// universal
		if (health != null) {
			health.ResetColor ();
		}

		for (int i = 0; i < seats.Count; i++) {
			GameObject exitingRider = riders [i];
			if (exitingRider == null) {
				continue;
			}

			riders [i] = null;
			StartCoroutine (SpawnExitVehicle (exitingRider.transform));

			Collider mounterCol = exitingRider.GetComponent<Collider> ();
			mounterCol.enabled = true;
			exitingRider.GetComponent<Rigidbody> ().isKinematic = false;
			exitingRider.transform.parent = null;
			if (exitingRider.transform.Find ("WeaponParent") != null) {
				exitingRider.transform.Find ("WeaponParent").gameObject.SetActive (true);
			}

			EnableHands (i, false);

			nextEnterTime = Time.time + reentryWait;

			bool riderIsPlayer = exitingRider.GetComponentInParent<PlayerController> () != null;
			if (riderIsPlayer) {
				rb.interpolation = RigidbodyInterpolation.None;
			}

			// if enemy
			AIController ai = exitingRider.GetComponentInParent<AIController>();
			if (ai != null) {
				ai.EjectFromVehicle (rb);
				ai.enabled = true;
			}

			StartCoroutine (IgnoreDriverCollisions (mounterCol));
		}

		if (isEnemyTargetable && !GameManager.allEnemyTargets.Contains (transform)) {
			GameManager.allEnemyTargets.Add (transform);
		}

		remainingSeats = seats.Count;

		if (health != null) {
			health.UpdateRenderersNextFrame ();
		}
	}

	public void EnableHands(int index, bool enable) {
		Transform hands = seats[index].Find("Hands");
		if(hands != null) {
			hands.gameObject.SetActive (enable);
		}
	}

	public SavedVehicle GetSavedData() {
		SavedVehicle vehicle = new SavedVehicle ();
		vehicle.position = transform.position;
		vehicle.rotation = transform.rotation;
		//save health?
		return vehicle;
	}

	IEnumerator SpawnExitVehicle(Transform rider) {
		if (exitingVehicle == null) {
			yield break;
		}

		yield return new WaitForSeconds (spawnDelay);

		Instantiate (exitingVehicle, rider.position, Quaternion.Euler(0, rider.rotation.eulerAngles.y, 0));
	}

	IEnumerator IgnoreDriverCollisions(Collider driverCol) {
		Collider mainCollider = gameObject.GetComponentInChildren<BoxCollider> () as Collider;
		if (mainCollider == null || driverCol == null) {
			yield break;
		}
		Physics.IgnoreCollision (mainCollider, driverCol);

		yield return new WaitForSeconds (0.5f);

		if (mainCollider == null || driverCol == null) {
			yield break;
		}
		Physics.IgnoreCollision (mainCollider, driverCol, false);
	}

	protected virtual void CompleteObjective() {
		LevelProgressManager.instance.CompleteObjective ();
	}

	//sets up vehicle as objective
	public void SetupObjective() {
		isObjective = true;
	}

	protected virtual void Die() {
		//test for driver in vehicle
		if (driver) {
			Dismount ();
		}

		if (isEnemyTargetable && GameManager.allEnemyTargets.Contains(transform)) {
			GameManager.allEnemyTargets.Remove (transform);
		}

		Destroy (this);
	}
}
