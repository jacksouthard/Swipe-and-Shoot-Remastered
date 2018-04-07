using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Objective {
	public enum Type {
		Pickup,
		Zone,
		Kills,
		Vehicle,
		Camera,
		Defend
	}

	public Type type;

	public float initialDelay;
	public List<NotificationManager.SplashData> startingSplashes = new List<NotificationManager.SplashData>();
	public bool hasCameraEvent; //automatically creates a camera event before this objective

	[Space(15)]
	public GameObject objectiveObj; //object to set up
	public GameObject overrideTarget; //optional object that indicators will point to
	public GameObject objectsToDisable; //optional objects to disable after completing this objective
	public GameObject objectsToEnable; //optional objects to enable after completing this objective
	public float time; //time for defend or camera objectives
	public Animator animation;
	public Color fadeInColor = Color.black;
	public Color fadeOutColor = Color.black;
	public bool doesNotSave;

	[Space(15)]
	public Health crucialHealth; //health to track at the top of the screen
	public Sprite icon; //icon for the health thing to track

	[Space(15)]
	public bool timerActiveState;

	[Space(15)]
	public bool showsScreenIndicator = true;
	public bool showsWorldIndicator;
	public string completionBanner;
	public string helpText;
}

public class SavedAI {
	public Vector3 position;
	public float angle;
	public string weaponName;

	public SavedAI (AIController controller) {
		position = controller.transform.position;
		angle = controller.transform.rotation.eulerAngles.y;
		weaponName = controller.curWeaponName;
	}
}

public class SavedVehicle {
	public Vector3 position;
	public Quaternion rotation;
}

public class LevelProgressManager : MonoBehaviour {
	public static LevelProgressManager instance;
	public static int savedObjectiveId;
	public static string lastWeaponName = "None";
	public static Dictionary<float, SavedAI> startingAIData = new Dictionary<float, SavedAI> ();
	public static List<float> killedAIs = new List<float> ();
	public static Dictionary<float, SavedVehicle> startingVehicleData = new Dictionary<float, SavedVehicle>();
	public static Vector3 playerSpawnPoint;
	public static bool firstTime = true;
	static float timeRemaining = 0f;
	List<float> killedAIsSinceLastCheckpoint = new List<float>();
	public int curObjectiveId { get; private set; }
	
	[Header("Objective")]
	public List<Objective> objectives = new List<Objective>();
	Objective curObjective { get { return objectives [curObjectiveId]; } }
	EdgeView objectiveEdgeView;
	public string winMessage;

	[Header("Timer")]
	public GameTimer gameTimer;

	[Header("UI")]
	public GameObject winScreen;
	public Animator barAnim;
	public Animator timerAnim;
	public Transform topBar;
	public Image barIcon;

	[Header("Debug")]
	public int startingObjective = 0;

	public static bool hasMadeProgress {
		get {
			if (instance != null) {
				return savedObjectiveId > instance.startingObjective;
			} else {
				return false;
			}
		}
	}
	public bool isComplete;
	PlayerController pc;
	Text winText;

	public float timer = 0f; //for defend objectives

	void Awake() {
		instance = this;

		if (savedObjectiveId == 0) {
			savedObjectiveId = startingObjective;
		}

		curObjectiveId = savedObjectiveId;

		winScreen.SetActive (false);
		winText = winScreen.transform.Find ("Window").Find("Title").GetComponent<Text>();
		pc = GameObject.FindObjectOfType<PlayerController> ();

		objectiveEdgeView = EdgeView.Create (false);
		objectiveEdgeView.Hide ();

		PrepareObjectives ();

		if (gameTimer != null) {
			gameTimer.enabled = curObjective.timerActiveState;

			if (timeRemaining > 0) {
				gameTimer.Init (timeRemaining);
				return;
			}

			int firstTimerIndex = 0; //find the first event with a timer
			while (!objectives [firstTimerIndex].timerActiveState) {
				firstTimerIndex++;
			}

			if (firstTimerIndex > savedObjectiveId) {
				gameTimer.Init (objectives [firstTimerIndex].time); //set it to what it will start as
			} else {
				foreach(GameTimerEvent timerEvent in gameTimer.timerEvents) {
					if (timerEvent.isActivatedOncePassed && savedObjectiveId > timerEvent.objectiveIdsToActivateOn[0]) {
						timerEvent.onTimerEnd.Invoke (); //we finished the timer so invoke whatever methods we were supposed to invoke
					}
				}
			}
		}
	}

