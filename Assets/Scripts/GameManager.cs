using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	public bool isPaused;
	public int levelId;

	public GameObject startScreen;
	public Text levelTitle;

	void Awake() {
		instance = this;
		InitLevelData ();
		SetPaused (true);
	}

	void InitLevelData() {
		levelTitle.text = LevelManager.instance.levelData [levelId].name;
		startScreen.SetActive (true);
	}

	public void StartGame() {
		SetPaused (false);
		startScreen.SetActive (false);
	}

	public void SetPaused(bool pause) {
		isPaused = pause;
		Time.timeScale = (isPaused) ? 0f : 1f;
	}
}
