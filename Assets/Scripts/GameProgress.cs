using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameProgress {
	public static int farthestLevel {
		get {
			return PlayerPrefs.GetInt ("FarthestLevel");
		}
		set {
			if (value > farthestLevel) {
				PlayerPrefs.SetInt ("HasSeenStartingMessage", 0);
				PlayerPrefs.SetInt ("FarthestLevel", value);
			}
		}
	}

	public static bool firstTime {
		get {
			return (PlayerPrefs.GetInt ("HasSeenStartingMessage") == 0) ? true : false;
		}
		set {
			PlayerPrefs.SetInt ("HasSeenStartingMessage", value ? 0 : 1);
		}
	}

	public static void Reset() {
		PlayerPrefs.SetInt ("HasSeenStartingMessage", 0);
		PlayerPrefs.SetInt ("FarthestLevel", 0);
	}

	public static void UnlockAll() {
		PlayerPrefs.SetInt ("FarthestLevel", LevelManager.instance.levelData.Count - 1);
		PlayerPrefs.SetInt ("HasSeenStartingMessage", 1);
	}
}
