using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[Header("Constriants")]
	public float radius;
	public Vector2 center;
	public float heightMinOffset;
	public float heightMaxOffset;

	[Header("Speed")]
	public float speed;

	Transform player;
	Vector3 offset;

	void Awake() {
		player = GameObject.FindObjectOfType<PlayerController> ().transform;
		offset = transform.position - player.position;
	}

	public void ResetPosition() {
		Vector3 targetPos = CalculateTargetPos ();

		transform.position = targetPos + offset;
	}

	void LateUpdate() {
		Vector3 targetPos = CalculateTargetPos ();

		transform.position = Vector3.Lerp (transform.position, targetPos + offset, Time.deltaTime * speed);
	}

	Vector3 CalculateTargetPos() {
		Vector2 playerPos2D = new Vector2 (player.position.x, player.position.z);
		Vector2 diff = playerPos2D - center;
		float curRadius = diff.magnitude;
		if (curRadius > radius) {
			// outside of allowed zone
			playerPos2D = (diff.normalized * radius) + center;
		}

		float height = Mathf.Clamp (player.position.y, heightMinOffset, heightMaxOffset);
		return new Vector3 (playerPos2D.x, height, playerPos2D.y);
	}
}
