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
		int prevBest = GetBestDifficultyForIndex (index);
		if (GameSettings.difficulty > prevBest) {
			string key = LevelManager.instance.levelData [index].name;

			if (progressData.ContainsKey(key)) {
				progressData.Remove (key);
			}

			LevelProgressData newData = new LevelProgressData (key, GameSettings.difficulty);
			progressData.Add (key, newData);

			string serializedData = "";
			foreach(KeyValuePair<string, LevelProgressData> pair in progressData) {
				serializedData += pair.Value.Serialized ();
				serializedData += "/";
			}

			serializedData = serializedData.Substring (0, serializedData.Length - 1); //cut off last slashs

			PlayerPrefs.SetString ("ProgressData", serializedData);
		}
	}

	public static int GetBestDifficultyForIndex(int levelIndex) {
		string key = LevelManager.instance.levelData [levelIndex].name;

		return (progressData.ContainsKey (key)) ? progressData [key].bestDifficulty : -1;
	}

	public static Dictionary<string, LevelProgressData> progressData = new Dictionary<string, LevelProgressData> ();

	public static void LoadProgressData() {
		if (progressData.Count > 0) {
			return;
		}

		string curData = PlayerPrefs.GetString ("ProgressData");
		if (!string.IsNullOrEmpty (curData)) {
			string[] dataList = curData.Split ('/');
			foreach (string levelData in dataList) {
				LevelProgressData newData = new LevelProgressData(levelData);
				progressData.Add(newData.levelName, newData);
			}
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
		PlayerPrefs.SetString ("ProgressData", "");
		progressData.Clear ();
	}

	public static void UnlockAll() {
		PlayerPrefs.SetInt ("FarthestLevel", LevelManager.instance.levelData.Count - 1);
	}

	public class LevelProgressData {
		public string levelName;
		public int bestDifficulty;

		public LevelProgressData(string name, int difficulty) {
			levelName = name;
			bestDifficulty = difficulty;
		}

		public LevelProgressData (string info) {
			string[] tokens = info.Split(',');

			levelName = tokens[0];
			bestDifficulty = int.Parse(tokens[1]);
			//make sure we check that future tokens aren't null since some people may only have these ones 
		}

		public string Serialized() {
			return levelName + "," + bestDifficulty.ToString();
		}
	}
}
