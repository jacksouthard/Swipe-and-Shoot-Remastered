using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscortController : AIController {
	List<Transform> targetList = new List<Transform>();

	protected override void Init () {
		base.Init ();

		enabled = false;
		targetList.Add(GameObject.FindObjectOfType<PlayerController>().transform);
	}

	public void Enable() {
		enabled = true;
		GameManager.allEnemyTargets.Add (transform);
	}

	protected override void UpdateTarget () {
		base.UpdateTarget ();
		SetTargets (targetList);
	}

	public override void Die() {
		base.Die ();

		GameManager.instance.GameOver ();
	}
}
