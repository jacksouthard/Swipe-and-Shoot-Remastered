using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour {
	public UnityEvent enterActions;
	public UnityEvent exitActions;

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") {
			enterActions.Invoke ();
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.tag == "Player") {
			exitActions.Invoke ();
		}
	}
}
