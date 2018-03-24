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
	public GameObject objectsToDisable; //optional objects to disable after completing this objective
	public GameObject objectsToEnable; //optional objects to enable after completing this objective
	public float time; //time for defend or camera objectives
	public Animator animation;

	[Space(15)]
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
	public static int curObjectiveId;
	public static string lastWeaponName = "None";
	public static Dictionary<float, SavedAI> startingAIData = new Dictionary<float, SavedAI> ();
	public static List<float> killedAIs = new List<float> ();
	public static Dictionary<float, SavedVehicle> startingVehicleData = new Dictionary<float, SavedVehicle>();
	public static Vector3 playerSpawnPoint;
	public static bool firstTime = true;
	List<float> killedAIsSinceLastCheckpoint = new List<float>();
	
	[Header("Objective")]
	public List<Objective> objectives = new List<Objective>();
	Objective curObjective { get { return objectives [curObjectiveId]; } }
	EdgeView objectiveEdgeView;
	public string winMessage;

	[Header("UI")]
	public GameObject winScreen;

	[Header("Debug")]
	public int startingObjective = 0;

	public static bool hasMadeProgress {
		get {
			if (instance != null) {
				return curObjectiveId > instance.startingObjective;
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

		if (curObjectiveId == 0) {
			curObjectiveId = startingObjective;
		}

		winScreen.SetActive (false);
		winText = winScreen.transform.Find ("Window").Find("Title").GetComponent<Text>();
		pc = GameObject.FindObjectOfType<PlayerController> ();

		objectiveEdgeView = EdgeView.Create (false);
		objectiveEdgeView.Hide ();

		PrepareObjectives ();
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
			if (i < curObjectiveId) {
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

			if (objectives [i].hasCameraEvent) {
				Objective newCameraObjective = new Objective ();
				newCameraObjective.type = Objective.Type.Camera;
				newCameraObjective.objectiveObj = objectives [i].objectiveObj;
				newCameraObjective.showsWorldIndicator = objectives [i].showsWorldIndicator;
				newCameraObjective.time = objectives[i].time;
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
				StartCoroutine (MoveCamera ());
				break;
			case Objective.Type.Defend:
				timer = curObjective.time;
				break;
		}

		if (curObjective.animation != null && curObjective.type != Objective.Type.Camera) {
			curObjective.animation.SetTrigger ("Play");
		}

		UpdateObjectiveUI ();
	}

	void UpdateObjectiveUI() {
		bool hasIndicators = objectives.Count > 0 && curObjectiveId < objectives.Count && curObjective.type != Objective.Type.Kills;

		if (hasIndicators) {
			objectiveEdgeView.SetTarget (curObjective.objectiveObj, curObjective.showsWorldIndicator); //set target
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

	IEnumerator MoveCamera() {
		TimeManager.SetPaused (true);

		yield return StartCoroutine (CameraController.instance.ShowTarget(curObjective.objectiveObj.transform));

		if (curObjective.animation != null) {
			curObjective.animation.SetTrigger ("Play");
		}

		yield return new WaitForSecondsRealtime (curObjective.time);

		TimeManager.SetPaused (false);
		CameraController.instance.Resume();

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

	//assumes player is completing objectives in order for now
	public void CompleteObjective() {
		if (GameManager.instance.isGameOver) {
			return;
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
			SaveGame ();
			StartCoroutine(InitNextObjective ());
		}
	}

	public void EnterCutsceneVehicle() {
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

		if (LevelManager.instance.levelData[nextLevel].type != LevelManager.LevelData.Type.Endless) {
			MainMenu.LoadLevel (nextLevel);
		} else {
			GameManager.instance.ReturnToMain (Mathf.Min(LevelManager.instance.levelData.Count - 1, nextLevel));
		}
	}

	public void Restart() {
		GameManager.instance.Restart ();
	}

	public void ReturnToMain() {
		GameManager.instance.ReturnToMain ();
	}

	public static void Reset() {
		curObjectiveId = 0;
		lastWeaponName = "None";
		startingAIData.Clear ();
		killedAIs.Clear ();
		startingVehicleData.Clear ();
		playerSpawnPoint = Vector3.zero;
		firstTime = true;
	}

	public static float CalculateHash(Vector3 pos) {
		return (pos.x * 1000) + (pos.z); //hash is based on initial position
	}
}
