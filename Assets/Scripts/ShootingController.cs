using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingController : MonoBehaviour {
	public bool canRotateParent;

	[Header("Control")]
	public string targetTag;
	public float clampAngle;
	public bool shouldUpdateTarget = true;

	[Header("Speed")]
	public float parentSpeed; //speed at which the body rotates towards target
	public float speed; //speed at which the hands rotate towards target

	[Header("Throwing")]
	public GameObject weaponPickupPrefab;
	public float throwHeight;
	public float throwVelocity;

	public bool targetInRange { get { return target != null; } }
	public bool hasWeapon { get { return weapon != null; } }
	public string curWeaponName { get { return (hasWeapon) ? weapon.name : "None"; } }
	public float range { get { return (hasWeapon) ? weapon.range : 0; } }

	public Transform target { get; private set; }

	bool isPlayer;
	PlayerController pc;
	Transform player;
	Health health;
	Weapon weapon;

	void Awake() {
		isPlayer = gameObject.GetComponentInParent<PlayerController> () != null;
		if (!isPlayer) {
			pc = GameObject.FindObjectOfType<PlayerController> ();
			player = pc.transform.Find("Center"); //target is the player's "center"
		}

		health = gameObject.GetComponentInParent<Health> ();
	}

	//updates current weapon and throws out old weapon
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

	//instantly removes current weapon
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

		//choose target
		if (shouldUpdateTarget) {
			if (targetTag == "Player") {
				target = (pc.inVehicle) ? pc.currentVehicle.transform.Find ("Center") : player;
			} else {
				target = GetNearestTarget ();
			}
		}
	
		if (target != null) {
			RotateTowards (target);
		} else {
			ResetRotation (); //idle position
		}
	}

	public void OverrideSwitchTargets(Transform newTarget) {
		if (newTarget == player) {
			shouldUpdateTarget = true;
			target = (pc.inVehicle) ? pc.currentVehicle.transform.Find ("Center") : player;
		} else {
			shouldUpdateTarget = false;
			target = newTarget;
		}
	}

	//point towards a target
	void RotateTowards(Transform target) {
		Quaternion parentRotation = transform.parent.rotation;
		Vector3 diff = target.position - transform.position;
		float angle = Mathf.Atan2 (diff.x, diff.z) * Mathf.Rad2Deg;

		if (canRotateParent) { //rotates character towards target
			transform.parent.rotation = Quaternion.RotateTowards (transform.parent.rotation, Quaternion.Euler (parentRotation.eulerAngles.x, angle, parentRotation.eulerAngles.z), Time.deltaTime * parentSpeed * weapon.speedMultiplier);
		}

		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime * speed * weapon.speedMultiplier);
		transform.localRotation = Quaternion.Euler(ClampedAngle(transform.localRotation.eulerAngles.x), ClampedAngle(transform.localRotation.eulerAngles.y), 0f);
	}

	//rotates parent in a direction (for when player looks in direction of swipe)
	public void OverrideRotateParent(float angle) {
		Quaternion parentRotation = transform.parent.rotation;

		if (canRotateParent) {
			transform.parent.rotation = Quaternion.RotateTowards (transform.parent.rotation, Quaternion.Euler (parentRotation.eulerAngles.x, angle, parentRotation.eulerAngles.z), Time.deltaTime * parentSpeed);
		}
	}

	void ResetRotation() {
		transform.localRotation = Quaternion.RotateTowards (transform.localRotation, Quaternion.identity, Time.deltaTime * parentSpeed);
	}

	float ClampedAngle(float angle) {
		return Mathf.Clamp (((angle + 180f) % 360f) - 180f, -clampAngle, clampAngle);
	}
		
	public WeaponManager.WeaponData GetWeaponData() {
		return (hasWeapon) ? WeaponManager.instance.WeaponDataFromName (weapon.name) : null;
	}

	public void Die() {
		if (hasWeapon) {
			Destroy (weapon);
		}	

		Destroy (this);
	}

	//find nearest in range
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