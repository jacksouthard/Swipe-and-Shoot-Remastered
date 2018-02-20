using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	public static bool firstTime = true; //reset this when we go to the main menu

	public int levelId;
	public float killHeight;

	[Header("Game Start")]
	public GameObject startScreen;
	public Text levelTitle;
	public Text requirements;

	[Header("Game Over")]
	public GameObject gameOverScreen;
	public bool isGameOver;

	LevelManager.LevelData levelData;

	void Awake() {
		instance = this;
		InitLevelData ();
		TimeManager.SetPaused (true);
	}

	void InitLevelData() {
		levelData = LevelManager.instance.levelData [levelId];
		levelTitle.text = levelData.name;
		requirements.text = "Special requirements: " + (levelData.requiresElimination ? "Kill all enemies" : "None");
		startScreen.SetActive (true);
		gameOverScreen.SetActive (false);
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

	public void GameOver() {
		if (LevelProgressManager.instance == null || !LevelProgressManager.instance.isComplete) {
			gameOverScreen.SetActive (true);
			isGameOver = true;

			EndLevel ();
		}
	}

	public void EndLevel() {
		Destroy (gameObject.GetComponent<SwipeManager> ());
		Spawner enemySpawner = GameObject.FindObjectOfType<Spawner> ();
		if(enemySpawner != null) {
			Destroy (enemySpawner); //no new enemies!
		}
	}

	public void Restart() {
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}
}
