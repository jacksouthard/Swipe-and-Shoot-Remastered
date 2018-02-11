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

	[Header("Speed")]
	public float swipeForce; //the amount by which the swiping force is scaled by
	public float verticalFactor; //the amount by which the y-vector of the launch force is scaled by relative to the launch magnitude
	public float turnSpeed; //how fast the player returns to a standing position

	Vechicle currentVechicle;

	Rigidbody rb;
	ShootingController shooting;

	void Awake() {
		rb = gameObject.GetComponent<Rigidbody> ();
		shooting = gameObject.GetComponentInChildren<ShootingController> ();
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
	}

	//launches character in a direction
	public void Swipe(Vector2 dir) {
		if (currentVechicle != null) {
			rb.interpolation = RigidbodyInterpolation.Extrapolate;
			currentVechicle.Dismount ();
			currentVechicle = null;
		}
		rb.constraints = RigidbodyConstraints.None;
		rb.AddForce (new Vector3(dir.x, dir.magnitude * verticalFactor, dir.y) * swipeForce);
		state = MovementState.Jumping;
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
		if (state == MovementState.Tumbling) {
			Vector2 groundSpeed = new Vector2 (rb.velocity.x, rb.velocity.z);
			if (groundSpeed.magnitude < stoppingSpeed) { //stop when the player is slow enough
				Stop ();
			}
		}

		if (state == MovementState.Grounded && currentVechicle == null) { //stand up
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
			WeaponPickup weaponPickup = coll.gameObject.GetComponent<WeaponPickup> ();
			shooting.SetWeapon (weaponPickup.weaponData);
			Destroy (coll.gameObject);
		}
	}
}
