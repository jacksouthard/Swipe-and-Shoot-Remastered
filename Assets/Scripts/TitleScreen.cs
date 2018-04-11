using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : MonoBehaviour {
	public AudioSource riser;
	public AudioSource music;

	public void StartGame () {
		//for now, we assume that this is the user's first time if they haven't beaten boot camp
		if (GameProgress.farthestLevel != 0) {
			SceneFader.FadeToScene (1, Color.black); //load main menu
		} else {
			GetComponent<Animator>().SetTrigger ("ShowDifficulty");
		}
	}

	public void StartRiser() {
		riser.Play ();
	}

	public void StartMusic() {
		riser.Stop ();
		music.Play ();
	}

	public void ChooseDifficulty(int difficulty) {
		GameSettings.difficulty = difficulty;
		SceneFader.FadeToScene (2, Color.black); //load boot camp
	}
}
