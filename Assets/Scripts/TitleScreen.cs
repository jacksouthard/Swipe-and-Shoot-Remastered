﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : MonoBehaviour {
	public void StartGame () {
		//for now, we assume that this is the user's first time if they haven't beaten boot camp
		int levelToLoad = (GameProgress.farthestLevel != 0) ? 1 : 2; //1 = main menu scene, 2 = bootcamp
		SceneFader.FadeToScene(levelToLoad, Color.black);
	}
}