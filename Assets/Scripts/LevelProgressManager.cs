using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Objective {
	public enum Type {
		Pickup,
		Zone,
		KillCount,
		Vehicle
	}

	public Type type;

	public Transform spawnPoint; //where the player spawns after completing this objective
	public GameObject objectiveObj; //object to set up
	public GameObject objectsToDisable; //optional objects to disable after completing this objective
	public GameObject objectsToEnable; //optional objects to enable after completing this objective

	public bool showsWorldIndicator;
	public string bannerText;
	public List<string> splashTexts = new List<string>();
}

public class AIData {
	public Vector3 position;
	public float angle;
	public string weaponName;

	public AIData (AIController controller) {
		position = controller.transform.position;
		angle = controller.transform.rotation.eulerAngles.y;
		weaponName = controller.curWeaponName;
	}
}

public class LevelProgressManager : MonoBehaviour {
	public static LevelProgressManager instance;
	public static int curObjectiveId;
	public static string lastWeaponName = "None";
	public static Dictionary<float, AIData> startingAIData = new Dictionary<float, AIData> ();
	public static List<float> killedAIs = new List<float> ();
	List<float> killedAIsSinceLastCheckpoint = new List<float>();
	
	[Header("Objective")]
	public GameObject objectiveScreenIndicator;
	public GameObject objectiveWorldIndicator;
	public List<Objective> objectives = new List<Objective>();

	[Header("UI")]
	public GameObject winScreen;

	[Header("Debug")]
	public int startingObjective = 0;

	public bool allEnemiesKilled { get { return enemyParent.childCount == 0; } }
	public bool isComplete;
	PlayerController pc;
	Transform enemyParent;

	void Awake() {
		instance = this;
		enemyParent = GameObject.Find ("Enemies").transform;

		if (curObjectiveId == 0) {
			curObjectiveId = startingObjective;
		}

		winScreen.SetActive (false);
		pc = GameObject.FindObjectOfType<PlayerController> ();

		PrepareObjectives ();
		InitNextObjective ();
		UpdateObjectiveUI ();
	}

	void Start() {
		if (curObjectiveId > 0) {
			UpdatePlayer ();
		}
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

	void InitNextObjective() {
		if (objectives.Count == 0) {
			Debug.LogError ("No objectives to set up");
			return;
		}

		switch (objectives[curObjectiveId].type) {
			case Objective.Type.Pickup:
				objectives[curObjectiveId].objectiveObj.tag = "Pickup";
				break;
			case Objective.Type.Zone:
				objectives[curObjectiveId].objectiveObj.GetComponent<PlayerTrigger> ().enterActions.AddListener (CompleteObjective);
				break;
			case Objective.Type.KillCount:
				//TODO
				break;
			case Objective.Type.Vehicle:
				//TODO
//				objectives[0].go.GetComponent<PlayerTrigger> ().enterActions.AddListener (CompleteObjective);
				break;
		}
	}

	void UpdateObjectiveUI() {
		objectiveWorldIndicator.transform.parent = null; //unparent

		bool hasIndicators = objectives.Count > 0 && curObjectiveId < objectives.Count && objectives [curObjectiveId].type != Objective.Type.KillCount;

		//show/hide indicators
		objectiveScreenIndicator.SetActive (hasIndicators);
		objectiveWorldIndicator.SetActive (hasIndicators && objectives[curObjectiveId].showsWorldIndicator);

		if (hasIndicators) {
			objectiveScreenIndicator.GetComponent<EdgeView> ().Init(objectives[curObjectiveId].objectiveObj); //set target
			objectiveWorldIndicator.transform.position = objectives[curObjectiveId].objectiveObj.transform.position; //move to target
		}
	}

	void UpdatePlayer() {
		pc.transform.position = objectives[curObjectiveId - 1].spawnPoint.position; //move player to last checkpoint

		GameObject.FindObjectOfType<CameraController> ().ResetPosition (); //move camera
	}

	void SaveAI() {
		startingAIData.Clear ();
		AIController[] ais = GameObject.FindObjectsOfType<AIController> ();
		foreach(AIController ai in ais) {
			startingAIData.Add (ai.hash, new AIData(ai));
		}

		killedAIs.AddRange (killedAIsSinceLastCheckpoint);
		killedAIsSinceLastCheckpoint.Clear ();
	}

	public void EnemyDeath(float hash) {
		killedAIsSinceLastCheckpoint.Add (hash);
	}

	//assumes player is completing objectives in order for now
	public void CompleteObjective() {
		if(objectives[curObjectiveId].objectsToEnable != null) {
			objectives [curObjectiveId].objectsToEnable.SetActive (true);
		}
		if(objectives[curObjectiveId].objectsToDisable != null) {
			objectives [curObjectiveId].objectsToDisable.SetActive (false);
		}

		if (!string.IsNullOrEmpty(objectives [curObjectiveId].bannerText)) {
			NotificationManager.instance.ShowBanner (objectives[curObjectiveId].bannerText);
		}

		//NOTE: splash text on the final objective does not work
		foreach(string message in objectives[curObjectiveId].splashTexts) {
			NotificationManager.instance.ShowSplash (message);
		}

		curObjectiveId++;
		lastWeaponName = pc.curWeaponName;

		if (curObjectiveId == objectives.Count) {
			CompleteLevel ();
		} else {
			SaveAI ();
			InitNextObjective ();
		}

		UpdateObjectiveUI ();
	}

	public void EnterCutsceneVehicle() {
		objectiveScreenIndicator.SetActive (false);
		objectiveWorldIndicator.SetActive (false);
	}

	//ends the level
	public void CompleteLevel() {
		winScreen.SetActive (true);
		isComplete = true;
		GameManager.instance.EndLevel ();
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
	}
}
