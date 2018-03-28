using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParachuteEnemySpawn : SpecialSpawn {
	public GameObject parachutePrefab;
	public GameObject enemyPrefab;

	public override void Init () {
		Instantiate (parachutePrefab, transform.position, transform.rotation, transform.parent);
		Instantiate (enemyPrefab, transform.position, transform.rotation, transform.parent);

		base.Init ();
	}
}
