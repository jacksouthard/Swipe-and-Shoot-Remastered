using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour {
	public static LevelProgressManager instance;
	public static int curCheckpointId;

	public GameObject winScreen;

	public bool isComplete;

	void Awake() {
		instance = this;
		winScreen.SetActive (false);

		InitCheckpoints ();
		MovePlayer ();
	}

	void InitCheckpoints() {
		for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild (i).GetComponent<Checkpoint> ().Init (i + 1);
		}
	}

	void MovePlayer() {
		if (curCheckpointId > 0) {
			GameObject.FindObjectOfType<PlayerController> ().transform.position = transform.GetChild (curCheckpointId - 1).position;
		}
	}

	public void CompleteLevel() {
		winScreen.SetActive (true);
		isComplete = true;
		GameManager.instance.EndLevel ();
	}

	public void TriggerCheckpoint(int checkpointId) {
		curCheckpointId = checkpointId;
	}
}
