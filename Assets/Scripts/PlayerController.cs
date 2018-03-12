using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//controls the player character
public class PlayerController : MonoBehaviour {
	public enum MovementState {
		Grounded,
		Jumping,
		Tumbling
	}

	public MovementState state;

	[Header("Control")]
	public float stoppingSpeed; //ground speed before the player returns to standed state
	public float stoppingAngle; //angle difference before the player returns to grounded state
	public float autoResetTime; //amount of time before the player auto resets
	float nextAutoReset;

	[Header("Speed")]
	public float swipeForce; //the amount by which the swiping force is scaled by
	public float verticalFactor; //the amount by which the y-vector of the launch force is scaled by relative to the launch magnitude
	public float turnSpeed; //how fast the player returns to a standing position

	[Header("Weapon")]
	public string defaultWeaponName = "None";
	public string curWeaponName { get { return shooting.curWeaponName; } }

	[Header("Equipment")]
	public List<string> startingEquipment = new List<string> ();
	EquipmentData[] equipment = new EquipmentData[3]; //there are 3 types of equipment
	GameObject[] equipmentObjects = new GameObject[3];
	public Transform equipmentParent;
	Jetpack jetpack; //current jetpack

	[Header("Picking Up")]
	public float pickupTime;
	public GameObject timerDisplay;
	public GameObject timerBar;
	public Text nextPickupText;

	[Header("Throwing")]
	public GameObject pickupPrefab;
	public float throwHeight;
	public float throwVelocity;

	List<PickupTimer> curPickingupTimers = new List<PickupTimer>();

	public bool inVehicle { get { return currentVehicle != null; } }

	[HideInInspector]
	public Rideable currentVehicle;

	Rigidbody rb;
	ShootingController shooting;
	Health health;

	void Awake() {
		rb = gameObject.GetComponent<Rigidbody> ();
		health = gameObject.GetComponent<Health> ();

		shooting = gameObject.GetComponentInChildren<ShootingController> ();
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		timerDisplay.transform.parent = null; //timer moves independently from player

		health.onDeath += Die;
	}

	void Start() {
		//set starting weapon
		string weaponToUse = (LevelProgressManager.lastWeaponName != "None") ? LevelProgressManager.lastWeaponName : defaultWeaponName;
		if (weaponToUse != "None") {
			shooting.SetWeapon (WeaponManager.instance.GetDataFromName (weaponToUse));
		}

		foreach (string equipmentName in startingEquipment) {
			SwitchEquipment (EquipmentManager.instance.GetDataFromName (equipmentName));
		}
	}

	//launches character in a direction
	public void Swipe (Vector2 dir) {
		if (inVehicle) {
			ExitVehicle ();
		}
		rb.constraints = RigidbodyConstraints.None;
		rb.AddForce (new Vector3(dir.x, dir.magnitude * verticalFactor, dir.y) * swipeForce);
		state = MovementState.Jumping;
		nextAutoReset = Time.time + autoResetTime; //so you can't get stuck in jumping state

		if (jetpack != null) {
			jetpack.Launch ();
		}
	}

	void EnterVehicle(Rideable newVehicle) {
		currentVehicle = newVehicle;
		rb.interpolation = RigidbodyInterpolation.None;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		currentVehicle.Mount (gameObject);
		state = MovementState.Grounded;

		shooting.canRotateParent = false;
		shooting.gameObject.SetActive (false);

		curPickingupTimers.Clear ();
	}

	public void ExitVehicle() {
		if (rb != null) {
			rb.interpolation = RigidbodyInterpolation.Extrapolate;
			rb.constraints = RigidbodyConstraints.FreezeRotation;
			rb.velocity = currentVehicle.GetComponent<Rigidbody> ().velocity;
		}

		state = MovementState.Jumping;

		currentVehicle.Dismount ();
		currentVehicle = null;

		shooting.canRotateParent = true;
		shooting.gameObject.SetActive (true);

		health.ResetColor ();
	}

