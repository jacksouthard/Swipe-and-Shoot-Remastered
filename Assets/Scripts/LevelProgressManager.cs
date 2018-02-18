using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour {
	public static LevelProgressManager instance;
	public static int curCheckpointId;

	[Header("UI")]
	public GameObject winScreen;
	public GameObject checkpointText;
	public float notificationTime;

	public bool isComplete;

	void Awake() {
		instance = this;
		winScreen.SetActive (false);
		checkpointText.SetActive (false);

		InitCheckpoints ();
	}

	void Start() {
		MovePlayer ();
	}

	void InitCheckpoints() {
		for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild (i).GetComponent<Checkpoint> ().Init (i);
		}
	}

	void MovePlayer() {
		if (curCheckpointId > 0) {
			GameObject.FindObjectOfType<PlayerController> ().transform.position = transform.GetChild (curCheckpointId).position;
			GameObject.FindObjectOfType<CameraController> ().ResetPosition ();
		}
	}

	public void CompleteLevel() {
		winScreen.SetActive (true);
		isComplete = true;
		GameManager.instance.EndLevel ();
	}

	public void TriggerCheckpoint(int checkpointId) {
		curCheckpointId = checkpointId;
		StartCoroutine (ShowCheckpointReached());
	}

	IEnumerator ShowCheckpointReached() {
		checkpointText.SetActive (true);
		yield return new WaitForSeconds (notificationTime);
		checkpointText.SetActive (false);
	}
}
