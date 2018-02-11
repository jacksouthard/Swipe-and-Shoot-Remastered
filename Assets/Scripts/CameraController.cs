using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float speed;

	Transform player;
	Vector3 offset;

	void Awake() {
		player = GameObject.FindObjectOfType<PlayerController> ().transform;
		offset = transform.position - player.position;
	}

	void LateUpdate() {
		transform.position = Vector3.Lerp (transform.position, player.position + offset, Time.deltaTime * speed);
	}
}
