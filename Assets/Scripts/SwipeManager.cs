using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls all swipe input and sends it to the player
public class SwipeManager : MonoBehaviour {
	public float maxSwipeDistance;
	public float swipeCancelRange;

	Camera gameCam;
	PlayerController player;
	LineRenderer swipeLine;

	LineManager lm;

	bool isTapping;
	bool isSelectingPlayer;
	Vector2 startPos;

	void Awake() {
		gameCam = GameObject.FindObjectOfType<Camera> ();
		player = GameObject.FindObjectOfType<PlayerController> ();
		swipeLine = player.GetComponentInChildren<LineRenderer> ();
		lm = GameObject.Find("Player").GetComponentInChildren<LineManager> ();
	}

	void Update() {
		//on tap down
		if (player.state == PlayerController.MovementState.Grounded && !isTapping && Input.GetMouseButtonDown (0)) {
			StartTap (Input.mousePosition);
		}

		//on tap up
		if (isTapping && Input.GetMouseButtonUp (0)) {
			EndTap (Input.mousePosition);
		}
	}

	void LateUpdate() {
		if (isTapping) {
			if (isSelectingPlayer) {
				UpdateLine (Input.mousePosition);
			} else if (player.inVehicle) {
				UpdateVehicle (Input.mousePosition);
			}
		}
	}

	void StartTap(Vector2 curPos) {
		isTapping = true;
		startPos = curPos; //save initial tap position

		//check whether or not we are selecting the player
		if (!player.inVehicle) {
			isSelectingPlayer = true;
		} else {
			RaycastHit hitInfo;
			Physics.Raycast (gameCam.ScreenPointToRay (curPos), out hitInfo, 100f, 1 << 2);

			isSelectingPlayer = (hitInfo.collider != null && hitInfo.collider.gameObject.name == "InteractionSphere");
		}

		if (isSelectingPlayer) {
			swipeLine.enabled = true;
		}
	}

	//update LineRenderer
	void UpdateLine(Vector2 curPos) {
		Vector2 dir2d = CalculateDirection (curPos);

		lm.UpdateLineTrajectory(dir2d);
	}

	void UpdateVehicle(Vector2 curPos) {
		player.currentVechicle.targetDirection = CalculateDirection (curPos);
	}

	void EndTap(Vector2 endPos) {
		swipeLine.enabled = false;
		isTapping = false;

		if (player.inVehicle) {
			player.currentVechicle.targetDirection = Vector2.zero;
		}

		Vector2 dir = CalculateDirection (endPos);

		if (isSelectingPlayer && dir.magnitude > swipeCancelRange) {
			player.Swipe (dir);
		}

		isSelectingPlayer = false;
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