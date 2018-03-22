using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CostumeManager : MonoBehaviour {
	public static CostumeManager instance;

	public List<CostumeData> datas;

	void Awake() {
		instance = this;
	}

	public GameObject GetDataFromName(string _name) {
		foreach(CostumeData data in datas) {
			if (data.name == _name) {
				return data.prefab;
			}
		}

		Debug.LogError ("Character " + _name + " not found.");
		return datas[0].prefab;
	}
}

[System.Serializable]
public class CostumeData {
	public string name;
	public GameObject prefab;
}
