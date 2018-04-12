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

	public static void CompleteLevel(int index) {
		string curData = PlayerPrefs.GetString ("DifficultyData");
		string newData;

		int difficulty = GameSettings.difficulty;

		if (curData.Length >= index + 1) {
			int prevBest = int.Parse (curData [index].ToString ());
			if (difficulty > prevBest) {
				newData = curData.Substring (0, index) + difficulty.ToString ();
				if (newData.Length < curData.Length) {
					newData += curData.Substring (index + 1);
				}
			} else {
				return;
			}
		} else {
			newData = curData;
			while(newData.Length < index) {
				newData += "0";
			}
			newData += difficulty.ToString ();
		}

		PlayerPrefs.SetString ("DifficultyData", newData);
	}

	public static int GetBestDifficultyForIndex(int levelIndex) {
		string curData = PlayerPrefs.GetString ("DifficultyData");

		if (curData.Length < levelIndex + 1) {
			return 0;
		} else {
			return int.Parse (curData[levelIndex].ToString());
		}
	}

	/*public static bool isFirstTime {
		get {
			return PlayerPrefs.GetInt ("IsFirstTime") == 0;
		}
		set {
			PlayerPrefs.SetInt ("IsFirstTime", (value) ? 1 : 0);
		}
	}*/

	public static void Reset() {
		PlayerPrefs.SetInt ("FarthestLevel", 0);
		PlayerPrefs.SetString ("DifficultyData", "");
	}

	public static void UnlockAll() {
		PlayerPrefs.SetInt ("FarthestLevel", LevelManager.instance.levelData.Count - 1);
	}
}
