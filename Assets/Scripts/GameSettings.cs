using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings {
	public static bool autoSwiping {
		get {
			return (PlayerPrefs.GetInt ("IsAutoSwiping") == 0) ? false : true;
		}
		set {
			PlayerPrefs.SetInt ("IsAutoSwiping", (value) ? 1 : 0);
		}
	}

	public static int difficulty {
		get {
			if (lastDifficulty == -1) {
				int curDiff = PlayerPrefs.GetInt ("Difficulty", -1);
				if (curDiff == -1) { //the value has not been set yet
					difficulty = 1; //set to default difficulty of 1
				} else {
					lastDifficulty = curDiff;
				}
			}

			return lastDifficulty;
		}
		set {
			if (value != lastDifficulty) {
				PlayerPrefs.SetInt ("Difficulty", value);
				lastDifficulty = value;
			}
		}
	}

	static int lastDifficulty = -1; //reference so that we don't have to get from PlayerPrefs every time
}
