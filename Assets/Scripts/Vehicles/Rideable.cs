using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rideable : MonoBehaviour {
	[HideInInspector]
	public float hash;

	public bool dismountable;
	public bool controllable;
	public bool isEnemyMountable;

	float reentryWait = 1.0f;
	float nextEnterTime;

	[HideInInspector]
	public bool driver = false;
	GameObject handsContainer;
	Transform seat;

	[Header("Exit")]
	public GameObject exitingVehicle;
	public float spawnDelay;

	protected GameObject mounter;
	protected AudioSource engine;

	public virtual bool shouldBeShotAt { get { return false; } }
	public virtual bool saves { get { return false; } }

	bool isObjective = false;

	[HideInInspector]
	public Rigidbody rb;

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
		seat = transform.Find("Seat");
		handsContainer = transform.Find("Hands").gameObject;
		handsContainer.SetActive (false);
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

	public bool canBeMounted { get { return (Time.time >= nextEnterTime) && !driver && Vector3.Dot(Vector3.up, transform.up) > 0; } }

	public virtual void Mount (GameObject _mounter) {
		// universal
		mounter = _mounter;
		mounter.GetComponent<BoxCollider> ().enabled = false;
		mounter.GetComponent<Rigidbody> ().isKinematic = true;
		mounter.transform.parent = seat;
		mounter.transform.localPosition = Vector3.zero;
		mounter.transform.localRotation = Quaternion.identity;

		handsContainer.SetActive (true);

		driver = true;
		rb.interpolation = RigidbodyInterpolation.Extrapolate;

		// if enemy
		EnemyController em = mounter.GetComponentInParent<EnemyController>();
		if (em != null) {
			em.enabled = false;
		}

		if (isObjective) {
			isObjective = false;
			this.CompleteObjective ();
		}

		if (engine != null) {
			engine.Play ();
			engine.pitch = 0;
		}
	}

	public virtual void Dismount () {
		// universal
		StartCoroutine (SpawnExitVehicle(mounter.transform));

		Collider mounterCol = mounter.GetComponent<Collider> ();
		mounterCol.enabled = true;
		mounter.GetComponent<Rigidbody> ().isKinematic = false;
		mounter.transform.parent = null;
		if (mounter.transform.Find ("WeaponParent") != null) {
			mounter.transform.Find ("WeaponParent").gameObject.SetActive (true);
		}

		handsContainer.SetActive (false);

		driver = false;
		nextEnterTime = Time.time + reentryWait;
		rb.interpolation = RigidbodyInterpolation.None;

		// if enemy
		EnemyController em = mounter.GetComponentInParent<EnemyController>();
		if (em != null) {
			em.EjectFromVehicle ();
			em.enabled = true;
		}

		StartCoroutine(IgnoreDriverCollisions(mounterCol));
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
}
