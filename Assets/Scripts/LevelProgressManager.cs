using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour {
	public static LevelProgressManager instance;

	public bool isComplete;

	void Awake() {
		instance = this;
	}

	public void CompleteLevel() {
		isComplete = true;
		GameManager.instance.EndLevel ();
	}
}