	void Start() {
		if (hasMadeProgress) {
			UpdatePlayer ();
		}

		CameraController.instance.ResetPosition (); //move camera
	}

	public void StartGame() {
		StartCoroutine(InitNextObjective ());
	}

	void PrepareObjectives() {
		for(int i = 0; i < objectives.Count; i++) {
			if (i < savedObjectiveId) {
				if (objectives [i].objectsToDisable != null) {
					objectives [i].objectsToDisable.SetActive (false);
				}

				if (objectives [i].objectsToEnable != null) {
					objectives [i].objectsToEnable.SetActive (true);
				}

				//skip animations we have passed
				if (objectives [i].animation != null) {
					objectives [i].animation.SetTrigger ("Skip");
				}

				//these enemies are dead so don't bring them back
				if (objectives [i].type == Objective.Type.Kills) {
					objectives [i].objectiveObj.SetActive (false);
				}

				//reactivates escort
				EscortController escort = objectives [i].objectiveObj.GetComponent<EscortController> ();
				if (escort != null) {
					escort.Enable ();
				}
			} else {
				if (objectives [i].objectsToEnable != null) {
					objectives [i].objectsToEnable.SetActive (false);
				}
			}

			if (objectives [i].type == Objective.Type.Camera) {
				if (objectives [i].objectiveObj.GetComponent<Camera> () != null) {
					objectives [i].objectiveObj.SetActive (false);
				}
			}

			if (objectives [i].hasCameraEvent) {
				Objective newCameraObjective = new Objective ();
				newCameraObjective.type = Objective.Type.Camera;
				newCameraObjective.objectiveObj = objectives [i].objectiveObj;
				newCameraObjective.showsWorldIndicator = objectives [i].showsWorldIndicator;
				newCameraObjective.time = objectives[i].time;
				objectives [i].time = 0;
				objectives.Insert(i, newCameraObjective);
				i++;
			}
		}
	}

	IEnumerator InitNextObjective() {
		objectiveEdgeView.Hide ();
		NotificationManager.instance.HideHelp ();

		if (objectives.Count == 0) {
			Debug.LogError ("No objectives to set up");
			yield break;
		}

		//some UI updates before the initial delay
		timerAnim.SetBool ("Open", curObjective.timerActiveState);

		if (firstTime) {
			firstTime = false;
			yield return new WaitForSeconds (curObjective.initialDelay);

			foreach (NotificationManager.SplashData message in curObjective.startingSplashes) {
				NotificationManager.instance.ShowSplash (message);
			}
		}

		switch (curObjective.type) {
			case Objective.Type.Pickup:
				curObjective.objectiveObj.tag = "Pickup";
				Pickup pickup = curObjective.objectiveObj.GetComponent<Pickup> ();
				if (pickup != null) {
					pickup.isObjective = true;
				}
				break;
			case Objective.Type.Zone:
				PlayerTrigger trigger = curObjective.objectiveObj.AddComponent<PlayerTrigger> ();
				trigger.enterActions.AddListener (CompleteObjective);
				trigger.oneTime = true;
				break;
			case Objective.Type.Kills:
				StartCoroutine (CheckForEnemyDeaths ());
				break;
			case Objective.Type.Vehicle:
				curObjective.objectiveObj.GetComponent<Rideable> ().SetupObjective ();
				break;
			case Objective.Type.Camera:
				StartCoroutine (CameraEvent ());
				break;
			case Objective.Type.Defend:
				timer = curObjective.time;
				break;
		}

		if (curObjective.animation != null && curObjective.type != Objective.Type.Camera) {
			curObjective.animation.SetTrigger ("Play");
		}

		if (gameTimer != null) {
			gameTimer.enabled = curObjective.timerActiveState;
			if (curObjective.timerActiveState && curObjective.time > 0) {
				gameTimer.Init (curObjective.time);
			}
		}

		UpdateObjectiveUI ();
	}

