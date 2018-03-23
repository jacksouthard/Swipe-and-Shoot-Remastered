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

	bool hasWorldIndicator;
	bool destroysOnDeath;

	Camera gameCam;
	Vector3 offset; //offset from center
	RectTransform canvas;
	GameObject image;
	GameObject worldIndicator;

	static GameObject screenIndicatorPrefab;
	static GameObject worldIndicatorPrefab;
	static bool hasSetPrefabs = false;

	public static EdgeView Create(GameObject _newTarget, bool _hasWorldIndicator, bool _destroysOnDeath = true) {
		EdgeView edgeView = Create (_destroysOnDeath);
		edgeView.SetTarget (_newTarget, _hasWorldIndicator);
		return edgeView;
	}

	public static EdgeView Create(bool _destroysOnDeath = true) {
		if (!hasSetPrefabs) {
			LoadPrefab ();
		}

		GameObject edgeViewObj = Instantiate (screenIndicatorPrefab, GameManager.instance.transform.parent.GetComponentInChildren<Canvas>().transform); //always use the canvas in LevelAssets
		EdgeView edgeView = edgeViewObj.GetComponent<EdgeView> ();
		edgeView.Init (_destroysOnDeath);

		return edgeView;
	}

	static void LoadPrefab() {
		screenIndicatorPrefab = Resources.Load ("EdgeView_Screen") as GameObject;
		worldIndicatorPrefab = Resources.Load ("EdgeView_World") as GameObject;
	}

	public void Init(bool _destroysOnDeath) {
		gameCam = GameObject.FindObjectOfType<Camera>();
		canvas = transform.parent.GetComponent<RectTransform>(); 
		image = transform.GetComponentInChildren<Image> ().gameObject;
		destroysOnDeath = _destroysOnDeath;
	}

	public void SetTarget(GameObject _newTarget, bool _hasWorldIndicator) {
		target = _newTarget;

		offset = gameCam.transform.forward * Mathf.Sqrt (2) * (gameCam.transform.position.y - target.transform.position.y); //gets point in the center of the camera's view
		hasWorldIndicator = _hasWorldIndicator;

		SetUpWorldIndicator ();

		//try to get the health component
		if (destroysOnDeath) {
			Health health = target.GetComponent<Health> ();
			if (health != null) {
				health.onDeath += Destroy;
			}
		}

		enabled = true;
	}

	void SetUpWorldIndicator() {
		if (hasWorldIndicator) {
			if (worldIndicator == null) {
				worldIndicator = (GameObject)Instantiate (worldIndicatorPrefab);
			}

			worldIndicator.SetActive (true);
			worldIndicator.transform.position = target.transform.position;
		} else {
			if (worldIndicator != null) {
				worldIndicator.SetActive (false);
			}
		}
	}

	public void Hide() {
		image.SetActive (false);
		if (worldIndicator != null) {
			worldIndicator.SetActive (false);
		}
		enabled = false;
	}

	public void Destroy() {
		if (worldIndicator != null) {
			Destroy (worldIndicator);
		}

		Destroy (gameObject);
	}

	void Start () {
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
		if (target == null && destroysOnDeath) {
			this.Destroy ();
			return;
		}

		SetPositionAndRotation();
		UpdateVisibility();
	}

	void SetPositionAndRotation () {
		Vector3 targetVector = (target.transform.position - (gameCam.transform.position + offset));
		float angle = CalculateAngle (transform.forward, targetVector) - gameCam.transform.rotation.eulerAngles.y; //rotate based on camera's rotation

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
		bool targetIsOnScreen = CameraController.PositionIsInView (target.transform.position);
		image.SetActive (!targetIsOnScreen);
		if (worldIndicator != null) {
			worldIndicator.SetActive (hasWorldIndicator && targetIsOnScreen);
		}
	}

	//From https://gist.github.com/shiwano/0f236469cd2ce2f4f585
	public static float CalculateAngle(Vector3 from, Vector3 to) {
		return Quaternion.FromToRotation(from, to).eulerAngles.y;
	}
}
