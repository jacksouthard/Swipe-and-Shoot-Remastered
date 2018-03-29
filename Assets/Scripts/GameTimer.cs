using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class GameTimerEvent {
	public List<int> objectiveIdsToActivateOn;
	public UnityEvent onTimerEnd;
	public bool isActivatedOncePassed; //check this if the event is saved after passing it
}

public class GameTimer : MonoBehaviour {
	public Text[] displayTexts;

	public List<GameTimerEvent> timerEvents = new List<GameTimerEvent>();

	public float timeLeft { get; private set; }

	public void Init(float startingTime) {
		timeLeft = startingTime;
		UpdateTexts ();
	}

	void Update() {
		if (timeLeft > 0f) {
			timeLeft -= Time.deltaTime;
			UpdateTexts ();
			if (timeLeft <= 0f) {
				timeLeft = 0;
				UpdateTexts ();
				foreach(GameTimerEvent timeEvent in timerEvents) {
					if (timeEvent.objectiveIdsToActivateOn.Contains (LevelProgressManager.instance.curObjectiveId)) {
						timeEvent.onTimerEnd.Invoke ();
					}
				}
			//	GameManager.instance.GameOver ("Time's up");
			}
		}
	}

	void UpdateTexts() {
		foreach(Text text in displayTexts) {
			text.text = string.Format ("{0:00}:{1:00}", Mathf.FloorToInt(timeLeft / 60), Mathf.FloorToInt(timeLeft % 60));;
		}
	}
}