	void OnCollisionEnter(Collision other) {
		if (other.collider.tag == "Vehicle") {
			Rideable newVehicle = other.gameObject.GetComponentInParent<Rideable> ();
			if(newVehicle != null && newVehicle.canBeMounted) {
				EnterVehicle (newVehicle); //enter vehicle when you hit something tagged with vehicle
			}
		} else {
			//changes states when hit
			if (state == MovementState.Jumping) {
				shooting.canRotateParent = false;
				state = MovementState.Tumbling;
			}
		}
	}

	public void Hit() {
		nextAutoReset = Time.time + autoResetTime;
		rb.constraints = RigidbodyConstraints.None;
		shooting.canRotateParent = false;
	}

	//takes enemy weapon if you don't have one
	public bool TrySwapWeapons(WeaponData weaponData) {
		if (shooting.hasWeapon) {
			return false;
		}

		shooting.SetWeapon (weaponData);
		return true;
	}

	//rotates in direction of swipe unless an enemy is in range
	public void TryRotateInDir(Vector2 dir) {
		if (shooting.targetInRange) {
			return;
		}

		float angle = Mathf.Atan2 (dir.x, dir.y) * Mathf.Rad2Deg;
		shooting.OverrideRotateParent (angle);
	}

	void LateUpdate() {
		if (state == MovementState.Tumbling || (state == MovementState.Jumping && Time.time > nextAutoReset)) {
			Vector2 groundSpeed = new Vector2 (rb.velocity.x, rb.velocity.z);
			if (groundSpeed.magnitude < stoppingSpeed) { //stop when the player is slow enough
				Stop ();
			}
		}

		if (state == MovementState.Grounded && !inVehicle && Time.time > nextAutoReset) { //stand up
			if (!shooting.canRotateParent) {
				Stop ();
			}
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), turnSpeed * Time.deltaTime);
		}

		DisplayPickupTimer ();
	}

	//once player has slowed down enough, reset for next swipe
	void Stop() {
		state = MovementState.Grounded;
		rb.velocity = Vector3.zero;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		shooting.canRotateParent = true;
	}

	public void Die() {
		if (inVehicle) {
			return;
		}
		rb.constraints = RigidbodyConstraints.None;
		shooting.Die ();
		Destroy (this);

		GameManager.instance.GameOver ();
	}

	// picking up weapons
	void OnTriggerEnter (Collider coll) {
		if (!inVehicle && coll.gameObject.tag == "Pickup") {
			// new pickup enter range
			curPickingupTimers.Add (new PickupTimer (coll.gameObject, pickupTime));
		}
	}

	void OnTriggerExit (Collider coll) {
		if (!inVehicle && coll.gameObject.tag == "Pickup") {
			// pickup exit range
			List<PickupTimer> exitedTimers = new List<PickupTimer>();
			foreach (var pickupTimer in curPickingupTimers) {
				if (pickupTimer.pickup == coll.gameObject) {
					exitedTimers.Add (pickupTimer);
				}
			}

			// remove exited timers
			foreach (var exitedTimer in exitedTimers) {
				curPickingupTimers.Remove (exitedTimer);
			}
		}
	}

	void Update () {
		if (curPickingupTimers.Count == 0) {
			return;
		}

		foreach (var pickupTimer in curPickingupTimers) {
			pickupTimer.timer -= Time.deltaTime;
			if (pickupTimer.timer <= 0f) { //if a timer has run out, pick it up
				Pickup (pickupTimer);
				curPickingupTimers.Remove (pickupTimer);
				return;
			}
		}
	}

	void SwitchWeapon(WeaponData data) {
		if (shooting.hasWeapon) {
			ThrowPickup (shooting.GetWeaponData().ToAssetData());
		}
		shooting.SetWeapon (data as WeaponData);
	}

	void SwitchEquipment(EquipmentData data) {
		int index = (int)data.type;

		if (equipmentObjects[index] != null) {
			RemoveEquipmentBuff (equipment[index]);
			ThrowPickup (equipment[index]);
			Destroy (equipmentObjects [index]);
		}

		equipment [index] = data;

		GameObject newEquipment = (GameObject) Instantiate (data.prefab, transform.position, transform.rotation, transform);
		equipmentObjects [index] = newEquipment;
		ApplyEquipmentBuff (data);

		health.UpdateRenderersNextFrame ();
	}

	void RemoveEquipmentBuff(EquipmentData data) {
		switch (data.name) {
			case "Body Armor":
				health.maxHealth -= 20f;
				health.health = Mathf.Min (health.health, health.maxHealth);
				break;
			case "Jetpack":
				verticalFactor -= 1.5f;
				jetpack = null;
				break;
		}
	}

	void ApplyEquipmentBuff(EquipmentData data) {
		switch (data.name) {
			case "Body Armor":
				health.maxHealth += 20f;
				break;
			case "Jetpack":
				verticalFactor += 1.5f;
				jetpack = equipmentObjects [(int)data.type].GetComponent<Jetpack> ();
				break;
		}
	}

	//picks up object associated with this timer
	void Pickup (PickupTimer timer) {
		foreach (var pickupTimer in curPickingupTimers) {
			if (pickupTimer != timer) {
				pickupTimer.ResetTimer ();
			}
		}

		if (timer.type == PickupTimer.Type.Drop) {
			Pickup drop = timer.pickup.GetComponent<Pickup> ();
			if (drop.data.GetAssetType () == "Weapon") {
				SwitchWeapon (drop.data as WeaponData);
			} else if (drop.data.GetAssetType() == "Equipment") {
				SwitchEquipment (drop.data as EquipmentData);
			}
			Destroy (timer.pickup);
		} else if (timer.type == PickupTimer.Type.Objective) {
			LevelProgressManager.instance.CompleteObjective ();
			EscortController escort = timer.pickup.GetComponentInParent<EscortController> ();
			if (escort == null) {
				Destroy (timer.pickup.GetComponent<Collider> ());
			} else {
				escort.Enable ();
			}
		}
	}

	//display timer information
	void DisplayPickupTimer() {
		timerDisplay.SetActive (curPickingupTimers.Count > 0);

		if (curPickingupTimers.Count == 0) {
			return;
		}

		timerDisplay.transform.position = transform.position;

		nextPickupText.text = curPickingupTimers [0].name;
		timerBar.transform.localScale = new Vector3 (1f - curPickingupTimers[0].percentage, 1f, 1f);
	}

	void ThrowPickup (Data data) {
		Vector3 pos = transform.TransformPoint(0f, throwHeight, 1f);
		GameObject newPickup = Instantiate (pickupPrefab, pos, Quaternion.identity);
		newPickup.GetComponent<Pickup> ().Init (data, true);
		newPickup.GetComponent<Rigidbody>().velocity = transform.forward * throwVelocity;
	}

	public class PickupTimer {
		public enum Type {
			Drop,
			Objective
		};

		public Type type;

		public GameObject pickup;
		public float timer;
		public string name;

		float originalTime;

		public PickupTimer (GameObject _pickup, float _timer) {
			pickup = _pickup;
			timer = _timer;
			originalTime = _timer;

			DetermineType();
		}

		//setup timer based on object
		void DetermineType() {
			Pickup weaponPickup = pickup.GetComponent<Pickup> ();
			if (weaponPickup != null) {
				name = weaponPickup.data.name;
				type = Type.Drop;
				return;
			}

			//if we add costume pickups, check for it here
			name = pickup.name;
			type = Type.Objective;
		}

		public void ResetTimer() {
			timer = originalTime;
		}

		public float percentage { get { return timer / originalTime; } }
	}
}
