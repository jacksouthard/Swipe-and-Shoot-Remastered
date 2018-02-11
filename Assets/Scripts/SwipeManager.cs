﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls all swipe input and sends it to the player
public class SwipeManager : MonoBehaviour {
	public float lineLength;

	Camera gameCam;
	PlayerController player;
	LineRenderer swipeLine;

	bool isTapping;
	Vector2 startPos;

	void Awake() {
		gameCam = GameObject.FindObjectOfType<Camera> ();
		player = GameObject.FindObjectOfType<PlayerController> ();
		swipeLine = player.GetComponentInChildren<LineRenderer> ();
	}

	void Update() {
		//on tap down
		if (player.state == PlayerController.MovementState.Grounded && !isTapping && Input.GetMouseButtonDown (0)) {
			StartSwipe (Input.mousePosition);
		}

		//on tap up
		if (isTapping && Input.GetMouseButtonUp (0)) {
			EndSwipe (Input.mousePosition);
		}
	}

	void LateUpdate() {
		if (isTapping) {
			UpdateLine (Input.mousePosition);
		}
	}

	void StartSwipe(Vector2 curPos) {
		swipeLine.enabled = true;

		isTapping = true;
		startPos = curPos; //save initial tap position
	}

	//update LineRenderer
	void UpdateLine(Vector2 curPos) {
		Vector2 dir2d = CalculateDirection (curPos) * lineLength;
		Vector3 dir3d = new Vector3 (dir2d.x, 0f, dir2d.y);

		Vector3 startPos = player.transform.position;
		Vector3 endPos = startPos + dir3d;

		swipeLine.SetPosition (0, startPos);
		swipeLine.SetPosition (1, endPos);
	}

	void EndSwipe(Vector2 endPos) {
		swipeLine.enabled = false;
		isTapping = false;

		Vector2 dir = CalculateDirection (endPos);
		player.Swipe (dir);
	}

	Vector2 CalculateDirection(Vector2 curPos) {
		Vector2 dir = curPos - startPos;
		dir = RotateVector (dir, -gameCam.transform.rotation.eulerAngles.y); //rotates the vector so that it aligns with the world angle
		dir.Normalize();

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