using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour {
	public static SpriteManager instance;

	public List<SpriteData> datas;

	void Awake () {
		instance = this;
	}

	public Sprite GetDataFromName(string _name) {
		foreach(SpriteData data in datas) {
			if (data.name == _name) {
				return data.sprite;
			}
		}

		Debug.LogError ("Sprite " + _name + " not found.");
		return null;
	}
}

[System.Serializable]
public class SpriteData {
	public string name;
	public Sprite sprite;
}
