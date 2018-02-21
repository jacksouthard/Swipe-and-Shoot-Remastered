using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rideable : MonoBehaviour {
	public bool dismountable;
	public bool controllable;

	float reentryWait = 1.0f;
	float nextEnterTime;

	[HideInInspector]
	public bool driver = false;
	GameObject handsContainer;
	Transform seat;
	GameObject mounter;

	[HideInInspector]
	public Rigidbody rb;

	void Awake () {
		Initiate ();
	}

	public void Initiate () {
		rb = GetComponent<Rigidbody> ();

		// init mounting stuff
		seat = transform.Find("Seat");
		handsContainer = transform.Find("Hands").gameObject;
		handsContainer.SetActive (false);
	}

	public bool canBeMounted { get { return (Time.time >= nextEnterTime); } }

	public virtual void Mount (GameObject _mounter) {
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

	public virtual void Dismount () {
		mounter.GetComponent<BoxCollider> ().enabled = true;
		mounter.GetComponent<Rigidbody> ().isKinematic = false;
		mounter.transform.parent = null;

		handsContainer.SetActive (false);

		driver = false;
		nextEnterTime = Time.time + reentryWait;
		rb.interpolation = RigidbodyInterpolation.None;
	}
}
