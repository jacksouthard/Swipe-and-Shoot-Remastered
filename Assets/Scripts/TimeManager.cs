using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeManager {
	public static bool isPaused;

	//pauses or unpauses the game
	public static void SetPaused(bool pause) {
		isPaused = pause;
		Time.timeScale = (isPaused) ? 0f : 1f;
	}
}