	void UpdateObjectiveUI() {
		bool hasIndicators = objectives.Count > 0 && curObjectiveId < objectives.Count && curObjective.type != Objective.Type.Kills && curObjective.showsScreenIndicator;

		if (hasIndicators) {
			GameObject targetObj = (curObjective.overrideTarget == null) ? curObjective.objectiveObj : curObjective.overrideTarget;
			objectiveEdgeView.SetTarget (targetObj, curObjective.showsWorldIndicator); //set target
		}

		if (curObjective.type == Objective.Type.Kills) {
			Transform targetParent = curObjective.objectiveObj.transform;
			foreach (Transform child in targetParent) {
				if (child.name.Contains ("Target")) {
					EdgeView.Create(child.gameObject, true);
				}
			}
		}

		if (curObjectiveId < objectives.Count && !string.IsNullOrEmpty (curObjective.helpText)) {
			NotificationManager.instance.ShowHelp (curObjective.helpText);
		}
			
		barAnim.SetBool ("Open", curObjective.crucialHealth != null);
		if (curObjective.crucialHealth != null) {
			if (curObjective.icon != null) {
				barIcon.gameObject.SetActive (true);
				barIcon.sprite = curObjective.icon;
			} else {
				barIcon.gameObject.SetActive (false);
			}
		}
	}

	public void HideAllUI() {
		NotificationManager.instance.HideHelp ();
		if (objectiveEdgeView != null) {
			objectiveEdgeView.Hide ();
		}
	}

	void UpdatePlayer() {
		pc.transform.position = playerSpawnPoint; //move player to last checkpoint
	}

	void SaveGame() {
		playerSpawnPoint = pc.transform.position;
		startingAIData.Clear ();
		AIController[] ais = GameObject.FindObjectsOfType<AIController> ();
		foreach(AIController ai in ais) {
			startingAIData.Add (ai.hash, new SavedAI(ai));
		}

		killedAIs.AddRange (killedAIsSinceLastCheckpoint);
		killedAIsSinceLastCheckpoint.Clear ();

		startingVehicleData.Clear ();
		Rideable[] vehicles = GameObject.FindObjectsOfType<Rideable> ();
		foreach(Rideable vehicle in vehicles) {
			if (vehicle.saves) {
				startingVehicleData.Add (vehicle.hash, vehicle.GetSavedData ());
			}
		}

		if (gameTimer != null) {
			timeRemaining = gameTimer.timeLeft;
		}
	}

	public void EnemyDeath(float hash) {
		killedAIsSinceLastCheckpoint.Add (hash);
	}

	IEnumerator CheckForEnemyDeaths() {
		List<Health> enemyHealths = new List<Health>(curObjective.objectiveObj.GetComponentsInChildren<Health>());

		while(enemyHealths.Count > 0) {
			foreach(Health health in enemyHealths) {
				if (health.state != Health.State.Alive) {
					enemyHealths.Remove (health);
					break; //break out of the loop
				}
			}

			yield return new WaitForEndOfFrame ();
		}

		CompleteObjective ();
	}

	IEnumerator CameraEvent() {
		TimeManager.SetPaused (true);
		Camera newCamera = curObjective.objectiveObj.GetComponent<Camera> ();
		bool movesCamera = newCamera == null;

		if (movesCamera) {
			yield return StartCoroutine (CameraController.instance.ShowTarget (curObjective.objectiveObj.transform));
		} else {
			yield return StartCoroutine (SceneFader.FadeToCameraAndWait(newCamera, curObjective.fadeInColor));
		}

		if (curObjective.animation != null) {
			curObjective.animation.SetTrigger ("Play");
		}

		yield return new WaitForSecondsRealtime (curObjective.time);

		TimeManager.SetPaused (false);
		if (curObjectiveId < objectives.Count - 1) {
			if (movesCamera) {
				CameraController.instance.Resume ();
			} else {
				yield return StartCoroutine (SceneFader.FadeToCameraAndWait (CameraController.instance.GetComponent<Camera> (), curObjective.fadeOutColor));
			}
		}

		CompleteObjective ();
	}

