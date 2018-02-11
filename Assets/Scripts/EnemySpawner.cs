using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
	public static EnemySpawner instance;

	public GameObject enemyPrefab;
	public int maxEnemies;
	public float spawnRate;

	int enemyCount = 0;
	float spawnTimer;

	List<Vector3> spawnPoints = new List<Vector3>();

	void Awake () {
		instance = this;
	}

	void Start () {
		for (int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild (i);
			if (child.name.Contains ("SpawnPoint")) {
				spawnPoints.Add (child.position);
			}
		}

		spawnTimer = spawnRate;
	}
	
	void Update () {
		if (enemyCount < maxEnemies) {
			spawnTimer -= Time.deltaTime;
			if (spawnTimer <= 0f) {
				spawnTimer = spawnRate;

				SpawnEnemy ();
			}
		}
	}

	void SpawnEnemy () {
		int index = Random.Range (0, spawnPoints.Count);
		Vector3 spawnPoint = spawnPoints [index];

		GameObject enemy = Instantiate (enemyPrefab, spawnPoint, Quaternion.identity, transform);

		enemyCount++;
	}
}
