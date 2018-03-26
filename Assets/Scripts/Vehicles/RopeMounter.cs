using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeMounter : MonoBehaviour {
	public float pullTime;

	LineRenderer lrend;
	Transform seat;
	BoxCollider col;
	Rideable rideable;
	bool wasControllable;
	bool wasDismountable;

	void Awake() {
		lrend = GetComponent<LineRenderer> ();
		lrend.enabled = false;

		seat = transform.Find ("Seat");

		col = GetComponent<BoxCollider> ();
		col.enabled = false;

		rideable = GetComponentInParent<Rideable> ();
		wasControllable = rideable.controllable;
		wasDismountable = rideable.dismountable;
	}

	public IEnumerator Lower() {
		RaycastHit hitInfo;
		float ropeLength;
		Physics.Raycast (transform.position, Vector3.down, out hitInfo, 50f, 1 << 10); //figure out where the ground is
		if (hitInfo.collider == null) {
			ropeLength = 6f; //default rope length
		} else {
			ropeLength = transform.position.y - hitInfo.point.y - 1f; //leave the rope slightly above the ground
		}

		lrend.enabled = true;

		float p = 0f;
		while(p <= 1f) {
			p += Time.deltaTime / pullTime;
			seat.transform.position = Vector3.Lerp (transform.position, transform.position + (Vector3.down * ropeLength), p);
			lrend.SetPosition (1, seat.transform.localPosition);
			yield return null;
		}

		seat.transform.position = transform.position + (Vector3.down * ropeLength);
		lrend.SetPosition (1, seat.transform.localPosition);
		col.size = new Vector3 (lrend.startWidth, ropeLength, lrend.startWidth);
		col.center = new Vector3 (0f, -ropeLength / 2, 0f);
		col.enabled = true;
	}

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") {
			PlayerController otherPc = other.GetComponentInParent<PlayerController> ();
			if (otherPc != null) {
				otherPc.PrepareForVehicle (rideable);
				rideable.Mount (otherPc.gameObject);
				StartCoroutine (Raise ());
			}
		}
	}

	IEnumerator Raise() {
		rideable.controllable = false;
		rideable.dismountable = false;

		Vector3 relativeStartPos = seat.transform.localPosition;
		col.enabled = false;

		float p = 0f;
		while(p <= 1f) {
			p += Time.deltaTime / pullTime;
			seat.transform.position = Vector3.Lerp (transform.position + relativeStartPos, transform.position, p);
			lrend.SetPosition (1, seat.transform.localPosition);
			yield return null;
		}

		seat.transform.position = transform.position;
		lrend.enabled = false;
		rideable.controllable = wasControllable;
		rideable.dismountable = wasDismountable;
		rideable.FinishRope ();
	}
}
