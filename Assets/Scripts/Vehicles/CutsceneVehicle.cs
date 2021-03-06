﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneVehicle : Rideable {
	Animator anim;

	void Awake() {
		anim = GetComponent<Animator> ();
		base.Initiate ();
	}

	protected override void CompleteObjective () {
		anim.SetTrigger ("Play");
		dismountable = false;
		LevelProgressManager.instance.EnterCutsceneVehicle ();
	}

	//call this at the end of the animation
	public void OnAnimationEnd() {
		LevelProgressManager.instance.CompleteLevel ();
	}
}
