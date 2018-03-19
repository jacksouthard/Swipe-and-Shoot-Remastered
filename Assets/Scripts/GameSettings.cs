using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings {
	public static bool autoSwiping {
		get {
			return (PlayerPrefs.GetInt ("IsAutoSwiping") == 0) ? false : true;
		}
		set {
			PlayerPrefs.SetInt ("IsAutoSwiping", (value) ? 1 : 0);
		}
	}
}
