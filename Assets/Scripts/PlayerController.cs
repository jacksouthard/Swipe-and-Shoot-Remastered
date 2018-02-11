using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	List<PickupTimer> curPickingupTimers = new List<PickupTimer>();

	public bool inVehicle { get { return currentVechicle != null; } }

	public Vechicle currentVechicle;

	Rigidbody rb;
	ShootingController shooting;

	void Awake() {
		rb = gameObject.GetComponent<Rigidbody> ();
		shooting = gameObject.GetComponentInChildren<ShootingController> ();
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
	}

	//launches character in a direction
	public void Swipe (Vector2 dir) {
		if (inVehicle) {
			rb.interpolation = RigidbodyInterpolation.Extrapolate;
			currentVechicle.Dismount ();
			currentVechicle = null;
		}
		rb.constraints = RigidbodyConstraints.None;
		rb.AddForce (new Vector3(dir.x, dir.magnitude * verticalFactor, dir.y) * swipeForce);
		state = MovementState.Jumping;
		nextAutoReset = Time.time + autoResetTime;
	}

	void OnCollisionEnter(Collision other) {
		if (other.collider.tag == "Vechicle") {
			Vechicle newVechicle = other.gameObject.GetComponentInParent<Vechicle> ();
			if(newVechicle.canBeMounted) {
				currentVechicle = newVechicle;
				rb.interpolation = RigidbodyInterpolation.None;

				currentVechicle.Mount (gameObject);
				state = MovementState.Grounded;
			}
		} else {
			//changes states when hit
			if (state == MovementState.Jumping) {
				shooting.canRotate = false;
				state = MovementState.Tumbling;
			}
		}
	}

	void LateUpdate() {
		if (state == MovementState.Tumbling || (state == MovementState.Jumping && Time.time > nextAutoReset)) {
			Vector2 groundSpeed = new Vector2 (rb.velocity.x, rb.velocity.z);
			if (groundSpeed.magnitude < stoppingSpeed) { //stop when the player is slow enough
				Stop ();
			}
		}

		if (state == MovementState.Grounded && !inVehicle) { //stand up
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), turnSpeed * Time.deltaTime);
		}
	}

	void Stop() {
		state = MovementState.Grounded;
		rb.velocity = Vector3.zero;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		shooting.canRotate = true;
	}

	// picking up weapons
	void OnTriggerEnter (Collider coll) {
		if (coll.gameObject.tag == "Pickup") {
			// new pickup enter range
			curPickingupTimers.Add (new PickupTimer (coll.gameObject, pickupTime));
		}
	}

	void OnTriggerExit (Collider coll) {
		if (coll.gameObject.tag == "Pickup") {
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

		List<PickupTimer> completedTimers = new List<PickupTimer>();
		foreach (var pickupTimer in curPickingupTimers) {
			pickupTimer.timer -= Time.deltaTime;
			if (pickupTimer.timer <= 0f) {
				Pickup (pickupTimer.pickup);
				completedTimers.Add (pickupTimer);
			}
		}

		// remove completed timers
		foreach (var completedTimer in completedTimers) {
			curPickingupTimers.Remove (completedTimer);
		}
	}

	void Pickup (GameObject pickup) {
		WeaponPickup weaponPickup = pickup.GetComponent<WeaponPickup> ();
		shooting.SetWeapon (weaponPickup.weaponData);
		Destroy (pickup);
	}

	public class PickupTimer {
		public GameObject pickup;
		public float timer;

		public PickupTimer (GameObject _pickup, float _timer) {
			pickup = _pickup;
			timer = _timer;
		}
	}
}
