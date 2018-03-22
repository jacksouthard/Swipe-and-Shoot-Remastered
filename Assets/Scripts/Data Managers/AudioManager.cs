using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public static AudioManager instance;

	public List<AudioClip> enemyDeathSounds;

	void Awake() {
		instance = this;
	}

	public AudioClip GetRandomEnemyDeathSound() {
		return enemyDeathSounds[Random.Range(0, enemyDeathSounds.Count)];
	}
}
