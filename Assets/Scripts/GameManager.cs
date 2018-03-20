using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	public static bool firstTime = true;
	public static List<Transform> allEnemyTargets = new List<Transform>();

	[Header("Game Start")]
	public GameObject startScreen;
	public Text levelTitle;
	public Text characterText;

	[Header("Game Over")]
	public GameObject gameOverScreen;
	public bool isGameOver;
	bool isGameWon { get { return LevelProgressManager.instance != null && LevelProgressManager.instance.isComplete; } }
	Text gameOverText;

	[Header("Pause")]
	public GameObject pauseScreen;

	public LevelManager.LevelData levelData { get; private set; }
	public int curLevelId { get { return SceneManager.GetActiveScene().buildIndex - 1; } }

	void Awake() {
		instance = this;

		startScreen.SetActive (true);
		gameOverScreen.SetActive (false);
		pauseScreen.SetActive (false);

		gameOverText = gameOverScreen.transform.Find ("Window").Find ("Title").GetComponent<Text> ();

		allEnemyTargets = new List<Transform> ();
		allEnemyTargets.Add (GameObject.FindObjectOfType<PlayerController>().transform);

		InitLevelData ();
		TimeManager.SetPaused (true);
	}

	void InitLevelData() {
		levelData = LevelManager.instance.levelData [curLevelId];
		levelTitle.text = levelData.name;
		characterText.text = "character: " + levelData.GetCharacterName();
	}

	public void StartGame() {
		TimeManager.SetPaused (false);
		startScreen.SetActive (false);

		if (firstTime) {
			firstTime = false;

			foreach (NotificationManager.SplashData message in levelData.startingMessages) {
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

	public void GameOver(string gameOverMessage = "game over") {
		if (!isGameWon && !isGameOver) {
			gameOverText.text = gameOverMessage;
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

	public void Restart(bool fullReset = true) {
		if (fullReset) {
			LevelProgressManager.Reset ();
			firstTime = true;
		}
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}

	public void ReturnToMain(int startingLevelId) {
		MainMenu.startingLevel = startingLevelId;
		SceneManager.LoadScene (0);
	}

	public void ReturnToMain() {
		ReturnToMain (curLevelId);
	}
}
