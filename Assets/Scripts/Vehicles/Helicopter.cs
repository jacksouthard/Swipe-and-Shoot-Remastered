using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : Rideable {
	[Header("Turning")]
	public float turnSpeed;

	[Header("Speed")]
	public float maxSpeed;
	public float acceleration;

	[Header("Hovering")]
	public Transform groundCheckPointsContainer;
	public float hoverHeight;
	public float minHoverHeight;
	public float targetHeight;
	public float curHeight;
	public float upDownSpeed;
	List<Transform> checkPoints = new List<Transform>();

	[Header("Tilting")]
	public float maxTiltAngle;

	[Header("Rotors")]
	public float rotorAcceleration;
	public float maxSpinSpeed;
	public Transform topRotor;
	public Transform tailRotor;

	[Header("AI")]
	public bool AIOverridePlayer;

	[Space(20)]
	[Header("Debug")]
	public bool groundInZone = false;
	public bool hasAI = false;
	public ChopperAI ai;
	public float aiSpeedMultiplier = 1f;
	public bool flying = false;
	public Vector2 targetDirection = Vector2.zero;
	public int targetSpeedPercent = 0;
	public int targetRotPercentage = 0;
	public float curFlySpeed = 0f;
	public float curRotorSpeed = 0f;
	public int targetRotorPercent = 0;

	ShootingController shooting;

	public Transform vectorArrow;

	public override bool shouldBeShotAt { get { return true; } }
	public override bool saves { get { return true; } }

	void Awake () {
		base.Initiate ();
		vectorArrow = transform.Find ("TargetVector");
		vectorArrow.gameObject.SetActive (false);
	}

	void Start () {
		if (GetComponent<ChopperAI> () != null) {
			hasAI = true;
			ai = GetComponent<ChopperAI> ();
		}

		shooting = GetComponentInChildren<ShootingController> ();

		// setup check points
		for (int i = 0; i < groundCheckPointsContainer.childCount; i++) {
			checkPoints.Add (groundCheckPointsContainer.GetChild (i));
		}
	}

	public override void Mount (GameObject _mounter) {
		base.Mount (_mounter);
		if (hasAI) {
			ai.PlayerMounted ();
			if (!AIOverridePlayer) {
				ai.AIStop ();
			}
		} else {
			shooting.SetEnabled (true);
		}
		if (!flying) {
			EngageFlight ();
		}
	}

	public override void Dismount () {
		if (dismountable) {
			base.Dismount ();
			vectorArrow.gameObject.SetActive (false);

			if (!hasAI) {
				shooting.SetEnabled (false);
				DisengageFlight ();
			} else {
				ai.AIStart ();
			}

		}
	}

	public void EngageFlight () {
		rb.useGravity = false;
		flying = true;
		targetRotorPercent = 1;
	}

	public void DisengageFlight () {
		rb.useGravity = true;
		flying = false;
		targetHeight = 0f;
		targetRotorPercent = 0;
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
			if (driver && !AIOverridePlayer) {
				vectorArrow.gameObject.SetActive (true);
			}

			targetSpeedPercent = 1;
		}
	}

	void Update () {
		if (engine != null) {
			engine.pitch = curRotorSpeed;

			if (!driver && engine.pitch <= 0.1f) {
				engine.Stop ();
			}
		}

		if (!driver && !hasAI) {
			return;
		}

		AdaptTargetDirection();

		if (targetSpeedPercent == 0f) {
			// deccelerate if no input
			if (curFlySpeed > 0) {
				curFlySpeed -= acceleration;
			} else if (curFlySpeed < 0) {
				curFlySpeed += acceleration;
			}
		}
		if (targetSpeedPercent == 1) {
			// accelerate if forward input
			if (curFlySpeed >= 0) {
				curFlySpeed += acceleration;
			} else if (curFlySpeed < 0) {
				curFlySpeed += acceleration;
			}
		}

		curFlySpeed = Mathf.Clamp (curFlySpeed, 0f, 1f);
	}

	void FixedUpdate () {
		if (flying) {
			ApplyRotation ();
			ApplyFlyingForce ();
			ApplyHover ();
			transform.rotation = Quaternion.Euler (new Vector3 (0f, transform.rotation.eulerAngles.y, 0f));
		}
		RotateRotors ();

	}

	void ApplyHover () {
		CalculateTargetHeight ();

		if (Mathf.Abs(transform.position.y - targetHeight) < 0.1f) {
			return;
		}
		float heightIncriment = upDownSpeed * curRotorSpeed;
		if (targetHeight < transform.position.y) {
			heightIncriment *= -1;
		}
		float newY = heightIncriment + transform.position.y;
		transform.position = new Vector3 (transform.position.x, newY, transform.position.z);
	}

	void CalculateTargetHeight () {
		float maxHeight = 0;

		foreach (var point in checkPoints) {
			RaycastHit hit;
			Physics.Raycast (point.position, -Vector3.up, out hit);
			if (hit.collider != null) {
				float height = hit.point.y;
				if (height > maxHeight) {
					maxHeight = height;
				}
			}
		}
			
		targetHeight = maxHeight + hoverHeight;
		if (targetHeight < minHoverHeight) {
			targetHeight = minHoverHeight;
		}
	}

	void RotateRotors () {
		if (curRotorSpeed < targetRotorPercent) {
			curRotorSpeed += rotorAcceleration;
		} else if (curRotorSpeed > targetRotorPercent) {
			curRotorSpeed -= rotorAcceleration;
		}
		Mathf.Clamp01 (curRotorSpeed);
		float newRot = topRotor.transform.localEulerAngles.y + maxSpinSpeed * curRotorSpeed;
		topRotor.transform.localRotation = Quaternion.Euler (new Vector3 (-90f, newRot, 0f));
		tailRotor.transform.localRotation = Quaternion.Euler (new Vector3 (newRot, 0f, 0f));
	}

	void ApplyRotation () {
		rb.AddRelativeTorque (Vector3.up * targetRotPercentage * turnSpeed * curFlySpeed);
	}

	void ApplyFlyingForce () {
		Vector3 force = Vector3.forward * maxSpeed * curFlySpeed * curRotorSpeed * aiSpeedMultiplier;
		rb.AddRelativeForce (force);
	}
}
