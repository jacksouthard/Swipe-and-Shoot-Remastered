using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopperAI : MonoBehaviour {
	public float hoverDistance;
	public Transform target;

	Helicopter heli;
	Vector2 targetPos;
	Vector2 pos2d;
	bool active;

	void Start () {
		heli = GetComponent<Helicopter> ();

		AIStart ();
	}

	public void AIStart () {
		if (!heli.flying) {
			heli.EngageFlight ();
		}
		SetNextTargetPos ();
		active = true;
	}

	public void AIStop () {
		active = false;
		heli.targetDirection = Vector2.zero;
	}
	
	void Update () {
		if (!active) {
			return;
		}

		pos2d = new Vector2 (transform.position.x, transform.position.z);
		if ((pos2d - targetPos).magnitude < (hoverDistance / 4f)) {
			SetNextTargetPos ();
		}

		heli.targetDirection = CalculateDirectionToTarget ();
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
