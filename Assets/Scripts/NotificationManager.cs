using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour {
	public static NotificationManager instance;

	public GameObject bannerParent;
	Text bannerText;
	Animator bannerAnim;

	public GameObject splashParent;
	Text splashText;
	Animator splashAnim;

	public GameObject helpParent;
	Text helpText;
	Animator helpAnim;

	//for storage in the event of successive messages
	List<string> splashes = new List<string> ();
	List<string> banners = new List<string>();

	const float bannerTime = 2f;
	const float bannerWait = 0.5f; //delay between successive banners
	const float splashAnimTime = 0.5f; //delay before unpausing after the last splash screen

	void Awake() {
		bannerText = bannerParent.GetComponentInChildren<Text> ();
		bannerAnim = bannerParent.GetComponent<Animator>();

		splashText = splashParent.transform.Find("Panel").Find("SplashText").GetComponent<Text> (); //there is also text on the close button so we need to make sure it's the right one
		splashAnim = splashParent.GetComponent<Animator>();

		helpText = helpParent.GetComponentInChildren<Text> ();
		helpAnim = helpParent.GetComponent<Animator>();

		bannerParent.SetActive (true);
		splashParent.SetActive (true);
		helpParent.SetActive (true);

		instance = this;
	}

	//notifications that appear at the top of the screen for a certain amount of time
	public void ShowBanner(string message) {
		banners.Add (message);

		if (banners [0] == message) {
			StartCoroutine (PlayBanner());
		}
	}

	IEnumerator PlayBanner() {
		//continue looping until all banners have been cleared
		while (banners.Count > 0) {
			bannerText.text = banners [0];
			SetAnim (bannerAnim, true);

			yield return new WaitForSeconds (bannerTime);

			SetAnim (bannerAnim, false);

			yield return new WaitForSeconds (bannerWait);
		
			banners.RemoveAt (0);
		}
	}

	//notifications that fill the whole screen and pause the game
	public void ShowSplash(string message) {
		splashes.Add (message);

		if (splashes[0] == message) {
			DisplaySplash ();
			SetAnim (splashAnim, true); //only play animation the first time
		}
	}

	//show splash text
	void DisplaySplash() {
		TimeManager.SetPaused (true);
		splashText.text = splashes[0];
	}

	public void HideSplash() {
		splashes.RemoveAt (0);

		if (splashes.Count == 0) {
			SetAnim (splashAnim, false);
			StartCoroutine (UnpauseDelayed());
		} else {
			DisplaySplash(); //keep going until all splashes have been cleared
		}
	}

	IEnumerator UnpauseDelayed() {
		yield return new WaitForSecondsRealtime (splashAnimTime);
		TimeManager.SetPaused (false);
	}

	//notifications that appear on the bottom of the screen under certain conditions
	public void ShowHelp(string message) {
		helpText.text = message;
		SetAnim (helpAnim, true);
	}

	public void HideHelp() {
		SetAnim (helpAnim, false);
	}

	void SetAnim(Animator anim, bool state) {
		anim.SetBool ("Active", state);
	}
}
