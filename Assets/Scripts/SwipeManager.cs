using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls all swipe input and sends it to the player
public class SwipeManager : MonoBehaviour {
	public static SwipeManager instance;

	public float maxSwipeDistance;
	public float swipeCancelRange;

	public bool autoSwiping;

	[Header("UI")]
	public RectTransform joystickVisual;

	Camera gameCam;
	PlayerController player;
	LineRenderer swipeLine;

	LineManager lm;

	bool isTapping;
	bool isSelectingPlayer;
	Vector2 startPos;

	float timer;

	void Awake() {
		instance = this;

		gameCam = GameObject.FindObjectOfType<Camera> ();
		player = GameObject.FindObjectOfType<PlayerController> ();
		swipeLine = player.GetComponentInChildren<LineRenderer> ();
		lm = player.GetComponentInChildren<LineManager> ();

		swipeLine.enabled = false;
		joystickVisual.gameObject.SetActive (false);
	}

	void Update() {
		//on tap down
		if (!isTapping && Input.GetMouseButtonDown (0) && !TimeManager.isPaused) {
			StartTap (Input.mousePosition);
		}

		//on tap up
		if (isTapping && Input.GetMouseButtonUp (0)) {
			EndTap (Input.mousePosition);
		}

		if (timer > 0) {
			timer -= Time.deltaTime;
		}
	}

	void LateUpdate() {
		if (isTapping) {
			if (isSelectingPlayer) {
				Vector2 dir = CalculateDirection (Input.mousePosition);
				player.TryRotateInDir (dir.normalized);
				UpdateLine (Input.mousePosition);

				if (autoSwiping && CanLaunch(dir)) {
					player.Swipe (dir);
				}
			} else if (player.inVehicle) {
				UpdateVehicle (Input.mousePosition);
			}
		}
	}

	void StartTap(Vector2 curPos) {
		//check whether or not we are selecting the player
		if (!player.inVehicle) {
			// not in vehicle
			isSelectingPlayer = true;
		} else {
			// in vehicle
			if (!player.currentVehicle.dismountable) {
				// cannot leave vehicle
				isSelectingPlayer = false;
			} else {
				// can leave vehicle
				RaycastHit[] hitInfos = Physics.RaycastAll (gameCam.ScreenPointToRay (curPos), 100f, 1 << 2);

				isSelectingPlayer = false;
				foreach (RaycastHit hitInfo in hitInfos) {
					if(hitInfo.collider != null && hitInfo.collider.gameObject.name == "InteractionSphere") {
						isSelectingPlayer = true;
						break;
					}
				}
			}
		}

		bool startTap = false;
		if (player.inVehicle) {
			// in vehicle
			if (player.currentVehicle.controllable && !isSelectingPlayer) {
				startTap = true;
			}
			if (isSelectingPlayer) {
				startTap = true;
			}
		} else {
			startTap = true;
		}

		if (startTap) {
			isTapping = true;
			startPos = curPos; //save initial tap position

			joystickVisual.gameObject.SetActive (true);
			joystickVisual.position = startPos;

			if (isSelectingPlayer) {
				swipeLine.enabled = true;
			}
		}
	}

	public void Pop() {
		if(autoSwiping) {
			timer = 0.1f; //wait a bit before you can swipe again
		}
	}

	//update LineRenderer
	void UpdateLine(Vector2 curPos) {
		Vector2 dir = CalculateDirection (curPos);

		swipeLine.enabled = (dir.magnitude > swipeCancelRange);

		Color lineColor = (player.state == PlayerController.MovementState.Grounded) ? Color.white : new Color(1, 1, 1, 0.125f);
		lm.line.material.color = lineColor;

		lm.UpdateLineTrajectory(dir);
	}

	void UpdateVehicle(Vector2 curPos) {
		if (player.currentVehicle is Vehicle) {
			(player.currentVehicle as Vehicle).targetDirection = CalculateDirection (curPos);
		} else if (player.currentVehicle is Helicopter) {
			(player.currentVehicle as Helicopter).targetDirection = CalculateDirection (curPos);
		}
	}

	void EndTap(Vector2 endPos) {
		joystickVisual.gameObject.SetActive (false);
		swipeLine.enabled = false;
		isTapping = false;

		if (player.inVehicle) {
			if (player.currentVehicle is Vehicle) {
				(player.currentVehicle as Vehicle).targetDirection = Vector2.zero;
			} else if (player.currentVehicle is Helicopter) {
				(player.currentVehicle as Helicopter).targetDirection = Vector2.zero;
			}
		}

		Vector2 dir = CalculateDirection (endPos);

		if (CanLaunch(dir)) {
			player.Swipe (dir);
		}

		isSelectingPlayer = false;
	}

	bool CanLaunch (Vector2 dir) {
		return isSelectingPlayer && player.state == PlayerController.MovementState.Grounded && dir.magnitude > swipeCancelRange && timer <= 0f;
	}

	Vector2 CalculateDirection(Vector2 curPos) {
		Vector2 dir = curPos - startPos;
		dir = RotateVector (dir, -gameCam.transform.rotation.eulerAngles.y); //rotates the vector so that it aligns with the world angle
		dir /= (Screen.height / 2); //scale based on screen size

		if (dir.magnitude > maxSwipeDistance) {
			dir = dir.normalized * maxSwipeDistance;
		}

		return dir;
	}

	//rotates a Vector2 around the origin by an angle (in degrees)
	Vector2 RotateVector(Vector2 originalVector, float angle) {
		float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
		float cos = Mathf.Cos(angle * Mathf.Deg2Rad);

		Vector2 newVector = new Vector2();
		newVector.x = (cos * originalVector.x) - (sin * originalVector.y);
		newVector.y = (sin * originalVector.x) + (cos * originalVector.y);
		return newVector;
	}
}