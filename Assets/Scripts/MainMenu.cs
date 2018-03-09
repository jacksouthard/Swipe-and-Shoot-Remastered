using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	public Text levelTitleText;

	int curLevelIndex = 0;

	void Awake() {
		TimeManager.SetPaused (false);
		LevelProgressManager.Reset ();
		Spawner.spawners.Clear ();
		LoadLevelData (0);//later replace with last level
	}

	public void CycleLevel(int dir) {
		int newLevelIndex = curLevelIndex + dir;
		if (newLevelIndex >= 0 && newLevelIndex < LevelManager.instance.levelData.Count) {
			LoadLevelData (newLevelIndex);
		}
	}

	void LoadLevelData(int levelIndex) {
		curLevelIndex = levelIndex;
		levelTitleText.text = LevelManager.instance.levelData [levelIndex].name;
	}

	public void StartLevel() {
		SceneManager.LoadScene (curLevelIndex + 1);
	}
}
