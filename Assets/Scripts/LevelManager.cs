using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public static LevelManager instance;

	public List<LevelData> levelData;

	[System.Serializable]
	public class LevelData {
		public string name;
	}	

	void Awake() {
		instance = this;
	}
}
