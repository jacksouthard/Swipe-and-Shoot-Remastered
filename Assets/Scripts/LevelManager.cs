using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public static LevelManager instance;

	public List<LevelData> levelData;

	[System.Serializable]
	public class LevelData {
		public enum Type
		{
			Campaign,
			Endless,
			Cutscene
		}

		public string name;
		public Type type;
		public string character;
		public Sprite image;

		public string GetCharacterName() {
			return (string.IsNullOrEmpty (character)) ? "Soldier" : character;
		}
	}	

	void Awake() {
		instance = this;
	}
}
