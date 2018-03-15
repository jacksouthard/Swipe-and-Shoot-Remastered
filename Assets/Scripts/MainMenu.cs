using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	public Text levelTitleText;
	public Text characterText;

	int curLevelIndex = 0;

	void Awake() {
		TimeManager.SetPaused (false);
		LevelProgressManager.Reset ();
		Spawner.spawners.Clear ();
	}

	void Start() {
		LoadLevelData (GameProgress.farthestLevel);
	}

	public void CycleLevel(int dir) {
		int newLevelIndex = curLevelIndex + dir;
		if (newLevelIndex >= 0 && newLevelIndex < LevelManager.instance.levelData.Count) {
			LoadLevelData (newLevelIndex);
		}
	}

	void LoadLevelData(int levelIndex) {
		curLevelIndex = levelIndex;
		LevelManager.LevelData data = LevelManager.instance.levelData [levelIndex];
		levelTitleText.text = data.name;
		characterText.text = "Character: " + data.GetCharacterName ();

		if (GameProgress.firstTime) {
			foreach(NotificationManager.SplashData message in data.mainMenuMessages) {
				NotificationManager.instance.ShowSplash (message);
			}

			GameProgress.firstTime = false;
		}
	}

	public void StartLevel() {
		SceneManager.LoadScene (curLevelIndex + 1);
	}
}
