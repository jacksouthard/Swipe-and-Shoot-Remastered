using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : Rideable {
	public float hash;

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

	[Header("Reversing")]
	public float reverseEngageAngle = 90f;

	[Header("Exit")]
	public GameObject exitingVehicle;
	public float spawnDelay;

	[Space(20)]
	[Header("Debug")]
	public bool grounded = false;
	public Vector2 targetDirection = Vector2.zero;
	public int targetSpeedPercent = 0;
	public int targetRotPercentage = 0;
	public float curWheelSpeed = 0f;

	Health health;

	Transform vectorArrow;

	// reversing
	int reverseMutliplier = 1;

	// wheels
	List<Wheel> steeringWheels = new List<Wheel>();
	List<Wheel> drivingWheels = new List<Wheel>();

	void Awake () {
		base.Initiate ();
	}

	void LoadFromCheckpoint() {
		hash = LevelProgressManager.CalculateHash (transform.position);

		if (LevelProgressManager.curObjectiveId > 0) {
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

	void Start () {
		health = GetComponent<Health> ();

		LoadFromCheckpoint ();

		health.onDeath += Die;

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
	}
		
	public override void Mount (GameObject _mounter) {
		base.Mount (_mounter);
		health.UpdateRenderersNextFrame ();
	}

	public override void Dismount () {
		if (dismountable) {
			StartCoroutine (SpawnExitVehicle(mounter.transform));

			base.Dismount ();
			vectorArrow.gameObject.SetActive (false);

			health.UpdateRenderersNextFrame ();
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

			float angleDiff = targetAngle - vehicleAngle;

			if (Mathf.Abs (angleDiff) > reverseEngageAngle) {
				// enter reverse mode
				reverseMutliplier = -1;
				targetSpeedPercent = -1;
			} else {
				reverseMutliplier = 1;
				targetSpeedPercent = 1;
			}

			float ZeroTo90Angle = Mathf.Abs(90f - Mathf.Abs (angleDiff)) / 90f;
			rotationSpeedLimiter = Mathf.Clamp (ZeroTo90Angle, 0.1f, 1f);
		}
	}

	void Update () {
		if (!driver) {
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

	public void Die() {
		// test for player in vechicle
		if (driver) {
			if (GetComponentInChildren<PlayerController> () != null) {
				GetComponentInChildren<PlayerController> ().ExitVehicle ();
			} else if (GetComponentInChildren<EnemyController> () != null) {
				GetComponentInChildren<EnemyController> ().EjectFromVehicle ();
			}
		}

		gameObject.GetComponent<Rigidbody> ().drag = 0;

		Destroy(this);
	}
}
