using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopperAI : MonoBehaviour {
	public float hoverDistance;
	public float targetUpdateRate;

	Transform player;
	public Transform target;
	Helicopter heli;
	Vector2 targetPos;
	Vector2 pos2d;
	ShootingController shooting;
	bool active;
	bool objectiveTarget;
	float nextTargetUpdateTime;

	void Start () {
		heli = GetComponent<Helicopter> ();
		player = GameObject.FindObjectOfType<PlayerController> ().transform;
		shooting = GetComponentInChildren<ShootingController> ();

		if (target != null && !LevelProgressManager.hasMadeProgress) {
			objectiveTarget = true;
			targetPos = new Vector2 (target.position.x, target.position.z);
		} else {
			objectiveTarget = false;
		}

		AIStart ();
	}

	public void AIStart () {
		if (!heli.flying) {
			heli.EngageFlight ();
		}
		active = true;
		heli.vectorArrow.gameObject.SetActive (false);
	}

	public void AIStop () {
		active = false;
		heli.targetDirection = Vector2.zero;
	}
	
	void Update () {
		if (!active) {
			return;
		}

		if (!objectiveTarget) {
			if (Time.time > nextTargetUpdateTime) {
				nextTargetUpdateTime = Time.time + targetUpdateRate;
				UpdateTarget ();
			}
		}

		pos2d = new Vector2 (transform.position.x, transform.position.z);
		if ((pos2d - targetPos).magnitude < (hoverDistance / 4f)) {
			if (objectiveTarget) {
				heli.Dismount ();
				objectiveTarget = false;
			}
			SetNextTargetPos ();
		}

		heli.targetDirection = CalculateDirectionToTarget ();
	}

	void UpdateTarget() {
		target = (shooting.target != null) ? shooting.target : player;
		SetNextTargetPos ();
	}

	Vector2 CalculateDirectionToTarget () {
		Vector2 diff = targetPos - pos2d;
		return diff.normalized;
	}

	void SetNextTargetPos () {
		Vector3 anchorPos3d = target.position;
		Vector2 anchorPos2d = new Vector2 (anchorPos3d.x, anchorPos3d.z);
		Vector2 distanceVector = Random.insideUnitCircle.normalized * hoverDistance;
		targetPos = distanceVector + anchorPos2d;
//		print ("New Pos: " + targetPos);
	}
}
