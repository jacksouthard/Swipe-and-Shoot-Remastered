using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour {
	public enum Type {
		Player,
		Enemy,
		Object,
		Vehicle
	}
	[HideInInspector]
	public Type type;

	public enum State
	{
		Alive,
		Dying,
		Decaying
	};
	[HideInInspector]
	public State state = State.Alive;

	[Header("Stats")]
	public float maxHealth;

	[Header("Regen")]
	public float regenSpeed;
	public float regenWait;

	[Header("Hit")]
	public float hitWait;
	public Color hitColor;

	[Header("Death")]
	public float dyingTimer;
	public float decayTimer;

	[Header("Debug")]
	public float waitTimer;
	public float health;

	bool shouldUpdateRenderers;
	List<MeshRenderer> mrs;
	List<Color[]> originalColors;

	// regening
	static GameObject regenEffectPrefab;
	bool regening = false;
	GameObject curRegenEffect;

	void Start () {
		if (regenEffectPrefab == null) {
			regenEffectPrefab = Resources.Load ("RegenEffect") as GameObject;
		}

		health = maxHealth;
		UpdateRenderers ();

		if (GetComponent<PlayerController> () != null) {
			type = Type.Player;
		} else if (GetComponent<EnemyController> () != null) {
			type = Type.Enemy;
		} else if (this is VehicleHealth) {
			type = Type.Vehicle;
		} else {
			type = Type.Object;
		}
	}

	//waits until next frame before updating MeshRenderers
	public void UpdateRenderersNextFrame() {
		shouldUpdateRenderers = true;
	}

	//updates list of all MeshRenderers - call whenever a child object is added/removed
	void UpdateRenderers() {
		shouldUpdateRenderers = false;

		List<MeshRenderer> originalMrs = mrs;
		mrs = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());

		if (originalMrs != null) {
			//reset any old meshrenderers
			for (int i = 0; i < originalMrs.Count; i++) {
				if (mrs.Contains (originalMrs [i])) {
					for (int j = 0; j < originalColors [i].Length; j++) {
						mrs [mrs.IndexOf (originalMrs [i])].materials [j].color = originalColors [i] [j];
					}
				}
			}
		}

		originalColors = new List<Color[]> ();

		for(int i = 0; i < mrs.Count; i++) {
			Color[] colors = new Color[mrs[i].materials.Length];
			for (int j = 0; j < colors.Length; j++) {
				colors [j] = mrs [i].materials [j].color;
			}
			originalColors.Add(colors);
		}
	}
	
	void Update () {
		if (state == State.Alive) {
			if (!regening) {
				if (health < maxHealth) {
					if (waitTimer <= 0f) {
						// start regening
						StartRegen ();
					} else {
						// incriment timer
						waitTimer -= Time.deltaTime;
					}
				}
			} else {
				health += regenSpeed * Time.deltaTime;
				if (health > maxHealth) {
					EndRegen();
					health = maxHealth;
				}
			}

		}

		if (type == Type.Player) {
			return;
		}

		if (state == State.Dying) {
			dyingTimer -= Time.deltaTime;
			if (dyingTimer <= 0f) {
				Decay ();
			}
		}

		if (state == State.Decaying) {
			decayTimer -= Time.deltaTime;
			if (decayTimer <= 0f) {
				Destroy (gameObject);
			}
		}
	}

	void LateUpdate() {
		if (shouldUpdateRenderers) {
			UpdateRenderers ();
		}
	}

	public virtual void TakeDamage (float damage) {
		if (state == State.Alive) {
			health -= damage;

			if (health <= 0f) {
				Die ();
			} else {
				StartCoroutine (HitAnimation ());
			}

			EndRegen ();
		}
	}

	public virtual void Die () {
		if (state != State.Alive) {
			return;
		}

		state = State.Dying;
		EndRegen ();

		if (type == Type.Player) {
			// handel player death
			ResetColor();
			gameObject.GetComponent<PlayerController>().Die();
			GameManager.instance.GameOver ();
			return;
		}

		// if enemy
		if (type == Type.Enemy) {
			gameObject.GetComponent<Rigidbody> ().isKinematic = false;

			Destroy(GetComponent<NavMeshAgent> ());
			Destroy(GetComponent<EnemyController> ());
			GetComponentInChildren<ShootingController> ().Die ();
			// notify spawner of death

			if (Spawner.instance != null) {
				Spawner.instance.EnemyDeath ();
			}
		}
	}

	void Decay () {
		state = State.Decaying;

		// disable colliders
		if (GetComponent<Collider> () != null) {
			GetComponent<Collider> ().enabled = false;
		}

		// children colliders
		Collider[] childColls = GetComponentsInChildren<Collider>();
		foreach (var coll in childColls) {
			coll.enabled = false;
		}

		// set color to black
		ChangeToColor(Color.black);
	}

	//changes color to red for a moment
	IEnumerator HitAnimation() {
		ChangeToColor (hitColor);
		yield return new WaitForSeconds (hitWait);
		if (state != State.Decaying) {
			ResetColor ();
		}
	}
	 
	//sets all MeshRenderers to a certain color
	void ChangeToColor(Color color) {
		foreach (var mr in mrs) {
			foreach (var mat in mr.materials) {
				mat.color = color;
			}
		}
	}

	void StartRegen () {
		curRegenEffect = Instantiate (regenEffectPrefab);
		curRegenEffect.GetComponent<EffectFollow> ().Init (transform);
		regening = true;
	}

	void EndRegen () {
		if (curRegenEffect != null) {
			curRegenEffect.GetComponent<EffectFollow> ().End ();
			curRegenEffect = null;
		}
		waitTimer = regenWait;
		regening = false;
	}

	//resets to original color
	public void ResetColor() {
		for (int i = 0; i < mrs.Count; i++) {
			int colorCount = originalColors [i].Length;
			for (int j = 0; j < colorCount; j++) {
				mrs [i].materials [j].color = originalColors [i] [j];
			}
		}
	}
}
