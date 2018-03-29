using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour {
	public static float curFadeAmount { get { return (instance != null) ? instance.fader.color.a : 0; } }

	static SceneFader instance;
	public Image fader { get; private set; }

	Color fullColor;
	Color zeroColor;

	const float fadeSpeed = 10;

	static void SetUpInstance() {
		if (instance == null) {
			GameObject prefab = Resources.Load("SceneFader") as GameObject;
			GameObject sceneFaderObj = (GameObject)Instantiate (prefab);
			instance = sceneFaderObj.GetComponent<SceneFader> ();
			instance.Init ();
			DontDestroyOnLoad (instance);
		}
	}

	public static void FadeToScene(int buildIndex, Color color) {
		SetUpInstance ();

		instance.SetColor (color);
		instance.StartCoroutine (instance.SwitchScenes(buildIndex));
	}

	public static void FadeToCamera(Camera camera, Color color) {
		SetUpInstance ();

		instance.SetColor (color);
		instance.StartCoroutine (instance.SwitchCameras(camera));
	}

	public static IEnumerator FadeToCameraAndWait(Camera camera, Color color) {
		SetUpInstance ();

		instance.SetColor (color);

		yield return instance.StartCoroutine (instance.SwitchCameras(camera));
	}

	public static void FadeToColor(Color color) {
		SetUpInstance ();

		instance.SetColor (color);
		instance.StartCoroutine (instance.FadeInAndOut());
	}

	public void Init() {
		fader = gameObject.GetComponentInChildren<Image> ();
	}

	public void SetColor(Color color) {
		fullColor = new Color(color.r, color.g, color.b, 1f);
		zeroColor = new Color(color.r, color.g, color.b, 0f);
	}

	public IEnumerator SwitchScenes(int buildIndex) {
		yield return StartCoroutine (FadeIn());

		AsyncOperation loadingLevel = SceneManager.LoadSceneAsync (buildIndex);
		yield return new WaitUntil (() => loadingLevel.isDone);

		yield return StartCoroutine (FadeOut());
	}

	public IEnumerator SwitchCameras(Camera camera) {
		yield return StartCoroutine (FadeIn());

		GameObject.FindObjectOfType<Camera> ().gameObject.SetActive (false);
		camera.gameObject.SetActive (true);

		yield return StartCoroutine (FadeOut());
	}

	public IEnumerator FadeInAndOut() {
		yield return StartCoroutine (FadeIn());
		yield return StartCoroutine (FadeOut());
	}
		
	IEnumerator FadeIn() {
		fader.raycastTarget = true;
		fader.color = zeroColor;
		float p = 0f;
		float t = Time.fixedUnscaledDeltaTime;

		while(p < 1f) {
			fader.color = Color.Lerp (zeroColor, fullColor, p);
			p += t * fadeSpeed;
			yield return new WaitForSecondsRealtime (t);
		}
		fader.color = fullColor;
	}

	IEnumerator FadeOut() {
		fader.color = fullColor;
		float p = 0f;
		float t = Time.fixedUnscaledDeltaTime;

		while(p < 1f) {
			fader.color = Color.Lerp (fullColor, zeroColor, p);
			p += t * fadeSpeed;
			yield return new WaitForSecondsRealtime (t);
		}
		fader.color = zeroColor;
		fader.raycastTarget = false;
	}
}
