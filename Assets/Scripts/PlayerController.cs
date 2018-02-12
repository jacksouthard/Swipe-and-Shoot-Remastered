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

	[Header("Picking Up")]
	public float pickupTime;
	public GameObject timerDisplay;
	public GameObject timerBar;
	public Text nextPickupText;

	List<PickupTimer> curPickingupTimers = new List<PickupTimer>();

	public bool inVehicle { get { return currentVehicle != null; } }

	public Vehicle currentVehicle;

	Rigidbody rb;
	ShootingController shooting;

	void Awake() {
		rb = gameObject.GetComponent<Rigidbody> ();
		shooting = gameObject.GetComponentInChildren<ShootingController> ();
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		timerDisplay.transform.parent = null;
	}

	//launches character in a direction
	public void Swipe (Vector2 dir) {
		if (inVehicle) {
			ExitVehicle ();
		}
		rb.constraints = RigidbodyConstraints.None;
		rb.AddForce (new Vector3(dir.x, dir.magnitude * verticalFactor, dir.y) * swipeForce);
		state = MovementState.Jumping;
		nextAutoReset = Time.time + autoResetTime;
	}

	void EnterVehicle(Vehicle newVehicle) {
		currentVehicle = newVehicle;
		rb.interpolation = RigidbodyInterpolation.None;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		currentVehicle.Mount (gameObject);
		state = MovementState.Grounded;

		shooting.gameObject.SetActive (false);
	}

	public void ExitVehicle() {
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		rb.velocity = currentVehicle.GetComponent<Rigidbody> ().velocity;

		state = MovementState.Jumping;

		currentVehicle.Dismount ();
		currentVehicle = null;

		shooting.gameObject.SetActive (true);
	}

	void OnCollisionEnter(Collision other) {
		if (other.collider.tag == "Vehicle") {
			Vehicle newVehicle = other.gameObject.GetComponentInParent<Vehicle> ();
			if(newVehicle != null && newVehicle.canBeMounted) {
				EnterVehicle (newVehicle);
			}
		} else {
			//changes states when hit
			if (state == MovementState.Jumping) {
				shooting.canRotate = false;
				state = MovementState.Tumbling;
			}
		}
	}

	public void Hit() {
		nextAutoReset = Time.time + autoResetTime;
		rb.constraints = RigidbodyConstraints.None;
		shooting.canRotate = false;
	}

	public bool TrySwapWeapons(WeaponManager.WeaponData weaponData) {
		if (shooting.GetWeaponData () != null) {
			return false;
		}

		shooting.SetWeapon (weaponData);
		return true;
	}

	void LateUpdate() {
		if (state == MovementState.Tumbling || (state == MovementState.Jumping && Time.time > nextAutoReset)) {
			Vector2 groundSpeed = new Vector2 (rb.velocity.x, rb.velocity.z);
			if (groundSpeed.magnitude < stoppingSpeed) { //stop when the player is slow enough
				Stop ();
			}
		}

		if (state == MovementState.Grounded && !inVehicle && Time.time > nextAutoReset) { //stand up
			if (!shooting.canRotate) {
				Stop ();
			}
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), turnSpeed * Time.deltaTime);
		}

		DisplayPickupTimer ();
	}

	void Stop() {
		state = MovementState.Grounded;
		rb.velocity = Vector3.zero;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		shooting.canRotate = true;
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
			if (pickupTimer.timer <= 0f) {
				Pickup (pickupTimer.pickup);
				curPickingupTimers.Remove (pickupTimer);
				return;
			}
		}
	}

	void Pickup (GameObject pickup) {
		foreach (var pickupTimer in curPickingupTimers) {
			if (pickupTimer.pickup != pickup) {
				pickupTimer.ResetTimer ();
			}
		}

		WeaponPickup weaponPickup = pickup.GetComponent<WeaponPickup> ();
		shooting.SetWeapon (weaponPickup.weaponData);
		Destroy (pickup);
	}

	void DisplayPickupTimer() {
		timerDisplay.SetActive (curPickingupTimers.Count > 0);

		if (curPickingupTimers.Count == 0) {
			return;
		}

		timerDisplay.transform.position = transform.position;

		nextPickupText.text = curPickingupTimers [0].data.name;
		timerBar.transform.localScale = new Vector3 (curPickingupTimers[0].percentage, 1f, 1f);
	}

	public class PickupTimer {
		public GameObject pickup;
		public WeaponManager.WeaponData data;
		public float timer;

		float originalTime;

		public PickupTimer (GameObject _pickup, float _timer) {
			pickup = _pickup;
			timer = _timer;
			originalTime = _timer;

			data = pickup.GetComponent<WeaponPickup>().weaponData;
		}

		public void ResetTimer() {
			timer = originalTime;
		}

		public float percentage { get { return timer / originalTime; } }
	}
}
