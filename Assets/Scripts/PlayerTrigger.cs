using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour {
	public UnityEvent enterActions = new UnityEvent();
	public UnityEvent exitActions = new UnityEvent();
	public bool oneTime;

	Transform player;
	bool activated = false;

	enum State {
		Entered,
		Exited
	}
	State state;

	void Awake() {
		player = GameObject.FindObjectOfType<PlayerController> ().transform;
		state = State.Exited;
	}

	void OnTriggerEnter(Collider other) {
		if ((other.transform == player || other.transform.root.GetComponentInChildren<PlayerController>() != null) && shouldActivate && state == State.Exited) {
			enterActions.Invoke ();
			state = State.Entered;
		}
	}

	void OnTriggerExit(Collider other) {
		if ((other.transform == player || other.transform.root.GetComponentInChildren<PlayerController>() != null) && shouldActivate && state == State.Entered) {
			exitActions.Invoke ();
			activated = true;
			state = State.Exited;
		}
	}

	bool shouldActivate { get { return (!oneTime || (oneTime && !activated)); } }
}
