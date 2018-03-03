using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Objective {
	public enum Type {
		Pickup,
		Zone,
		KillCount
	}

	public Type type;

	public GameObject go; //object to set up
	public GameObject newObjects; //optional objects to enable after completing this objective
}

public class LevelProgressManager : MonoBehaviour {
	public static LevelProgressManager instance;
	public static int curCheckpointId;
	public static string lastWeaponName = "None";
	
	[Header("Objective")]
	public GameObject objectiveScreenIndicator;
	public GameObject objectiveWorldIndicator;
	public List<Objective> objectives = new List<Objective>();

	[Header("UI")]
	public GameObject winScreen;

	public bool allEnemiesKilled { get { return enemyParent.childCount == 0; } }
	public bool isComplete;
	PlayerController pc;
	Transform enemyParent;

	void Awake() {
		instance = this;
		enemyParent = GameObject.Find ("Enemies").transform;

		winScreen.SetActive (false);
		pc = GameObject.FindObjectOfType<PlayerController> ();

		InitCheckpoints ();
		PrepareObjectives ();
		InitNextObjective ();
		UpdateObjectiveUI ();
	}

	void Start() {
		if (curCheckpointId > 0) {
			UpdatePlayer ();
		}
	}

	void InitCheckpoints() {
		for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild (i).GetComponent<Checkpoint> ().Init (i); //initialize each checkpoint (child objects of this transform)
		}
	}

	void PrepareObjectives() {
		foreach(Objective objective in objectives) {
			if (objective.newObjects != null) {
				objective.newObjects.SetActive (false);
			}
		}
	}

	void InitNextObjective() {
		if (objectives.Count == 0) {
			Debug.LogError ("No objectives to set up");
			return;
		}

		switch (objectives[0].type) {
			case Objective.Type.Pickup:
				objectives[0].go.tag = "Pickup";
				break;
			case Objective.Type.Zone:
				objectives[0].go.GetComponent<PlayerTrigger> ().enterActions.AddListener (CompleteObjective);
				break;
			case Objective.Type.KillCount:
				//TODO
				break;
		}
	}

	void UpdateObjectiveUI() {
		objectiveWorldIndicator.transform.parent = null; //unparent

		bool hasIndicators = objectives.Count > 0 && objectives [0].type != Objective.Type.KillCount;

		//show/hide indicators
		objectiveScreenIndicator.SetActive (hasIndicators);
		objectiveWorldIndicator.SetActive (hasIndicators);

		if (hasIndicators) {
			objectiveScreenIndicator.GetComponent<EdgeView> ().Init(objectives[0].go); //set target
			objectiveWorldIndicator.transform.position = objectives[0].go.transform.position; //move to target
		}
	}

	void UpdatePlayer() {
		pc.transform.position = transform.GetChild (curCheckpointId).position; //move player to last checkpoint

		GameObject.FindObjectOfType<CameraController> ().ResetPosition (); //move camera
	}

	//assumes player is completing objectives in order for now
	public void CompleteObjective() {
		if(objectives[0].newObjects != null) {
			objectives [0].newObjects.SetActive (true);
		}

		objectives.RemoveAt (0);

		if (objectives.Count == 0) {
			CompleteLevel ();
		} else {
			InitNextObjective ();
		}

		UpdateObjectiveUI ();
	}

	//ends the level
	public void CompleteLevel() {
		winScreen.SetActive (true);
		isComplete = true;
		GameManager.instance.EndLevel ();
	}

	//called when a player enters a checkpoint
	public void TriggerCheckpoint(int checkpointId) {
		curCheckpointId = checkpointId;
		lastWeaponName = pc.curWeaponName;

		NotificationManager.instance.ShowBanner ("CHECKPOINT REACHED");
	}

	public void Restart() {
		GameManager.instance.Restart ();
	}

	public void ReturnToMain() {
		GameManager.instance.ReturnToMain ();
	}

	public static void Reset() {
		curCheckpointId = 0;
		lastWeaponName = "None";
	}
}
