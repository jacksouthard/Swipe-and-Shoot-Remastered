using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	public bool isPaused;
	public int levelId;
	public float killHeight;

	[Header("Game Start")]
	public GameObject startScreen;
	public Text levelTitle;
	public Text requirements;

	[Header("Game Over")]
	public GameObject gameOverScreen;
	public bool isGameOver;

	void Awake() {
		instance = this;
		InitLevelData ();
		SetPaused (true);
	}

	void InitLevelData() {
		levelTitle.text = LevelManager.instance.levelData [levelId].name;
		requirements.text = "Special requirements: " + ((LevelManager.instance.levelData [levelId].requiresElimination) ? "Kill all enemies" : "None");
		startScreen.SetActive (true);
		gameOverScreen.SetActive (false);
	}

	public void StartGame() {
		SetPaused (false);
		startScreen.SetActive (false);
	}

	public void SetPaused(bool pause) {
		isPaused = pause;
		Time.timeScale = (isPaused) ? 0f : 1f;
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
