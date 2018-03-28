using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour {
	public Text[] displayTexts;

	public UnityEvent onTimerEnd;

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
				onTimerEnd.Invoke ();
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
