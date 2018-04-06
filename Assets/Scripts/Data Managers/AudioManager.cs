using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	[System.Serializable]
	public class DeathSound
	{
		public AudioClip sound;
		public int tier;
	}

	public static AudioManager instance;

	public List<DeathSound> enemyDeathSounds;
	int[] tierValues = {0, 70, 99}; //hardcoded tier probabilities

	void Awake() {
		instance = this;
	}

	public AudioClip GetRandomEnemyDeathSound() {
		int random = Random.Range (0, 100);
		int randomTier = 0;

		while((randomTier + 1) < tierValues.Length && random >= tierValues[randomTier + 1]) {
			randomTier++;
		}

		List<DeathSound> allDataOfTier = new List<DeathSound> ();
		foreach (DeathSound deathSound in enemyDeathSounds) {
			if (deathSound.tier == randomTier) {
				allDataOfTier.Add (deathSound);
			}
		}

		if (allDataOfTier.Count == 0) {
			// no clips in tier
			Debug.Log ("No death sounds found in tier " + randomTier);
			return null;
		} else {
			int index = Random.Range (0, allDataOfTier.Count);
			return allDataOfTier [index].sound;
		}
	}
}
