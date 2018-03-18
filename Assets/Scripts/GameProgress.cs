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
				PlayerPrefs.SetInt ("FarthestLevel", value);
			}
		}
	}

	public static void Reset() {
		PlayerPrefs.SetInt ("FarthestLevel", 0);
	}

	public static void UnlockAll() {
		PlayerPrefs.SetInt ("FarthestLevel", LevelManager.instance.levelData.Count - 1);
	}
}
