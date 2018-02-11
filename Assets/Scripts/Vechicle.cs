using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vechicle : MonoBehaviour {
	[Header("Turning")]
	public float turnSpeed;
	public float rotationSpeedLimiter = 1f;
	public float rotationSpeedLimitRatio;

	[Header("Speed")]
	public float maxSpeed;
	public float acceleration;
	public float decceleration;

	[Header("Drag")]
	public float groundedDrag;
	public float groundedAngDrag;
	public float airDrag;
	public float airAngDrag;

	[Header("Reversing")]
	public float reverseTime;
	public float stuckThreshold;

	[Space(20)]
	[Header("Debug")]
	public bool grounded = false;
	public Vector2 targetDirection = Vector2.zero;
	public int targetSpeedPercent = 0;
	public int targetRotPercentage = 0;
	public float curWheelSpeed = 0f;

	float reentryWait = 1.0f;
	float nextEnterTime;
	bool driver = false;
	GameObject handsContainer;
	Transform seat;
	GameObject mounter;

	Transform vectorArrow;

	// reversing
	Vector3 lastPos;
	float curReverseTimer;
	bool reversing = false;

	// wheels
	List<Wheel> steeringWheels = new List<Wheel>();
	List<Wheel> drivingWheels = new List<Wheel>();

	Rigidbody rb;

	void Start () {
		rb = GetComponent<Rigidbody> ();

		UpdateDrag ();

		vectorArrow = transform.Find ("TargetVector");
		vectorArrow.gameObject.SetActive (false);

		lastPos = transform.position;

		// init mounting stuff
		seat = transform.Find("Seat");
		handsContainer = transform.Find("Hands").gameObject;
		handsContainer.SetActive (false);

		// init wheels
		Wheel[] allWheels = GetComponentsInChildren<Wheel> ();
		foreach (var wheel in allWheels) {
			if (wheel.name.Contains ("D")) {
				drivingWheels.Add (wheel);
			} else if (wheel.name.Contains ("S")) {
				steeringWheels.Add (wheel);
			}
		}
	}

	public bool canBeMounted { get { return (Time.time >= nextEnterTime); } }

	public void Mount (GameObject _mounter) {
		mounter = _mounter;
		mounter.GetComponent<BoxCollider> ().enabled = false;
		mounter.GetComponent<Rigidbody> ().isKinematic = true;
		mounter.transform.parent = seat;
		mounter.transform.localPosition = Vector3.zero;
		mounter.transform.localRotation = Quaternion.identity;

		handsContainer.SetActive (true);

		driver = true;
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
	}

	public void Dismount () {
		mounter.GetComponent<BoxCollider> ().enabled = true;
		mounter.GetComponent<Rigidbody> ().isKinematic = false;
		mounter.transform.parent = null;

		handsContainer.SetActive (false);

		driver = false;
		nextEnterTime = Time.time + reentryWait;
		rb.interpolation = RigidbodyInterpolation.None;
	}

	void AdaptTargetDirection () {
		if (targetDirection == Vector2.zero) {
			// no input
			targetRotPercentage = 0;
			targetSpeedPercent = 0;
			vectorArrow.gameObject.SetActive (false);
		} else {
			// has input
			if (!reversing) {
				targetSpeedPercent = 1;
			} else {
				targetSpeedPercent = -1;

			}

			// calculate rotation direction
			Vector2 right = new Vector2(transform.right.x, transform.right.z);
			targetRotPercentage = Mathf.RoundToInt(Mathf.Sign (Vector2.Dot(right, targetDirection)));
			// calculate arrow rotation
			float targetAngle = Mathf.Atan2 (-targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
			vectorArrow.rotation = Quaternion.Euler (new Vector3 (0f, targetAngle, 0f));
			vectorArrow.gameObject.SetActive (true);
		}
	}

	void Update () {
		if (!driver) {
			return;
		}

		if (reversing) {
			curReverseTimer -= Time.deltaTime;
			if (curReverseTimer <= 0f) {
				reversing = false;
			}
		}
			
		AdaptTargetDirection();

		if (targetSpeedPercent == 0f) {
			// deccelerate if no input
			if (curWheelSpeed > 0) {
				curWheelSpeed -= decceleration;
			} else if (curWheelSpeed < 0) {
				curWheelSpeed += decceleration;
			}
		}
		if (targetSpeedPercent == 1) {
			// accelerate if forward input
			if (curWheelSpeed >= 0) {
				curWheelSpeed += acceleration;
			} else if (curWheelSpeed < 0) {
				curWheelSpeed += decceleration;
			}
		}
		if (targetSpeedPercent == -1) {
			// accelerate reverse if backward input
			if (curWheelSpeed >= 0) {
				curWheelSpeed -= decceleration;
			} else if (curWheelSpeed < 0) {
				curWheelSpeed -= acceleration;
			}
		}

		curWheelSpeed = Mathf.Clamp (curWheelSpeed, -1f, 1f);
	}

	void FixedUpdate () {
		CheckGrounded ();
		if (grounded) {
			if (!reversing) {
				if (Stuck ()) {
					// is stuck
					Reverse ();
				}
			}

			ApplyRotation ();
			ApplyDrivingWheelForce ();
			ApplySteeringWheelForce ();

			lastPos = transform.position;
		}
	}

	bool Stuck () {
		if (curWheelSpeed >= 0.9f) {
			Vector3 posDiff = transform.position - lastPos;
			if (posDiff.magnitude <= stuckThreshold) {
				return true;
			}
		}
		return false;

	}

	void Reverse () {
		reversing = true;
		curReverseTimer = reverseTime;
	}

	void ApplyRotation () {
		float wheelsRatio = 0f;
		foreach (var wheel in steeringWheels) {
			if (wheel.onGround) {
				wheelsRatio += 1f / steeringWheels.Count;
			}
		}
		rb.AddRelativeTorque (Vector3.up * targetRotPercentage * turnSpeed * Mathf.Abs (curWheelSpeed) * wheelsRatio);
		rotationSpeedLimiter = Mathf.Clamp01(Mathf.Abs(rotationSpeedLimitRatio / rb.angularVelocity.y));
	}

	void ApplyDrivingWheelForce () {
		float wheelsRatio = 0f;
		foreach (var wheel in drivingWheels) {
			if (wheel.onGround) {
				wheelsRatio += 1f / drivingWheels.Count;
			}
		}
		Vector3 force = Vector3.forward * maxSpeed * curWheelSpeed * wheelsRatio * rotationSpeedLimiter;
		rb.AddRelativeForce (force);
	}

	void ApplySteeringWheelForce () {
		float wheelsRatio = 0f;
		foreach (var wheel in steeringWheels) {
			if (wheel.onGround) {
				wheelsRatio += 1f / steeringWheels.Count;
			}
		}
		Vector3 force = Vector3.forward * maxSpeed * curWheelSpeed * wheelsRatio * rotationSpeedLimiter;
		rb.AddRelativeForce (force);
	}

	void CheckGrounded () {
		int wheelsGround = 0;
		foreach (var wheel in drivingWheels) {
			if (wheel.onGround) {
				wheelsGround++;
			}
		}
		foreach (var wheel in steeringWheels) {
			if (wheel.onGround) {
				wheelsGround++;
			}
		}

		if (wheelsGround >= 2) {
			grounded = true;
		} else {
			grounded = false;
		}

		UpdateDrag ();
	}

	void UpdateDrag () {
		if (grounded) {
			rb.drag = groundedDrag;
			rb.angularDrag = groundedAngDrag;
		} else {
			rb.drag = airDrag;
			rb.angularDrag = airAngDrag;
		}
	}

	// detect wheel colisions
	void OnCollisionStay (Collision coll) {
		foreach (var contactPoint in coll.contacts) {
			GameObject myCollGO = contactPoint.thisCollider.gameObject;
			if (myCollGO.GetComponent<Wheel> () != null) {
				myCollGO.GetComponent<Wheel> ().onGround = true;
			}
		}
	}

	void OnCollisionExit (Collision coll) {
		foreach (var wheel in drivingWheels) {
			wheel.onGround = false;
		}
		foreach (var wheel in steeringWheels) {
			wheel.onGround = false;
		}
	}
}
