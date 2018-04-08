using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyEnabler : MonoBehaviour {
	public int minDifficulty;

	void Awake() {
		if (GameSettings.difficulty < minDifficulty) {
			Destroy (gameObject);
		}
	}
}
