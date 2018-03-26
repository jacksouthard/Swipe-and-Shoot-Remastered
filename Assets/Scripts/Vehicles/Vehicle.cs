using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Vehicle : Rideable {
	[Header("Turning")]
	public float turnSpeed;
	public float rotationSpeedLimiter = 1f;

	[Header("Speed")]
	public float maxSpeed;
	public float acceleration;
	public float decceleration;

	[Header("Drag")]
	public float groundedDrag;
	public float groundedAngDrag;
	public float airDrag;
	public float airAngDrag;

	[Header("Audio")]
	public float averageEnginePitch;

	[Space(20)]
	[Header("Debug")]
	public bool grounded = false;
	public Vector2 targetDirection = Vector2.zero;
	public int targetSpeedPercent = 0;
	public int targetRotPercentage = 0;
	public float curWheelSpeed = 0f;

	public override bool shouldBeShotAt { get { return true; } }

	Transform vectorArrow;
	NavMeshObstacle obstacle;

	// reversing
	int reverseMutliplier = 1;

	// wheels
	List<Wheel> steeringWheels = new List<Wheel>();
	List<Wheel> drivingWheels = new List<Wheel>();

	void Awake () {
		base.Initiate ();
	}

	void Start () {
		UpdateDrag ();

		vectorArrow = transform.Find ("TargetVector");
		vectorArrow.gameObject.SetActive (false);

		// init wheels
		Wheel[] allWheels = GetComponentsInChildren<Wheel> ();
		foreach (var wheel in allWheels) {
			if (wheel.name.Contains ("D")) {
				drivingWheels.Add (wheel);
			} else if (wheel.name.Contains ("S")) {
				steeringWheels.Add (wheel);
			}
		}

		obstacle = GetComponent<NavMeshObstacle> ();
	}
		
	public override void Mount (GameObject _mounter) {
		base.Mount (_mounter);
		obstacle.enabled = false;
	}

	public override void Dismount () {
		if (dismountable) {
			obstacle.enabled = true;

			base.Dismount ();
			vectorArrow.gameObject.SetActive (false);
		}
	}

	void AdaptTargetDirection () {
		if (targetDirection == Vector2.zero) {
			// no input
			targetRotPercentage = 0;
			targetSpeedPercent = 0;
			vectorArrow.gameObject.SetActive (false);
		} else {
			// calculate rotation direction
			Vector2 right = new Vector2(transform.right.x, transform.right.z);
			targetRotPercentage = Mathf.RoundToInt(Mathf.Sign (Vector2.Dot(right, targetDirection)));
			float targetAngle = Mathf.Atan2 (-targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

			// calculate arrow rotation
			float localArrowAngle = targetAngle - transform.eulerAngles.y;
			vectorArrow.localRotation = Quaternion.Euler (new Vector3 (0f, localArrowAngle, 0f));
			vectorArrow.gameObject.SetActive (true);

			// calculate weather to reverse or not
			float vehicleAngle = transform.eulerAngles.y;
			vehicleAngle = ((vehicleAngle + 90f) % 360f) - 180f;

			Vector2 forward = new Vector2 (transform.forward.x, transform.forward.z);
			int multiplier = Mathf.RoundToInt(Mathf.Sign (Vector2.Dot(forward, targetDirection)));
			reverseMutliplier = multiplier;
			targetSpeedPercent = multiplier;

			float angleDiff = targetAngle - vehicleAngle;
			float ZeroTo90Angle = Mathf.Abs(90f - Mathf.Abs (angleDiff)) / 90f;
			rotationSpeedLimiter = Mathf.Clamp (ZeroTo90Angle, 0.1f, 1f);
		}
	}

	void Update () {
		if (engine != null) {
			float minPitch = (driver) ? (averageEnginePitch - 0.5f) : 0f;
			float maxPitch = averageEnginePitch + 0.5f;
			float goalPitch = Mathf.Clamp01(Mathf.Abs(curWheelSpeed)) * (maxPitch - minPitch) + minPitch;
			engine.pitch = Mathf.Lerp(engine.pitch, goalPitch, Time.deltaTime * 2); //arbitrary min/max values

			if (!driver && engine.pitch <= 0.1f) {
				engine.Stop ();
			}
		}

		if (!driver) {
			if (curWheelSpeed > 0) {
				curWheelSpeed -= decceleration;
			}
			return;
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
			ApplyRotation ();
			ApplyDrivingWheelForce ();
			ApplySteeringWheelForce ();
		}
	}

	void ApplyRotation () {
		float wheelsRatio = 0f;
		foreach (var wheel in steeringWheels) {
			if (wheel.onGround) {
				wheelsRatio += 1f / steeringWheels.Count;
			}
		}
		rb.AddRelativeTorque (Vector3.up * targetRotPercentage * turnSpeed * Mathf.Abs (curWheelSpeed) * wheelsRatio * reverseMutliplier);
//		rotationSpeedLimiter = Mathf.Clamp01(Mathf.Abs(rotationSpeedLimitRatio / rb.angularVelocity.y));
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

	protected override void Die() {
		gameObject.GetComponent<Rigidbody> ().drag = 0;
		base.Die ();
	}
}
