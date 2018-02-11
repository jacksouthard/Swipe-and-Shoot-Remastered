using System.Collections;
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

	[Header("Throwing")]
	public GameObject weaponPickupPrefab;
	public float throwHeight;
	public float throwVelocity;

	bool isPlayer;
	Transform player;
	Health health;
	Weapon weapon;

	void Awake() {
		isPlayer = gameObject.GetComponentInParent<PlayerController> () != null;
		if (!isPlayer) {
			player = GameObject.FindObjectOfType<PlayerController> ().transform;
		}

		health = gameObject.GetComponentInParent<Health> ();
	}

	public void SetWeapon(WeaponManager.WeaponData newWeapon) {
		if (weapon != null) {
			ThrowWeapon ();
		}

		GameObject weaponObj = Instantiate (newWeapon.prefab, transform.GetChild(0));
		weaponObj.transform.localPosition = Vector3.zero;
		weaponObj.transform.localRotation = Quaternion.identity;
		weaponObj.name = newWeapon.name;

		weapon = weaponObj.GetComponent<Weapon>();
		weapon.SetTarget(targetTag);

		if (health != null) {
			health.UpdateRenderersNextFrame ();
		}
	}

	void ThrowWeapon () {
		Vector3 pos = transform.parent.TransformPoint(0f, throwHeight, 1f);
		GameObject newPickup = Instantiate (weaponPickupPrefab, pos, Quaternion.identity);
		WeaponManager.WeaponData newWeaponData = WeaponManager.instance.WeaponDataFromName (weapon.name);
		newPickup.GetComponent<WeaponPickup> ().Init (newWeaponData);
		newPickup.GetComponent<Rigidbody>().velocity = transform.parent.forward * throwVelocity;

		Destroy (weapon.gameObject);
	}

	void LateUpdate() {
		if (weapon == null) {
			return;
		}
		Transform target = (!isPlayer) ? player : GetNearestTarget ();
	
		if (target != null) {
			RotateTowards (target);
		}
	}

	void RotateTowards(Transform target) {
		Quaternion parentRotation = transform.parent.rotation;
		Vector3 diff = target.position - transform.position + (Vector3.up * 0.6f);
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