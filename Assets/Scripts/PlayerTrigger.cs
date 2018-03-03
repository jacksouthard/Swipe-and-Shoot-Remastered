using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour {
	public UnityEvent enterActions;
	public UnityEvent exitActions;
	public bool oneTime;

	Transform player;
	bool activated = false;

	void Awake() {
		player = GameObject.FindObjectOfType<PlayerController> ().transform;
	}

	void OnTriggerEnter(Collider other) {
		if ((other.transform == player || other.GetComponentInParent<PlayerController>() != null) && shouldActivate) {
			enterActions.Invoke ();
		}
	}

	void OnTriggerExit(Collider other) {
		if ((other.transform == player || other.GetComponentInParent<PlayerController>() != null) && shouldActivate) {
			exitActions.Invoke ();
			activated = true;
		}
	}

	bool shouldActivate { get { return (!oneTime || (oneTime && !activated)); } }
}
