using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeMounter : MonoBehaviour {
	public float speed;

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
		Rigidbody parentRb = GetComponentInParent<Rigidbody> ();
		lrend.enabled = true;

		float groundHeight = 0f;
		do {
			RaycastHit hitInfo;
			Physics.Raycast (transform.position, Vector3.down, out hitInfo, 50f, 1 << 10); //figure out where the ground is

			groundHeight = (hitInfo.collider != null) ? hitInfo.point.y : 0f;

			seat.position += Vector3.down * speed * Time.deltaTime;
			lrend.SetPosition (1, seat.transform.localPosition);
			yield return null;
		} while(parentRb.velocity.magnitude > 0.5f || seat.transform.position.y > groundHeight);

		float ropeLength = transform.position.y - seat.position.y;

		seat.transform.position = transform.position + (Vector3.down * ropeLength);
		lrend.SetPosition (1, seat.transform.localPosition);
		col.size = new Vector3 (1f, ropeLength, 1f);
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

		while(seat.position.y < transform.position.y) {
			seat.position += Vector3.up * speed * Time.deltaTime;
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
