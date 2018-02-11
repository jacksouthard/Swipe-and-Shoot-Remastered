﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingController : MonoBehaviour {
	public bool canRotate;

	[Header("Control")]
	public string targetTag;
	public float clampAngle;

	[Header("Speed")]
	public float parentSpeed; //speed at which the body rotates towards target
	public float speed; //speed at which the hands rotate towards target

	Weapon weapon;

	public void SetWeapon(WeaponManager.WeaponData newWeapon) {
		if (weapon != null) {
			Destroy (weapon.gameObject);
		}

		GameObject weaponObj = Instantiate (newWeapon.prefab, transform.GetChild(0));
		weaponObj.transform.localPosition = Vector3.zero;
		weaponObj.transform.localRotation = Quaternion.identity;

		weapon = weaponObj.GetComponent<Weapon>();
		weapon.SetTarget(targetTag);
	}

	void LateUpdate() {
		if (weapon == null) {
			return;
		}
		Transform target = GetNearestTarget ();
		if (target != null) {
			RotateTowards (target);
		}
	}

	void RotateTowards(Transform target) {
		Quaternion parentRotation = transform.parent.rotation;
		Vector3 diff = target.position - transform.position;
		float angle = Mathf.Atan2 (diff.x, diff.z) * Mathf.Rad2Deg;

		if (canRotate) {
			transform.parent.rotation = Quaternion.Lerp (transform.parent.rotation, Quaternion.Euler (parentRotation.eulerAngles.x, angle, parentRotation.eulerAngles.z), Time.deltaTime * parentSpeed);
		}

		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime * speed);
		transform.localRotation = Quaternion.Euler(ClampedAngle(transform.localRotation.eulerAngles.x), ClampedAngle(transform.localRotation.eulerAngles.y), 0f);
	}

	float ClampedAngle(float angle) {
		return Mathf.Clamp (((angle + 180f) % 360f) - 180f, -clampAngle, clampAngle);
	}

	Transform GetNearestTarget() {
		Collider[] objectsInRange = Physics.OverlapSphere (transform.position, weapon.range);

		float closestDistance = Mathf.Infinity;
		Transform closestObj = null;

		foreach (Collider obj in objectsInRange) {
			if (obj.tag == targetTag) {
				float distance = Vector3.Distance (transform.position, obj.transform.position);
				if (distance < closestDistance) {
					closestObj = obj.transform;
					closestDistance = distance;
				}
			}
		}

		return closestObj;
	}
}