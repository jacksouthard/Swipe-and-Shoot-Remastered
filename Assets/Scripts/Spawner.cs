using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
	public static Spawner instance;

	public GameObject prefab;
	public int maxObjects;
	public float spawnRate;
	public float minSpawnRange;

	int count = 0;
	float spawnTimer;

	Transform player;
	List<Vector3> spawnPoints = new List<Vector3>();

	void Awake () {
		instance = this;
		player = GameObject.FindObjectOfType<PlayerController> ().transform;
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
		if (count < maxObjects) {
			spawnTimer -= Time.deltaTime;
			if (spawnTimer <= 0f) {
				spawnTimer = spawnRate;

				SpawnEnemy ();
			}
		}
	}

	public void EnemyDeath () {
		count--;
	}

	void SpawnEnemy () {
		Vector3 spawnPoint = FindValidSpawnPoint ();

		GameObject enemy = Instantiate (prefab, spawnPoint, Quaternion.identity, transform);

		count++;
	}

	Vector3 FindValidSpawnPoint() {
		Vector3 spawnPoint;
		do {
			int index = Random.Range (0, spawnPoints.Count);
			spawnPoint = spawnPoints [index];
		} while (Vector3.Distance(spawnPoint, player.position) < minSpawnRange);

		return spawnPoint;
	}
}
