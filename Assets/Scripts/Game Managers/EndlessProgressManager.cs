using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndlessProgressManager : MonoBehaviour {
	public static EndlessProgressManager instance;

	public Text killCounter;

	int totalKillCount;

	void Awake() {
		instance = this;
		totalKillCount = 0;
		killCounter.text = totalKillCount.ToString ();
	}

	public void RecordEnemyDeath() {
		totalKillCount += 1;
		killCounter.text = totalKillCount.ToString ();
	}
}
