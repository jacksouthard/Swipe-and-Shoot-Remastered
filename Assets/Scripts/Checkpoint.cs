﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
	int id;

	public void Init(int index) {
		id = index;

		if (id < LevelProgressManager.curCheckpointId) {
			ClearEnemies ();
		}
	}

	void ClearEnemies() {
		Collider col = gameObject.GetComponent<Collider> ();
		Collider[] enemisInCol = null;

		if (col is BoxCollider) {
			BoxCollider boxCol = col as BoxCollider;
			enemisInCol = Physics.OverlapBox (transform.position + boxCol.center, boxCol.size / 2, Quaternion.identity, 1<<8);
		} else if (col is SphereCollider) {
			SphereCollider sphereCol = col as SphereCollider;
			enemisInCol = Physics.OverlapSphere (sphereCol.center, sphereCol.radius, 1<<8);
		}

		foreach (Collider enemyCol in enemisInCol) {
			Destroy (enemyCol.GetComponentInParent<EnemyController>().gameObject);
		}
	}

	void OnTriggerEnter(Collider other) {
		if ((id > LevelProgressManager.curCheckpointId) && other.GetComponentInParent<PlayerController> ()) {
			LevelProgressManager.instance.TriggerCheckpoint (id);
		}
	}
}