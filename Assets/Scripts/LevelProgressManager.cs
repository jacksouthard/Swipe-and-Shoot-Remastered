﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Objective {
	public enum Type {
		Pickup,
		Zone,
		Kills,
		Vehicle
	}

	public Type type;

	public float initialDelay;
	public List<NotificationManager.SplashData> startingSplashes = new List<NotificationManager.SplashData>();

	[Space(15)]
	public Transform spawnPoint; //where the player spawns after completing this objective
	public GameObject objectiveObj; //object to set up
	public GameObject objectsToDisable; //optional objects to disable after completing this objective
	public GameObject objectsToEnable; //optional objects to enable after completing this objective

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
	public static bool firstTime = true;
	List<float> killedAIsSinceLastCheckpoint = new List<float>();
	
	[Header("Objective")]
	public List<Objective> objectives = new List<Objective>();
	EdgeView objectiveEdgeView;
	public string winMessage;

	[Header("UI")]
	public GameObject winScreen;

	[Header("Debug")]
	public int startingObjective = 0;

	public bool allEnemiesKilled { get { return enemyParent.childCount == 0; } }
	public bool isComplete;
	PlayerController pc;
	Transform enemyParent;
	Text winText;

	void Awake() {
		instance = this;
		enemyParent = GameObject.Find ("Enemies").transform;

		if (curObjectiveId == 0) {
			curObjectiveId = startingObjective;
		}

		winScreen.SetActive (false);
		winText = winScreen.transform.Find ("Window").Find("Title").GetComponent<Text>();
		pc = GameObject.FindObjectOfType<PlayerController> ();

		objectiveEdgeView = EdgeView.Create ();
		objectiveEdgeView.Hide ();

		PrepareObjectives ();
	}

	void Start() {
		if (curObjectiveId > 0) {
			UpdatePlayer ();
		}
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
		}
	}

	IEnumerator InitNextObjective() {
		objectiveEdgeView.Hide ();
		NotificationManager.instance.HideHelp ();

		yield return new WaitForSeconds (objectives[curObjectiveId].initialDelay);

		if (objectives.Count == 0) {
			Debug.LogError ("No objectives to set up");
			yield break;
		}

		if (firstTime) {
			firstTime = false;
			foreach (NotificationManager.SplashData message in objectives[curObjectiveId].startingSplashes) {
				NotificationManager.instance.ShowSplash (message);
			}
		}

		switch (objectives[curObjectiveId].type) {
			case Objective.Type.Pickup:
				objectives [curObjectiveId].objectiveObj.tag = "Pickup";
				Pickup pickup = objectives [curObjectiveId].objectiveObj.GetComponent<Pickup> ();
				if (pickup != null) {
					pickup.isObjective = true;
				}
				break;
			case Objective.Type.Zone:
				PlayerTrigger trigger = objectives [curObjectiveId].objectiveObj.AddComponent<PlayerTrigger> ();
				trigger.enterActions.AddListener (CompleteObjective);
				trigger.oneTime = true;
				break;
			case Objective.Type.Kills:
				StartCoroutine (CheckForEnemyDeaths ());
				break;
			case Objective.Type.Vehicle:
				objectives [curObjectiveId].objectiveObj.GetComponent<Rideable> ().SetupObjective ();
				break;
		}

		UpdateObjectiveUI ();
	}

	void UpdateObjectiveUI() {
		bool hasIndicators = objectives.Count > 0 && curObjectiveId < objectives.Count;

		if (hasIndicators) {
			GameObject target = objectives [curObjectiveId].objectiveObj;
			//choose objective arrow target
			if (objectives [curObjectiveId].type == Objective.Type.Kills) {
				Transform targetEnemy = null;
				Transform targetParent = objectives [curObjectiveId].objectiveObj.transform;
				foreach (Transform child in targetParent) {
					if (child.name.Contains ("Target")) {
						targetEnemy = child;
						break;
					}
				}

				if (targetEnemy != null) {
					target = targetEnemy.gameObject;
				} else {
					hasIndicators = false;
				}
			}
				
			objectiveEdgeView.SetTarget (target, objectives[curObjectiveId].showsWorldIndicator); //set target
		}

		if (curObjectiveId < objectives.Count && !string.IsNullOrEmpty (objectives [curObjectiveId].helpText)) {
			NotificationManager.instance.ShowHelp (objectives [curObjectiveId].helpText);
		}
	}

	void UpdatePlayer() {
		pc.transform.position = objectives[curObjectiveId - 1].spawnPoint.position; //move player to last checkpoint

		GameObject.FindObjectOfType<CameraController> ().ResetPosition (); //move camera
	}

	void SaveGame() {
		startingAIData.Clear ();
		AIController[] ais = GameObject.FindObjectsOfType<AIController> ();
		foreach(AIController ai in ais) {
			startingAIData.Add (ai.hash, new SavedAI(ai));
		}

		killedAIs.AddRange (killedAIsSinceLastCheckpoint);
		killedAIsSinceLastCheckpoint.Clear ();

		startingVehicleData.Clear ();
		Vehicle[] vehicles = GameObject.FindObjectsOfType<Vehicle> ();
		foreach(Vehicle vehicle in vehicles) {
			startingVehicleData.Add (vehicle.hash, vehicle.GetSavedData());
		}
	}

	public void EnemyDeath(float hash) {
		killedAIsSinceLastCheckpoint.Add (hash);
	}

	IEnumerator CheckForEnemyDeaths() {
		List<Health> enemyHealths = new List<Health>(objectives [curObjectiveId].objectiveObj.GetComponentsInChildren<Health>());

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

	//assumes player is completing objectives in order for now
	public void CompleteObjective() {
		if(objectives[curObjectiveId].objectsToEnable != null) {
			objectives [curObjectiveId].objectsToEnable.SetActive (true);
		}
		if(objectives[curObjectiveId].objectsToDisable != null) {
			objectives [curObjectiveId].objectsToDisable.SetActive (false);
		}

		if (!string.IsNullOrEmpty(objectives [curObjectiveId].completionBanner)) {
			NotificationManager.instance.ShowBanner (objectives[curObjectiveId].completionBanner);
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

		if (!string.IsNullOrEmpty (winMessage)) {
			winText.text = winMessage;
		}

		winScreen.SetActive (true);
		isComplete = true;
		int levelToUnlock = GameManager.instance.curLevelId + 1;
		while (levelToUnlock < (LevelManager.instance.levelData.Count - 1) && LevelManager.instance.levelData [levelToUnlock].name.Contains ("Endless")) {
			levelToUnlock++;
		}
		GameProgress.farthestLevel = levelToUnlock;
		GameManager.instance.EndLevel ();
	}

	public void Continue() {
		int nextLevel = GameManager.instance.curLevelId + 1;
		if (nextLevel < LevelManager.instance.levelData.Count && !LevelManager.instance.levelData [nextLevel].name.Contains ("Endless")) {
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
		firstTime = true;
	}

	public static float CalculateHash(Vector3 pos) {
		return (pos.x * 1000) + (pos.z); //hash is based on initial position
	}
}
