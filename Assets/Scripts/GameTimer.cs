using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour {
	public float time;
	public Text[] displayTexts;

	float timeLeft = 0f;

	void Awake() {
		if (timeLeft == 0) {
			Init ();
		}
	}

	public void Init() {
		timeLeft = time;
		UpdateTexts ();
	}

	void Update() {
		if (timeLeft > 0f) {
			timeLeft -= Time.deltaTime;
			UpdateTexts ();
			if (timeLeft <= 0f) {
				timeLeft = 0;
				UpdateTexts ();
				GameManager.instance.GameOver ("Time's up");
			}
		}
	}

	void UpdateTexts() {
		foreach(Text text in displayTexts) {
			text.text = string.Format ("{0:00}:{1:00}", Mathf.FloorToInt(timeLeft / 60), Mathf.FloorToInt(timeLeft % 60));;
		}
	}
}
