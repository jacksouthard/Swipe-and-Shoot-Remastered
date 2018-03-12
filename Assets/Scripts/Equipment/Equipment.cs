using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour {
	public EquipmentData data { get; private set; }
	protected PlayerController player;

	float durability;

	public virtual void Init(GameObject _go, EquipmentData _data) {
		durability = 100f;
		player = GetComponentInParent<PlayerController>();
		data = _data;
	}

	public virtual void OnJump () {}

	public virtual void OnHit (float damage) {}

	public void TakeDamage(float damage) {
		durability -= damage;
		if (durability <= 0) {
			player.RemoveEquipment ((int)data.slot);
		}
	}

	public virtual void Remove() {
		Destroy (gameObject);
	}
}
