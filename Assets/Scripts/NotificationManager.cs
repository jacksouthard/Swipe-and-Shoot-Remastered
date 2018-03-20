﻿using System.Collections;
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
	Image splashCharacter;
	Image splashImage;
	Animator splashAnim;
	bool isSplashing;

	public GameObject helpParent;
	Text helpText;
	Animator helpAnim;

	//for storage in the event of successive messages
	List<SplashData> splashes = new List<SplashData> ();
	List<string> banners = new List<string>();

	const float bannerTime = 2f;
	const float bannerWait = 0.5f; //delay between successive banners
	const float splashAnimTime = 0.5f; //delay before unpausing after the last splash screen
	const float splashDelayBetweenCharacters = 0.05f; //delay between each character

	void Awake() {
		bannerText = bannerParent.GetComponentInChildren<Text> ();
		bannerAnim = bannerParent.GetComponent<Animator>();

		Transform splashPanel = splashParent.transform.Find ("Panel");
		splashText = splashPanel.Find("SplashText").GetComponent<Text> (); //there is also text on the close button so we need to make sure it's the right one
		splashCharacter = splashParent.transform.Find("Character").GetComponent<Image>();
		splashImage = splashPanel.Find("SplashImage_Full").GetComponent<Image>();

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

		if (banners.Count == 1) {
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
	public void ShowSplash(SplashData data) {
		splashes.Add (data);

		if (splashes.Count == 1) {
			DisplaySplash (true);
			SetAnim (splashAnim, true); //only play animation the first time
		}
	}

	IEnumerator ShowSplashText(bool firstTime) {
		isSplashing = true;
		splashText.text = "";
		if (firstTime) {
			yield return new WaitForSecondsRealtime (splashAnimTime / 2);
		}

		int curCharacterIndex = 0;
		int messageLength = splashes [0].message.Length;

		while (isSplashing && curCharacterIndex < messageLength) {
			splashText.text += splashes [0].message[curCharacterIndex];
			curCharacterIndex++;
			yield return new WaitForSecondsRealtime (splashDelayBetweenCharacters);
		}

		isSplashing = false;
		splashText.text = splashes [0].message;
	}

	//show splash text
	void DisplaySplash(bool firstTime = false) {
		TimeManager.SetPaused (true);
		if (splashes [0].character == null) {
			splashCharacter.enabled = false;
		} else {
			splashCharacter.sprite = splashes[0].character;
			splashCharacter.preserveAspect = true;
			splashCharacter.enabled = true;
		}
			
		if (splashes[0].image == null) {
			splashText.enabled = true;
			StartCoroutine (ShowSplashText (firstTime));
			splashImage.enabled = false;
		} else {
			splashImage.sprite = splashes [0].image;
			splashImage.preserveAspect = true;
			splashImage.enabled = true;
			splashText.enabled = false;
		}
	}

	public void HideSplash() {
		if (isSplashing) {
			isSplashing = false;
			return;
		}

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

	[System.Serializable]
	public class SplashData {
		public string message;
		public Sprite character;
		public Sprite image;

		public SplashData (string _message, Sprite _character, Sprite _image) {
			message = _message;
			character = _character;
			image = _image;
		}
	}
}
