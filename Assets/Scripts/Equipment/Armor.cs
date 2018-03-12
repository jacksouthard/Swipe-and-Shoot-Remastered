using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor : Equipment {
	public float resistance;

	Health health;

	public override void Init (GameObject _go, EquipmentData _data) {
		base.Init (_go, _data);
		health = player.GetComponent<Health> ();
		health.resistance += resistance;
	}

	public override void OnHit (float damage) {
		TakeDamage (damage);
	}

	public override void Remove () {
		health.maxHealth -= resistance;
		base.Remove ();
	}
}
