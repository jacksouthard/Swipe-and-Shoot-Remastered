using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public static CameraController instance;

	[Header("Constriants")]
	public float radius;
	public Vector2 center;
	public float heightMinOffset;
	public float heightMaxOffset;

	[Header("Speed")]
	public float speed;

	Transform player;
	Vector3 offset;
	bool isActive = true;

	void Awake() {
		instance = this;

		player = GameObject.FindObjectOfType<PlayerController> ().transform;
		offset = transform.position;
	}

	public void ResetPosition() {
		Vector3 targetPos = CalculateTargetPos (player.position);

		transform.position = targetPos + offset;
	}

	public IEnumerator ShowTarget(Transform target) {
		isActive = false;
		Vector3 startingPos = transform.position;
		Vector3 targetPos = CalculateTargetPos (target.position);

		float p = 0;
		while(p < 1f) {
			yield return new WaitForSecondsRealtime(Time.fixedUnscaledDeltaTime);
			transform.position = Vector3.Lerp (startingPos, targetPos + offset, p);
			p += Time.fixedUnscaledDeltaTime * 2f;
		}
	}

	public void Resume() {
		isActive = true;
	}

	void LateUpdate() {
		if (!isActive) {
			return;
		}

		Vector3 targetPos = CalculateTargetPos (player.position);

		transform.position = Vector3.Lerp (transform.position, targetPos + offset, Time.deltaTime * speed);
	}

	Vector3 CalculateTargetPos(Vector3 position) {
		Vector2 targetPos2d = new Vector2 (position.x, position.z);
		Vector2 diff = targetPos2d - center;
		float curRadius = diff.magnitude;
		if (curRadius > radius) {
			// outside of allowed zone
			targetPos2d = (diff.normalized * radius) + center;
		}

		float height = Mathf.Clamp (position.y, heightMinOffset, heightMaxOffset);
		return new Vector3 (targetPos2d.x, height, targetPos2d.y);
	}

	public static bool PositionIsInView(Vector3 position) {
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint (position);
		return viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1 && viewportPoint.z > 0;
	}
}
