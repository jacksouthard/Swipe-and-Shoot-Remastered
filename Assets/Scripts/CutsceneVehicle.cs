using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneVehicle : Rideable {
	Animator anim;

	void Awake() {
		anim = GetComponent<Animator> ();
		anim.enabled = false;
		base.Initiate ();
	}

	public override void Mount (GameObject _mounter) {
		base.Mount (_mounter);
		anim.enabled = true;
	}

	//call this at the end of the animation
	public void OnAnimationEnd() {
		LevelProgressManager.instance.CompleteLevel ();
	}
}
