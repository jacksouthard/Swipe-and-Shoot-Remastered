using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscortController : AIController {
	Transform player;

	protected override void Init () {
		player = GameObject.FindObjectOfType<PlayerController>().transform;

		base.Init ();

		enabled = false;
	}

	public void Enable() {
		enabled = true;
		GameManager.allEnemyTargets.Add (transform);
	}

	protected override void UpdateTarget () {
		base.UpdateTarget ();
		SetTargets (player);
	}

	public override void Die() {
		base.Die ();

		GameManager.instance.GameOver ();
	}
}
