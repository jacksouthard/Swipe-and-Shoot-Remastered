using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//controls the player character
public class PlayerController : MonoBehaviour {
	public enum MovementState {
		Grounded,
		Jumping,
		Tumbling
	}

	public MovementState state;

	[Header("Control")]
	public float stoppingSpeed; //ground speed before the player returns to standed state
	public float stoppingAngle; //angle difference before the player returns to grounded state
	public float autoResetTime; //amount of time before the player auto resets
	float nextAutoReset;

	[Header("Speed")]
	public float swipeForce; //the amount by which the swiping force is scaled by
	public float verticalFactor; //the amount by which the y-vector of the launch force is scaled by relative to the launch magnitude
	public float turnSpeed; //how fast the player returns to a standing position

	[Header("Weapon")]
	public string defaultWeaponName = "None";
	public string curWeaponName { get { return shooting.curWeaponName; } }

	[Header("Picking Up")]
	public float pickupTime;
	public GameObject timerDisplay;
	public GameObject timerBar;
	public Text nextPickupText;

	List<PickupTimer> curPickingupTimers = new List<PickupTimer>();

	public bool inVehicle { get { return currentVehicle != null; } }

	public Rideable currentVehicle;

	Rigidbody rb;
	ShootingController shooting;
	Health health;

	void Awake() {
		rb = gameObject.GetComponent<Rigidbody> ();
		health = gameObject.GetComponent<Health> ();

		shooting = gameObject.GetComponentInChildren<ShootingController> ();
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		timerDisplay.transform.parent = null; //timer moves independently from player
	}

	void Start() {
		//set starting weapon
		string weaponToUse = (LevelProgressManager.lastWeaponName != "None") ? LevelProgressManager.lastWeaponName : defaultWeaponName;
		if (weaponToUse != "None") {
			shooting.SetWeapon (WeaponManager.instance.WeaponDataFromName (weaponToUse));
		}
	}

	//launches character in a direction
	public void Swipe (Vector2 dir) {
		if (inVehicle) {
			ExitVehicle ();
		}
		rb.constraints = RigidbodyConstraints.None;
		rb.AddForce (new Vector3(dir.x, dir.magnitude * verticalFactor, dir.y) * swipeForce);
		state = MovementState.Jumping;
		nextAutoReset = Time.time + autoResetTime; //so you can't get stuck in jumping state
	}

	void EnterVehicle(Rideable newVehicle) {
		currentVehicle = newVehicle;
		rb.interpolation = RigidbodyInterpolation.None;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		currentVehicle.Mount (gameObject);
		state = MovementState.Grounded;

		shooting.canRotateParent = false;
		shooting.gameObject.SetActive (false);

		curPickingupTimers.Clear ();
	}

	public void ExitVehicle() {
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		rb.velocity = currentVehicle.GetComponent<Rigidbody> ().velocity;

		state = MovementState.Jumping;

		currentVehicle.Dismount ();
		currentVehicle = null;

		shooting.canRotateParent = true;
		shooting.gameObject.SetActive (true);

		health.ResetColor ();
	}

	void OnCollisionEnter(Collision other) {
		if (other.collider.tag == "Vehicle") {
			Rideable newVehicle = other.gameObject.GetComponentInParent<Rideable> ();
			if(newVehicle != null && newVehicle.canBeMounted) {
				EnterVehicle (newVehicle); //enter vehicle when you hit something tagged with vehicle
			}
		} else {
			//changes states when hit
			if (state == MovementState.Jumping) {
				shooting.canRotateParent = false;
				state = MovementState.Tumbling;
			}
		}
	}

	public void Hit() {
		nextAutoReset = Time.time + autoResetTime;
		rb.constraints = RigidbodyConstraints.None;
		shooting.canRotateParent = false;
	}

	//takes enemy weapon if you don't have one
	public bool TrySwapWeapons(WeaponManager.WeaponData weaponData) {
		if (shooting.hasWeapon) {
			return false;
		}

		shooting.SetWeapon (weaponData);
		return true;
	}

	//rotates in direction of swipe unless an enemy is in range
	public void TryRotateInDir(Vector2 dir) {
		if (shooting.targetInRange) {
			return;
		}

		float angle = Mathf.Atan2 (dir.x, dir.y) * Mathf.Rad2Deg;
		shooting.OverrideRotateParent (angle);
	}

