using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour {
	public static LevelProgressManager instance;
	public static int curCheckpointId;
	public static string lastWeaponName = "None";
	
	[Header("Objective")]
	public GameObject objectiveScreenIndicator;
	public GameObject objectiveWorldIndicator;
	public Transform objective;

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

		SetupObjectiveUI ();
		InitCheckpoints ();
	}

	void Start() {
		if (curCheckpointId > 0) {
			UpdatePlayer ();
		}
	}

	void SetupObjectiveUI() {
		//show/hide indicators
		objectiveScreenIndicator.SetActive (objective != null);
		objectiveWorldIndicator.SetActive (objective != null);

		if (objective != null) {
			objectiveScreenIndicator.GetComponent<EdgeView> ().target = objective.gameObject; //set target
			objectiveWorldIndicator.transform.position = objective.position; //move to target
			objectiveWorldIndicator.transform.parent = null; //unparent
		}
	}

	void InitCheckpoints() {
		for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild (i).GetComponent<Checkpoint> ().Init (i); //initialize each checkpoint (child objects of this transform)
		}
	}

	void UpdatePlayer() {
		pc.transform.position = transform.GetChild (curCheckpointId).position; //move player to last checkpoint

		GameObject.FindObjectOfType<CameraController> ().ResetPosition (); //move camera
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
