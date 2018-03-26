using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeMounter : MonoBehaviour {
	public float pullTime;

	LineRenderer lrend;
	Transform seat;
	Collider col;
	Rideable rideable;
	bool wasControllable;
	bool wasDismountable;

	void Awake() {
		lrend = GetComponent<LineRenderer> ();
		lrend.enabled = false;

		seat = transform.Find ("Seat");

		col = GetComponent<Collider> ();
		col.enabled = false;

		rideable = GetComponentInParent<Rideable> ();
		wasControllable = rideable.controllable;
		wasDismountable = rideable.dismountable;
	}

	public IEnumerator Lower() {
		Vector3 endPos = transform.position + new Vector3 (0f, -6f, 0f); //arbitrary end position
		lrend.enabled = true;

		float p = 0f;
		while(p <= 1f) {
			p += Time.deltaTime / pullTime;
			seat.transform.position = Vector3.Lerp (transform.position, endPos, p);
			lrend.SetPosition (1, seat.transform.localPosition);
			yield return null;
		}

		seat.transform.position = endPos;
		lrend.SetPosition (1, seat.transform.localPosition);
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

		Vector3 startPos = seat.transform.position;
		Vector3 endPos = transform.position;
		col.enabled = false;

		float p = 0f;
		while(p <= 1f) {
			p += Time.deltaTime / pullTime;
			seat.transform.position = Vector3.Lerp (startPos, endPos, p);
			lrend.SetPosition (1, seat.transform.localPosition);
			yield return null;
		}

		seat.transform.position = endPos;
		lrend.enabled = false;
		rideable.controllable = wasControllable;
		rideable.dismountable = wasDismountable;
		rideable.FinishRope ();
	}
}