	void LateUpdate() {
		if (state == MovementState.Tumbling || (state == MovementState.Jumping && Time.time > nextAutoReset)) {
			Vector2 groundSpeed = new Vector2 (rb.velocity.x, rb.velocity.z);
			if (groundSpeed.magnitude < stoppingSpeed) { //stop when the player is slow enough
				Stop ();
			}
		}

		if (state == MovementState.Grounded && !inVehicle && Time.time > nextAutoReset) { //stand up
			if (!shooting.canRotateParent) {
				Stop ();
			}
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), turnSpeed * Time.deltaTime);
		}

		DisplayPickupTimer ();
	}

	//once player has slowed down enough, reset for next swipe
	void Stop() {
		state = MovementState.Grounded;
		rb.velocity = Vector3.zero;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		shooting.canRotateParent = true;
	}

	public void Die() {
		rb.constraints = RigidbodyConstraints.None;
		shooting.Die ();
		Destroy (this);
	}

	// picking up weapons
	void OnTriggerEnter (Collider coll) {
		if (!inVehicle && coll.gameObject.tag == "Pickup") {
			// new pickup enter range
			curPickingupTimers.Add (new PickupTimer (coll.gameObject, pickupTime));
		}
	}

	void OnTriggerExit (Collider coll) {
		if (!inVehicle && coll.gameObject.tag == "Pickup") {
			// pickup exit range
			List<PickupTimer> exitedTimers = new List<PickupTimer>();
			foreach (var pickupTimer in curPickingupTimers) {
				if (pickupTimer.pickup == coll.gameObject) {
					exitedTimers.Add (pickupTimer);
				}
			}

			// remove exited timers
			foreach (var exitedTimer in exitedTimers) {
				curPickingupTimers.Remove (exitedTimer);
			}
		}
	}

	void Update () {
		if (curPickingupTimers.Count == 0) {
			return;
		}

		foreach (var pickupTimer in curPickingupTimers) {
			pickupTimer.timer -= Time.deltaTime;
			if (pickupTimer.timer <= 0f) { //if a timer has run out, pick it up
				Pickup (pickupTimer);
				curPickingupTimers.Remove (pickupTimer);
				return;
			}
		}
	}

	//picks up object associated with this timer
	void Pickup (PickupTimer timer) {
		foreach (var pickupTimer in curPickingupTimers) {
			if (pickupTimer != timer) {
				pickupTimer.ResetTimer ();
			}
		}

		if (timer.type == PickupTimer.Type.Weapon) {
			WeaponPickup weaponPickup = timer.pickup.GetComponent<WeaponPickup> ();
			shooting.SetWeapon (weaponPickup.weaponData);
			Destroy (timer.pickup);
		} else if (timer.type == PickupTimer.Type.Objective) {
			LevelProgressManager.instance.CompleteLevel ();
			Destroy (timer.pickup.GetComponent<Collider> ());
		}
	}

	//display timer information
	void DisplayPickupTimer() {
		timerDisplay.SetActive (curPickingupTimers.Count > 0);

		if (curPickingupTimers.Count == 0) {
			return;
		}

		timerDisplay.transform.position = transform.position;

		nextPickupText.text = curPickingupTimers [0].name;
		timerBar.transform.localScale = new Vector3 (1f - curPickingupTimers[0].percentage, 1f, 1f);
	}

	public class PickupTimer {
		public enum Type {
			Weapon,
			Objective
		};

		public Type type;

		public GameObject pickup;
		public float timer;
		public string name;

		float originalTime;

		public PickupTimer (GameObject _pickup, float _timer) {
			pickup = _pickup;
			timer = _timer;
			originalTime = _timer;

			DetermineType();
		}

		//setup timer based on object
		void DetermineType() {
			WeaponPickup weaponPickup = pickup.GetComponent<WeaponPickup> ();
			if (weaponPickup != null) {
				name = weaponPickup.weaponData.name;
				type = Type.Weapon;
				return;
			}

			//if we add costume pickups, check for it here
			name = pickup.name;
			type = Type.Objective;
		}

		public void ResetTimer() {
			timer = originalTime;
		}

		public float percentage { get { return timer / originalTime; } }
	}
}
