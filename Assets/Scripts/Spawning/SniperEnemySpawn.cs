using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperEnemySpawn : SpecialSpawn {
	public GameObject enemyPrefab;

	public override void Init () {
		GameObject newEnemy = (GameObject)Instantiate (enemyPrefab, transform.position, transform.rotation, transform.parent);
		EnemyController controller = newEnemy.GetComponent<EnemyController> ();
		controller.defaultWeapon = "KAR98";
		controller.moves = false;

		base.Init ();
	}
}
