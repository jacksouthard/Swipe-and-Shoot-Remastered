using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	public static bool firstTime = true; //reset this when we go to the main menu

	[Header("Game Start")]
	public GameObject startScreen;
	public Text levelTitle;
	public Text requirements;

	[Header("Game Over")]
	public GameObject gameOverScreen;
	public bool isGameOver;
	bool isGameWon { get { return LevelProgressManager.instance != null && LevelProgressManager.instance.isComplete; } }

	[Header("Pause")]
	public GameObject pauseScreen;

	LevelManager.LevelData levelData;

	void Awake() {
		instance = this;

		startScreen.SetActive (true);
		gameOverScreen.SetActive (false);
		pauseScreen.SetActive (false);

		InitLevelData ();
		TimeManager.SetPaused (true);
	}

	void InitLevelData() {
		levelData = LevelManager.instance.levelData [SceneManager.GetActiveScene().buildIndex - 1];
		levelTitle.text = levelData.name;
		requirements.text = "Special requirements: " + (levelData.requiresElimination ? "Kill all enemies" : "None");
	}

	public void StartGame() {
		TimeManager.SetPaused (false);
		startScreen.SetActive (false);

		if (firstTime) {
			firstTime = false;

			foreach (string message in levelData.startingMessages) {
				NotificationManager.instance.ShowSplash (message);
			}
		}
	}

	public void PauseGame(bool shouldPause) {
		if (!isGameOver && !isGameWon) {
			pauseScreen.SetActive (shouldPause);
			TimeManager.SetPaused (shouldPause);
		}
	}

	public void GameOver() {
		if (!isGameWon) {
			SetupGameOverScreen ();
			gameOverScreen.SetActive (true);
			isGameOver = true;

			EndLevel ();
		}
	}

	void SetupGameOverScreen() {
		Button continueButton = gameOverScreen.transform.Find ("Window").Find("Buttons").Find("ContinueButton").GetComponent<Button>();
		continueButton.interactable = LevelProgressManager.curCheckpointId > 0;

		Text buttonText = continueButton.GetComponentInChildren<Text> ();
		buttonText.color = new Color (buttonText.color.r, buttonText.color.g, buttonText.color.b, (LevelProgressManager.curCheckpointId > 0) ? 1f : 0.5f);
	}

	public void EndLevel() {
		Destroy (gameObject.GetComponent<SwipeManager> ());
		Spawner enemySpawner = GameObject.FindObjectOfType<Spawner> ();
		if(enemySpawner != null) {
			Destroy (enemySpawner); //no new enemies!
		}
	}

	public void Restart(bool fullReset = true) {
		if (fullReset) {
			LevelProgressManager.Reset ();
		}
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}

	public void ReturnToMain() {
		SceneManager.LoadScene (0);
	}
}
