using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour {
	public static LevelProgressManager instance;

	public GameObject winScreen;

	public bool isComplete;

	void Awake() {
		instance = this;
		winScreen.SetActive (false);
	}

	public void CompleteLevel() {
		winScreen.SetActive (true);
		isComplete = true;
		GameManager.instance.EndLevel ();
	}
}
