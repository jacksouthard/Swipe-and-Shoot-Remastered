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
	Equipment[] equipment = new Equipment[3]; //there are 3 types of equipment
	public Transform equipmentParent;

	[Header("Picking Up")]
	public float pickupTime;
	public GameObject timerDisplay;
	public GameObject timerBar;
	GameObject weaponStatsParent;
	Text pickupTitleText;
	Text dpsText;
	Text rangeText;

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
	Transform audioListener;

	void Awake() {
		rb = gameObject.GetComponent<Rigidbody> ();
		health = gameObject.GetComponent<Health> ();

		shooting = gameObject.GetComponentInChildren<ShootingController> ();
		rb.interpolation = RigidbodyInterpolation.Extrapolate;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		audioListener = gameObject.GetComponentInChildren<AudioListener> ().transform;
		audioListener.parent = null; //unparent audiolistener
		audioListener.rotation = Quaternion.Euler(0, GameObject.FindObjectOfType<Camera>().transform.rotation.eulerAngles.y, 0); //rotation is based on camera rotation

		timerDisplay.transform.parent = null; //timer moves independently from player

		health.onDeath += Die;
		health.onHit += Hit;
	}

	void Start() {
		InitPickupDisplay ();

		//set starting weapon
		string weaponToUse = (LevelProgressManager.lastWeaponName != "None") ? LevelProgressManager.lastWeaponName : defaultWeaponName;
		if (weaponToUse != "None") {
			shooting.SetWeapon (WeaponManager.instance.GetDataFromName (weaponToUse));
		}

		foreach (string equipmentName in startingEquipment) {
			SwitchEquipment (EquipmentManager.instance.GetDataFromName (equipmentName));
		}

		Instantiate (CostumeManager.instance.GetDataFromName(GameManager.instance.levelData.GetCharacterName()), transform.position, transform.rotation, transform);
	}

	void InitPickupDisplay() {
		Transform canvas = timerDisplay.GetComponentInChildren<Canvas> ().transform;
		pickupTitleText = canvas.Find ("Name").GetComponent<Text> ();

		weaponStatsParent = canvas.transform.Find ("WeaponStats").gameObject;
		dpsText = weaponStatsParent.transform.Find ("DPS").GetComponent<Text>();
		rangeText = weaponStatsParent.transform.Find ("Range").GetComponent<Text> ();
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

		foreach (Equipment data in equipment) {
			if (data != null) {
				data.OnJump ();
			}
		}
	}

	void EnterVehicle(Rideable newVehicle) {
		health.ResetColor ();

		currentVehicle = newVehicle;
		rb.interpolation = RigidbodyInterpolation.None;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		currentVehicle.Mount (gameObject);
		state = MovementState.Grounded;

		SwipeManager.instance.EndSwipe ();

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

	public void Hit(float damage) {
//		nextAutoReset = Time.time + autoResetTime;
//		rb.constraints = RigidbodyConstraints.None;
//		shooting.canRotateParent = false;

		foreach (Equipment data in equipment) {
			if (data != null) {
				data.OnHit (damage);
			}
		}
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
		if (shooting.targetInRange || dir.magnitude == 0) {
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
		audioListener.position = transform.position;
	}

	//once player has slowed down enough, reset for next swipe
	void Stop() {
		state = MovementState.Grounded;
		rb.velocity = Vector3.zero;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		shooting.canRotateParent = true;

		SwipeManager.instance.Pop ();
	}

	public void Die() {
		if (inVehicle) {
			return;
		}
		rb.constraints = RigidbodyConstraints.None;
		shooting.Die ();
		SwipeManager.instance.EndSwipe ();
		Destroy (this);

		GameManager.instance.GameOver ("you died");
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
		int index = (int)data.slot;

		if (equipment[index] != null) {
			equipment [index].Remove ();
			ThrowPickup (equipment[index].data);
		}
			
		GameObject newEquipment = (GameObject) Instantiate (data.prefab, transform.position, transform.rotation, transform);
		equipment [index] = newEquipment.GetComponent<Equipment>();
		equipment [index].Init (newEquipment, data);

		health.UpdateRenderersNextFrame ();
	}

	public void RemoveEquipment(int index) {
		equipment [index].Remove ();
		health.UpdateRenderersNextFrame ();
	}

	//picks up object associated with this timer
	void Pickup (PickupTimer timer) {
		foreach (var pickupTimer in curPickingupTimers) {
			if (pickupTimer != timer) {
				pickupTimer.ResetTimer ();
			}
		}

		Pickup drop = timer.pickup.GetComponent<Pickup> ();
		if (drop != null) {
			if (drop.data.GetAssetType () == "Weapon") {
				SwitchWeapon (drop.data as WeaponData);
			} else if (drop.data.GetAssetType () == "Equipment") {
				SwitchEquipment (drop.data as EquipmentData);
			}
			Destroy (timer.pickup);
		}

		if (timer.type == PickupTimer.Type.Objective) {
			LevelProgressManager.instance.CompleteObjective ();

			if (drop == null) {
				EscortController escort = timer.pickup.GetComponentInParent<EscortController> ();
				if (escort == null) {
					Destroy (timer.pickup.GetComponent<Collider> ());
				} else {
					escort.Enable ();
				}
			}
		}

		pickupTitleText.text = ""; //reset text
	}

	//display timer information
	void DisplayPickupTimer() {
		timerDisplay.SetActive (curPickingupTimers.Count > 0);

		if (curPickingupTimers.Count == 0) {
			return;
		}

		timerDisplay.transform.position = transform.position;

		timerBar.transform.localScale = new Vector3 (1f - curPickingupTimers[0].percentage, 1f, 1f);

		if (pickupTitleText.text == curPickingupTimers [0].name) { //don't update if the pickup is the same
			return;
		}

		pickupTitleText.text = curPickingupTimers [0].name;
		if (shooting.hasWeapon) {
			Pickup pickup = curPickingupTimers [0].pickup.GetComponent<Pickup> ();
			if (pickup != null && pickup.data is WeaponData) {
				WeaponData newWeaponData = curPickingupTimers [0].pickup.GetComponent<Pickup> ().data as WeaponData;
				WeaponComparisonData comparisonStats = shooting.GetWeaponData ().Compare (newWeaponData);
				dpsText.text = Mathf.RoundToInt (comparisonStats.dpsPercentageDiff * 100) + "%";
				dpsText.color = WeaponComparisonData.GetColorFromPercentage (comparisonStats.dpsPercentageDiff);
				rangeText.text = Mathf.RoundToInt (comparisonStats.rangePercentageDiff * 100) + "%";
				rangeText.color = WeaponComparisonData.GetColorFromPercentage (comparisonStats.rangePercentageDiff);
				weaponStatsParent.SetActive (true);
				return;
			}
		}

		weaponStatsParent.SetActive (false);
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
			Pickup newPickup = pickup.GetComponent<Pickup> ();
			if (newPickup != null) {
				name = newPickup.data.name;
				type = (!newPickup.isObjective) ? Type.Drop : Type.Objective;
				return;
			}

			//if we add costume pickups, check for it here
			name = pickup.name;
			type = Type.Objective; //we assume any non-drop pickups are objectives for now
		}

		public void ResetTimer() {
			timer = originalTime;
		}

		public float percentage { get { return timer / originalTime; } }
	}
}
