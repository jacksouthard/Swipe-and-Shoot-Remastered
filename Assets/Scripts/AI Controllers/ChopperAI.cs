using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopperAI : MonoBehaviour {
	public float hoverDistance;
	public float targetUpdateRate;
	public float leashLength;
	public float distToSlowDown;

	Transform player;
	public Transform target;
	Helicopter heli;
	Vector2 targetPos;
	Vector2 pos2d;
	ShootingController shooting;
	bool active;
	bool objectiveTarget = false;
	public bool awaitingPlayer = false;
	public List<TargetData> quedTargets = new List<TargetData> ();
	float nextTargetUpdateTime;

	void Start () {
		heli = GetComponent<Helicopter> ();
		player = GameObject.FindObjectOfType<PlayerController> ().transform;
		shooting = GetComponentInChildren<ShootingController> ();

		UpdateTargetsQue ();

		AIStart ();
	}

	void UpdateTargetsQue () {
		if (quedTargets.Count != 0) {
			objectiveTarget = true;
			target = quedTargets [0].target;
			targetPos = new Vector2 (target.position.x, target.position.z);
		}
	}

	public void AddTarget (Transform target, TargetData.TargetType type) {
		objectiveTarget = true;
		quedTargets.Add (new TargetData (target, type));

		UpdateTargetsQue ();
	}

	public void AddTargetThroughEvent (Transform target) {
		TargetData.TargetType type = TargetData.TargetType.fly;
		if (target.name.Contains ("Extract")) {
			type = TargetData.TargetType.extract;
		} else if (target.name.Contains ("Deploy")) {
			type = TargetData.TargetType.deploy;
		}
		quedTargets.Add (new TargetData (target, type));

		UpdateTargetsQue ();
	}
		
	void CompleteFlyingToTarget () {
		if (quedTargets [0].type == TargetData.TargetType.deploy) {
			heli.Dismount ();
		} else if (quedTargets [0].type == TargetData.TargetType.extract) {
			heli.LowerRope ();
			awaitingPlayer = true;
			return;
		}

		SwitchToNextObjective ();
	}

	void SwitchToNextObjective () {
		quedTargets.RemoveAt (0);

		if (quedTargets.Count == 0) {
			objectiveTarget = false;
		} else {
			target = quedTargets [0].target;
			targetPos = new Vector2 (target.position.x, target.position.z);
		}
	}

	void CompleteExtraction () {
		awaitingPlayer = false;
		heli.aiSpeedMultiplier = 1f;
		SwitchToNextObjective ();
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

		if (!awaitingPlayer) {		
			float dstFromTarget = (targetPos - pos2d).magnitude;
				
			// calculate target direction	
			if (dstFromTarget < 2.5f) {
				if (objectiveTarget) {
					CompleteFlyingToTarget ();
				}
				SetNextTargetPos ();
			}

			heli.targetDirection = CalculateDirectionToTarget ();

			// calculate speed multiplier for exraction missions
			if (quedTargets.Count != 0) {
				if (quedTargets [0].type == TargetData.TargetType.extract) {
					if (dstFromTarget > distToSlowDown) {
						dstFromTarget = distToSlowDown;
					}
					float newSpeed = dstFromTarget / distToSlowDown;
					heli.aiSpeedMultiplier = newSpeed;
				}
			}
		} else {
			heli.targetDirection = Vector2.zero;
		}
	}

	void UpdateTarget() {
		if (!heli.driver && (pos2d - new Vector2 (player.position.x, player.position.z)).magnitude > leashLength) {
			target = player;
		} else {
			target = (shooting.target != null) ? shooting.target : player;
		}
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

	public void PlayerMounted () {
		if (awaitingPlayer) {
			CompleteExtraction ();
		}
	}
}

[System.Serializable]
public class TargetData {
	public Transform target;
	public enum TargetType {
		fly,
		deploy,
		extract
	}
	public TargetType type;

	public TargetData (Transform _target, TargetData.TargetType _type) {
		target = _target;
		type = _type;
	}
}