	void Update() {
		if (timer > 0f) {
			timer -= Time.deltaTime;
			if (timer <= 0f) {
				timer = 0f;
				CompleteObjective ();
			}
		}
	}

	void LateUpdate() {
		if (curObjectiveId < objectives.Count && curObjective.crucialHealth != null) {
			topBar.localScale = new Vector3 (Mathf.Max(curObjective.crucialHealth.healthPercentage, 0f), 1f, 1f);
		}
	}

	//assumes player is completing objectives in order for now
	public void CompleteObjective() {
		if (GameManager.instance.isGameOver) {
			return;
		}

		if (curObjectiveId == objectives.Count - 1 || objectives [curObjectiveId + 1].crucialHealth == null) {
			barAnim.SetBool ("Open", false);
		}

		if(curObjective.objectsToEnable != null) {
			curObjective.objectsToEnable.SetActive (true);
		}
		if(curObjective.objectsToDisable != null) {
			curObjective.objectsToDisable.SetActive (false);
		}

		if (!string.IsNullOrEmpty(curObjective.completionBanner)) {
			NotificationManager.instance.ShowBanner (curObjective.completionBanner);
		}

		curObjectiveId++;
		lastWeaponName = pc.curWeaponName;

		if (curObjectiveId == objectives.Count) {
			CompleteLevel ();
		} else {
			firstTime = true;
			if (!objectives [curObjectiveId - 1].doesNotSave) { //if we don't save from the previous objective
				savedObjectiveId = curObjectiveId;
				SaveGame ();
			}
			StartCoroutine(InitNextObjective ());
		}
	}

	public void EnterCutsceneVehicle() {
		barAnim.SetBool ("Open", false);
		NotificationManager.instance.HideHelp ();
		objectiveEdgeView.Hide ();
	}

	//ends the level
	public void CompleteLevel() {
		NotificationManager.instance.HideHelp ();
		objectiveEdgeView.Hide ();

		int levelToUnlock = GameManager.instance.curLevelId;
		int curFurthestLevel = GameProgress.farthestLevel;

		do {
			levelToUnlock++;
			if (levelToUnlock > curFurthestLevel && LevelManager.instance.levelData [levelToUnlock].type == LevelManager.LevelData.Type.Endless) {
				NotificationManager.instance.ShowBanner ("New endless unlocked");
			}
		} while (levelToUnlock < (LevelManager.instance.levelData.Count - 1) && LevelManager.instance.levelData [levelToUnlock].type != LevelManager.LevelData.Type.Campaign);
		GameProgress.farthestLevel = levelToUnlock;

		if (GameManager.instance.levelData.type == LevelManager.LevelData.Type.Cutscene) {
			Continue ();
			return;
		}

		if (!string.IsNullOrEmpty (winMessage)) {
			winText.text = winMessage;
		}
		winScreen.SetActive (true);
		isComplete = true;

		GameManager.instance.EndLevel ();
	}

	public void Continue() {
		int nextLevel = GameManager.instance.curLevelId;

		do {
			nextLevel++;
		} while(nextLevel < (LevelManager.instance.levelData.Count - 1) && LevelManager.instance.levelData [nextLevel].type == LevelManager.LevelData.Type.Endless);

		if (nextLevel < LevelManager.instance.levelData.Count - 1 && LevelManager.instance.levelData[nextLevel].type != LevelManager.LevelData.Type.Endless) {
			MainMenu.LoadLevel (nextLevel);
		} else {
			GameManager.instance.ReturnToMain (Mathf.Min(LevelManager.instance.levelData.Count - 1, nextLevel), true);
		}
	}

	public void Restart() {
		GameManager.instance.Restart ();
	}

	public void ReturnToMain() {
		GameManager.instance.ReturnToMain (true);
	}

	public static void Reset() {
		savedObjectiveId = 0;
		lastWeaponName = "None";
		startingAIData.Clear ();
		killedAIs.Clear ();
		startingVehicleData.Clear ();
		playerSpawnPoint = Vector3.zero;
		firstTime = true;
		timeRemaining = 0f;
	}

	public static float CalculateHash(Vector3 pos) {
		return (pos.x * 1000) + (pos.z); //hash is based on initial position
	}
}
