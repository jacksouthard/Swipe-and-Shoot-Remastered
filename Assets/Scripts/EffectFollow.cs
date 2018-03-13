using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectFollow : MonoBehaviour {
	public string effectName;

	Transform target;
	bool initiated = false;

	static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

	static bool prefabsSet = false;

	public static EffectFollow Create(string name, Transform parent) {
		if (!prefabsSet) {
			LoadPrefabs();
		}

		GameObject newEffect = (GameObject)Instantiate (prefabs[name], parent.position, Quaternion.identity);
		EffectFollow follower = newEffect.GetComponent<EffectFollow> ();
		follower.Init (parent);
		follower.effectName = name;
		return follower;
	}

	static void LoadPrefabs() {
		prefabsSet = true;

		Object[] effects = Resources.LoadAll ("FollowEffects/");
		foreach (Object effect in effects) {
			GameObject go = effect as GameObject;
			prefabs.Add (go.name, go);
		}
	}

	public void Init (Transform _target) {
		target = _target;
		initiated = true;
	}
	
	void LateUpdate () {
		if (initiated) {
			transform.position = target.position;
		}
	}

	public void End() {
		ParticleSystem particles = GetComponent<ParticleSystem>();
		if (particles != null) {
			GetComponent<ParticleSystem> ().Stop ();
			Destroy (gameObject, 1.0f); //wait before actually destroying
		} else {
			Destroy (gameObject);
		}
	}
}
