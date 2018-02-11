using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour {
	Vechicle vechicle;

	public bool onGround = false;

	void Start () {
		vechicle = GetComponentInParent<Vechicle> ();
	}
	
//	void OnCollisionEnter (Collision coll) {
//		print ("ent");
//		onGround = true;
//	}
//
//	void OnCollisionExit (Collision coll) {
//		onGround = false;
//	}
}
