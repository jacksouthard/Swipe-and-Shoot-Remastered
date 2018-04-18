using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
	public static int startingLevel = -1;

	public Text levelTitleText;

	public Animator menuAnim;

	[Header("Buttons")]
	public GameObject leftButton;
	public GameObject rightButton;
	public Toggle autoSwipingToggle;
	public Image[] difficultyButtons;
	public GameObject starParent;
	public Image resetButton;
	public Text resetText;
	Image[] stars;

	[Header("Transition")]
	public AnimationCurve backgroundCurve;
	public float transitionTime;
	public RectTransform backgroundParent;
	public List<Image> backgrounds;

	RectTransform mainCanvas;

	int curLevelIndex = 0;
	bool transitioning = false;
	bool resetConfirmed = false;

	void Awake() {
		TimeManager.SetPaused (false);

		mainCanvas = levelTitleText.GetComponentInParent<Canvas> ().GetComponent<RectTransform>();
		autoSwipingToggle.isOn = GameSettings.autoSwiping;
		stars = starParent.GetComponentsInChildren<Image> ();
		GameProgress.LoadProgressData ();

		UpdateDifficultyUI ();
	}

	void Start() {
		LoadFromFarthestLevel ();
	}

	void LoadFromFarthestLevel() {
		LoadLevelData ((startingLevel != -1) ? startingLevel : GameProgress.farthestLevel);
	}

	public void CycleLevel(int dir) {
		StartCoroutine (CycleAnim(dir));
	}

	public IEnumerator CycleAnim(int dir) {
		int newLevelIndex = GetLevelIdInDir (dir);
		
		
		if (transitioning || newLevelIndex < 0 || newLevelIndex >= LevelManager.instance.levelData.Count) {
			yield break;
		}

		transitioning = true;
		float p = 0;
		while(p < 1f) {
			p += (Time.deltaTime / transitionTime);
			backgroundParent.anchoredPosition = new Vector2 ((mainCanvas.rect.width + 25f) * backgroundCurve.Evaluate(p) * -dir, 0); //25 is the size of the bars
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

		leftButton.SetActive (curLevelIndex > 0);
		rightButton.SetActive (curLevelIndex < GameProgress.farthestLevel);

		for (int i = 0; i < backgrounds.Count; i++) {
			int bgLevelIndex = GetLevelIdInDir (i - 1);
			if (bgLevelIndex >= 0 && bgLevelIndex < LevelManager.instance.levelData.Count) {
				backgrounds [i].sprite = LevelManager.instance.levelData [bgLevelIndex].image;
			}
		}

		if (curLevelIndex == 0 || data.type != LevelManager.LevelData.Type.Campaign) {
			starParent.SetActive (false);
		} else {
			starParent.SetActive (true);
			int bestDifficulty = GameProgress.GetBestDifficultyForIndex (curLevelIndex);
			for (int i = 0; i < stars.Length; i++) {
				stars [i].color = (i <= bestDifficulty) ? Color.yellow : new Color (0, 0, 0, 0.5f);
			}
		}
	}

	int GetLevelIdInDir(int dir) {
		if (dir == 0) {
			return curLevelIndex;
		}

		int newLevelIndex = curLevelIndex;
		do {
			newLevelIndex += dir;
		} while(newLevelIndex < LevelManager.instance.levelData.Count && newLevelIndex > 0 && LevelManager.instance.levelData[newLevelIndex].type == LevelManager.LevelData.Type.Cutscene);

		return newLevelIndex;
	}

	public void StartLevel() {
		LoadLevel (curLevelIndex);
	}

	public void ResetGame() {
		if (resetConfirmed) {
			GameProgress.Reset ();
			startingLevel = 0;
			SceneFader.FadeToScene (0, Color.black); //return to title screen
		} else {
			resetConfirmed = true;
			UpdateResetButton ();
		}
	}

	void UpdateResetButton() {
		if (resetConfirmed) {
			resetButton.color = Color.red;
			resetText.text = "Sure?";
		} else {
			resetButton.color = Color.gray;
			resetText.text = "Reset game";
		}
	}

	public void BetaUnlock() {
		GameProgress.UnlockAll ();
		startingLevel = GameProgress.farthestLevel;
		LoadFromFarthestLevel ();
		OpenSettingsMenu (false);
	}

	public void ChangeDifficulty(int value) {
		resetConfirmed = false;
		UpdateResetButton ();

		difficultyButtons [GameSettings.difficulty].color = Color.gray;
		GameSettings.difficulty = value;
		UpdateDifficultyUI ();
	}

	void UpdateDifficultyUI() {
		difficultyButtons [GameSettings.difficulty].color = new Color(0f, 0.5f, 1f);
	}

	public static void LoadLevel(int levelIndex) {
		LevelProgressManager.Reset ();
		Spawner.spawners.Clear ();
		SceneFader.FadeToScene (levelIndex + 2, Color.black);
	}

	public void ToggleAutoSwiping(bool isOn) {
		GameSettings.autoSwiping = isOn;
	}

	public void OpenSettingsMenu(bool open) {
		if (open) {
			resetConfirmed = false;
			UpdateResetButton ();
		}

		menuAnim.SetBool ("SettingsOpen", open);
	}
}
