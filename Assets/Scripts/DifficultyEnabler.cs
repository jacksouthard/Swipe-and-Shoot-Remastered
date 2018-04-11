using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyEnabler : MonoBehaviour {
	public int minDifficulty;
	public int maxDifficutly;

	public bool shouldDestroy = false;

	void Awake() {
		if (GameSettings.difficulty < minDifficulty || GameSettings.difficulty > maxDifficutly) {
			shouldDestroy = true;
			Destroy (gameObject);
		}
	}
}
