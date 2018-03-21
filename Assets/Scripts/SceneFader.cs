using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour {
	static SceneFader instance;
	Image fader;

	const float fadeSpeed = 10;

	public static void FadeToScene(int buildIndex, Color color) {
		if (instance == null) {
			GameObject prefab = Resources.Load("SceneFader") as GameObject;
			GameObject sceneFaderObj = (GameObject)Instantiate (prefab);
			instance = sceneFaderObj.GetComponent<SceneFader> ();
			instance.Init ();
			DontDestroyOnLoad (instance);
		}

		instance.StartCoroutine (instance.Fade(buildIndex, color));
	}

	public void Init() {
		fader = gameObject.GetComponentInChildren<Image> ();
	}

	public IEnumerator Fade(int buildIndex, Color color) {
		Color fullColor = new Color(color.r, color.g, color.b, 1f);
		Color zeroColor = new Color(color.r, color.g, color.b, 0f);

		fader.raycastTarget = true;
		fader.color = zeroColor;
		while(fader.color.a < 0.95f) {
			fader.color = Color.Lerp (fader.color, fullColor, Time.fixedUnscaledDeltaTime * fadeSpeed);
			yield return new WaitForSecondsRealtime (Time.fixedUnscaledDeltaTime);
		}
		fader.color = fullColor;

		AsyncOperation loadingLevel = SceneManager.LoadSceneAsync (buildIndex);

		yield return new WaitUntil (() => loadingLevel.isDone);

		while(fader.color.a > 0.05f) {
			fader.color = Color.Lerp (fader.color, zeroColor, Time.fixedUnscaledDeltaTime * fadeSpeed);
			yield return new WaitForSecondsRealtime (Time.fixedUnscaledDeltaTime);
		}
		fader.color = zeroColor;
		fader.raycastTarget = false;
	}
}
