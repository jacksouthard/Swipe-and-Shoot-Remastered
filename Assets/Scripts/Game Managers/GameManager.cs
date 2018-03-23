﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	public static List<Transform> allEnemyTargets = new List<Transform>();

	[Header("Game Start")]
	public GameObject startScreen;
	public Text levelTitle;

	[Header("Game Over")]
	public GameObject gameOverScreen;
	public bool isGameOver;
	bool isGameWon { get { return LevelProgressManager.instance != null && LevelProgressManager.instance.isComplete; } }
	Text gameOverText;

	[Header("Pause")]
	public GameObject pauseScreen;

	public LevelManager.LevelData levelData { get; private set; }
	public int curLevelId { get { return SceneManager.GetActiveScene().buildIndex - 2; } }

	void Awake() {
		instance = this;

		startScreen.SetActive (true);
		gameOverScreen.SetActive (false);
		pauseScreen.SetActive (false);

		gameOverText = gameOverScreen.transform.Find ("Window").Find ("Title").GetComponent<Text> ();

		allEnemyTargets = new List<Transform> ();
		allEnemyTargets.Add (GameObject.FindObjectOfType<PlayerController>().transform);

		InitLevelData ();
	}

	void Start() {
		StartGame ();
	}

	void InitLevelData() {
		levelData = LevelManager.instance.levelData [curLevelId];
		levelTitle.text = levelData.name;
	}

	public void StartGame() {
		TimeManager.SetPaused (false);
		startScreen.SetActive (false);

		if (LevelProgressManager.instance != null) {
			LevelProgressManager.instance.StartGame ();
		}
	}

	public void PauseGame(bool shouldPause) {
		if (!isGameOver && !isGameWon) {
			pauseScreen.SetActive (shouldPause);
			TimeManager.SetPaused (shouldPause);
		}
	}

	public void GameOver(string gameOverMessage = "game over") {
		if (!isGameWon && !isGameOver) {
			gameOverText.text = gameOverMessage;
			gameOverScreen.SetActive (true);
			isGameOver = true;

			EndLevel ();
		}
	}

	public void EndLevel() {
		SwipeManager.instance.EndSwipe ();

		Destroy (SwipeManager.instance);
		Spawner enemySpawner = GameObject.FindObjectOfType<Spawner> ();
		if(enemySpawner != null) {
			Destroy (enemySpawner); //no new enemies!
		}
	}

	public void Restart(bool fullReset = true) {
		if (fullReset) {
			LevelProgressManager.Reset ();
		}
		SceneFader.FadeToScene(SceneManager.GetActiveScene ().buildIndex, Color.black);
	}

	public void ReturnToMain(int startingLevelId) {
		MainMenu.startingLevel = startingLevelId;
		SceneFader.FadeToScene (1, Color.black);
	}

	public void ReturnToMain() {
		ReturnToMain (curLevelId);
	}
}