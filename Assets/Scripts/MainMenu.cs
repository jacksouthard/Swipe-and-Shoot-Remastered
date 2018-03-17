using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	public Text levelTitleText;
	public Text characterText;

	[Header("Transition")]
	public AnimationCurve backgroundCurve;
	public float transitionTime;
	public RectTransform backgroundParent;
	public List<Image> backgrounds;

	RectTransform mainCanvas;

	int curLevelIndex = 0;
	bool transitioning = false;

	void Awake() {
		TimeManager.SetPaused (false);
		LevelProgressManager.Reset ();
		Spawner.spawners.Clear ();

		mainCanvas = levelTitleText.GetComponentInParent<Canvas> ().GetComponent<RectTransform>();
	}

	void Start() {
		LoadLevelData (GameProgress.farthestLevel);
	}

	public void CycleLevel(int dir) {
		StartCoroutine (CycleAnim(dir));
	}

	public IEnumerator CycleAnim(int dir) {
		int newLevelIndex = curLevelIndex + dir;
		if (transitioning || newLevelIndex < 0 || newLevelIndex >= LevelManager.instance.levelData.Count) {
			yield break;
		}

		transitioning = true;
		float p = 0;
		while(p < 1f) {
			p += (Time.deltaTime / transitionTime);
			backgroundParent.anchoredPosition = new Vector2 (mainCanvas.rect.width * backgroundCurve.Evaluate(p) * -dir, 0);
			yield return new WaitForEndOfFrame ();
		}

		backgroundParent.anchoredPosition = Vector2.zero;
		transitioning = false;
		
		LoadLevelData (newLevelIndex);
	}
		
	void LoadLevelData(int levelIndex) {
		curLevelIndex = levelIndex;
		LevelManager.LevelData data = LevelManager.instance.levelData [levelIndex];
		levelTitleText.text = data.name;
		characterText.text = "Character: " + data.GetCharacterName ();

		for (int i = 0; i < backgrounds.Count; i++) {
			int bgLevelIndex = curLevelIndex + i - 1;
			if (bgLevelIndex >= 0 && bgLevelIndex < LevelManager.instance.levelData.Count) {
				backgrounds [i].sprite = LevelManager.instance.levelData [bgLevelIndex].image;
			}
		}

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

	public void ResetGame() {
		GameProgress.Reset ();
		SceneManager.LoadScene (0);
	}
}
