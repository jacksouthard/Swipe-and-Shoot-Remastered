using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {
	public void StartGame () {
		//for now, we assume that this is the user's first time if they haven't beaten boot camp
		if(GameProgress.farthestLevel != 0) {
			SceneManager.LoadScene (1); //main menu scene
		} else {
			SceneManager.LoadScene (2); //go to bootcamp
		}
	}
}
