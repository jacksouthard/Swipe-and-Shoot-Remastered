using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EdgeView : MonoBehaviour {
	GameObject target;

	const float margin = 15f;	// margin from edge of screen
	float edgeOffset;
	float BEdge;
	float TEdge;
	float LEdge;
	float REdge;

	// constants storing screen corner angles (T = top, R = right, B = bottom, L = left)
	float TRAngle;
	float BRAngle;
	float BLAngle;
	float TLAngle;

	float canvasWidth;
	float canvasHeight;

	private float distanceToTarget;

	Transform center;
	RectTransform canvas;
	MeshRenderer targetRend;
	GameObject image;

	public void Init(GameObject newTarget) {
		target = newTarget;
		targetRend = target.GetComponentInChildren<MeshRenderer> ();
	}

	void Start () {
		center = GameObject.FindObjectOfType<CameraController>().transform;
		canvas = transform.parent.GetComponent<RectTransform>();
		image = transform.GetComponentInChildren<Image> ().gameObject;

		edgeOffset = margin + (this.GetComponent<RectTransform>().rect.width / 2);

		canvasWidth = canvas.rect.width;
		canvasHeight = canvas.rect.height;

		BEdge = -canvasHeight / 2 + edgeOffset;
		TEdge =  canvasHeight / 2 - edgeOffset;
		LEdge = -canvasWidth  / 2 + edgeOffset;
		REdge =  canvasWidth  / 2 - edgeOffset;

		// calculate angles of the 4 corners of the screen
		TRAngle = Mathf.Atan(REdge / TEdge) 						* Mathf.Rad2Deg;
		BRAngle = (Mathf.PI + Mathf.Atan(REdge / BEdge)) 			* Mathf.Rad2Deg;
		BLAngle = (Mathf.PI + Mathf.Atan(LEdge / BEdge))		 	* Mathf.Rad2Deg;
		TLAngle = ((2 * Mathf.PI) + Mathf.Atan(LEdge / TEdge)) 		* Mathf.Rad2Deg;
	}
	
	void Update () {
		SetPositionAndRotation();
		UpdateVisibility();
	}

	void SetPositionAndRotation () {
		Vector3 targetVector = (target.transform.position - center.position);
		float angle = CalculateAngle (transform.forward, targetVector);

		// position
		Vector2 position = Vector2.zero;

		// assign edgeView to 1 of 4 edges (Top, Right, Bottom, Left)
		if (angle >= TLAngle || angle <= TRAngle) {
			// top
			position.y = TEdge;
			position.x = TEdge * Mathf.Tan((angle) * Mathf.Deg2Rad);

		} else if (angle > TRAngle && angle < BRAngle) {
			// right
			position.x = REdge;
			position.y = REdge * Mathf.Tan((90 - angle) * Mathf.Deg2Rad);
		} else if (angle >= BRAngle && angle <= BLAngle) {
			// bottom
			position.y = BEdge;
			position.x = BEdge * Mathf.Tan((angle) * Mathf.Deg2Rad);
		} else if (angle > BLAngle && angle < TLAngle) {
			// left
			position.x = LEdge;
			position.y = LEdge * Mathf.Tan((270 - angle) * Mathf.Deg2Rad);
		}

		transform.localPosition = position;

		// rotate
	
		Quaternion targetRotation = Quaternion.Euler(0, 0, -angle);
		transform.rotation = targetRotation;

	}

	void UpdateVisibility () {
		image.SetActive (!targetRend.isVisible);
	}

	//From https://gist.github.com/shiwano/0f236469cd2ce2f4f585
	public static float CalculateAngle(Vector3 from, Vector3 to) {
		return Quaternion.FromToRotation(from, to).eulerAngles.y;
	}
}
