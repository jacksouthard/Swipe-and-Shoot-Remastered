using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour {
	public System.Action actions;

	void OnAnimationEnd() {
		if (actions != null) {
			actions.Invoke ();
		}
	}
}
