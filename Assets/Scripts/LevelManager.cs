using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public static LevelManager instance;

	public List<LevelData> levelData;
	[HideInInspector]
	public List<int> campaignLevelIds = new List<int>();

	[System.Serializable]
	public class LevelData {
		public string name;
		public List<NotificationManager.SplashData> startingMessages;
		public List<NotificationManager.SplashData> mainMenuMessages;
		public string character;
		public Sprite image;
		
		[HideInInspector]
		public int campaignId;

		public string GetCharacterName() {
			return (string.IsNullOrEmpty (character)) ? "Soldier" : character;
		}
	}	

	void Awake() {
		instance = this;

		InitializeCampaignIds ();
	}

	void InitializeCampaignIds() {
		int id = 0;
		for(int i = 0; i < levelData.Count; i++) {
			if (!levelData[i].name.Contains ("Endless")) {
				levelData[i].campaignId = id;
				campaignLevelIds.Add (i);
				id++;
			}
		}
	}
}
