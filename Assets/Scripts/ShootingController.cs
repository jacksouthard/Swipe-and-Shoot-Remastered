using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingController : MonoBehaviour {
	public bool canRotateParent;

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

	public bool targetInRange { get { return target != null; } }
	public bool hasWeapon { get { return weapon != null; } }

	bool isPlayer;
	PlayerController pc;
	Transform player;
	Health health;
	Weapon weapon;
	Transform target;

	void Awake() {
		isPlayer = gameObject.GetComponentInParent<PlayerController> () != null;
		if (!isPlayer) {
			pc = GameObject.FindObjectOfType<PlayerController> ();
			player = pc.transform.Find("Center");
		}

		health = gameObject.GetComponentInParent<Health> ();
	}

	public void SetWeapon(WeaponManager.WeaponData newWeapon) {
		if (hasWeapon) {
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

	public void RemoveWeapon() {
		Destroy (weapon.gameObject);

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
		if (!hasWeapon) {
			return;
		}
		if (!isPlayer) {
			target = (pc.inVehicle) ? pc.currentVehicle.transform.Find("Center") : player;
		} else {
			target = GetNearestTarget ();
		}
	
		if (target != null) {
			RotateTowards (target);
		}
	}

	void RotateTowards(Transform target) {
		Quaternion parentRotation = transform.parent.rotation;
		Vector3 diff = target.position - transform.position;
		float angle = Mathf.Atan2 (diff.x, diff.z) * Mathf.Rad2Deg;

		if (canRotateParent) {
			transform.parent.rotation = Quaternion.Lerp (transform.parent.rotation, Quaternion.Euler (parentRotation.eulerAngles.x, angle, parentRotation.eulerAngles.z), Time.deltaTime * parentSpeed * weapon.speedMultiplier);
		}

		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime * speed * weapon.speedMultiplier);
		transform.localRotation = Quaternion.Euler(ClampedAngle(transform.localRotation.eulerAngles.x), ClampedAngle(transform.localRotation.eulerAngles.y), 0f);
	}

	public void OverrideRotateParent(float angle) {
		Quaternion parentRotation = transform.parent.rotation;

		if (canRotateParent) {
			transform.parent.rotation = Quaternion.Lerp (transform.parent.rotation, Quaternion.Euler (parentRotation.eulerAngles.x, angle, parentRotation.eulerAngles.z), Time.deltaTime * parentSpeed);
		}
	}

	float ClampedAngle(float angle) {
		return Mathf.Clamp (((angle + 180f) % 360f) - 180f, -clampAngle, clampAngle);
	}
		
	public WeaponManager.WeaponData GetWeaponData() {
		return (hasWeapon) ? WeaponManager.instance.WeaponDataFromName (weapon.name) : null;
	}

	Transform GetNearestTarget() {
		Collider[] objectsInRange = Physics.OverlapSphere (transform.position, weapon.range);

		float closestDistance = Mathf.Infinity;
		Transform closestObj = null;

		foreach (Collider obj in objectsInRange) {
			if (obj.tag == targetTag) {
				if (targetTag == "Enemy" && obj.GetComponentInParent<Health>().state != Health.State.Alive) {
					continue;
				}

				float distance = Vector3.Distance (transform.position, obj.transform.position);
				if (distance < closestDistance) {
					closestObj = obj.transform;
					closestDistance = distance;
				}
			}
		}

		if (closestObj != null) {
			closestObj = closestObj.Find ("Center");
		}

		return closestObj;
	}
}