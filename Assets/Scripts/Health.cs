using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour {
	public enum Type {
		Player,
		Enemy,
		Object,
		Vechicle
	}
	public Type type;

	public enum State
	{
		Alive,
		Dying,
		Decaying
	};
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
	MeshRenderer[] mrs;
	List<Color[]> originalColors;

	void Start () {
		health = maxHealth;
		UpdateRenderers ();

		if (GetComponent<PlayerController> () != null) {
			type = Type.Player;
		} else if (GetComponent<EnemyController> () != null) {
			type = Type.Enemy;
		} else if (GetComponent<Vechicle> () != null) {
			type = Type.Vechicle;
		} else {
			type = Type.Object;
		}
	}

	public void UpdateRenderersNextFrame() {
		shouldUpdateRenderers = true;
	}

	void UpdateRenderers() {
		shouldUpdateRenderers = false;

		mrs = GetComponentsInChildren<MeshRenderer>();
		originalColors = new List<Color[]> ();

		for(int i = 0; i < mrs.Length; i++) {
			Color[] colors = new Color[mrs[i].materials.Length];
			for (int j = 0; j < colors.Length; j++) {
				colors [j] = mrs [i].materials [j].color;
			}
			originalColors.Add(colors);
		}
	}
	
	void Update () {
		if (state == State.Alive) {
			if (health < maxHealth) {
				if (waitTimer <= 0f) {
					// start regening
					health += regenSpeed * Time.deltaTime;
					if (health > maxHealth) {
						health = maxHealth;
					}
				} else {
					// incriment timer
					waitTimer -= Time.deltaTime;
				}
			}
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

	public void TakeDamage (float damage) {
		if (state == State.Alive) {
			health -= damage;
			if (health <= 0f) {
				Die ();
			} else {
				StartCoroutine (HitAnimation ());
			}
		}

		waitTimer = regenWait;
	}

	public void Die () {
		if (type == Type.Player) {
			// handel player death
			print ("Player Death");
			return;
		}

		state = State.Dying;

		// if enemy
		if (type == Type.Enemy) {
			gameObject.GetComponent<Rigidbody> ().isKinematic = false;

			Destroy(GetComponent<NavMeshAgent> ());
			Destroy(GetComponent<EnemyController> ());
			Destroy(GetComponentInChildren<ShootingController> ());
			Destroy (GetComponentInChildren<Weapon> ());

			// notify spawner of death
			Spawner.instance.EnemyDeath();
		}

		// if vechicle
		if (type == Type.Vechicle) {
			// test for player in vechicle
			Vechicle vechicle = GetComponent<Vechicle>();
			if (vechicle.driver) {
				GetComponentInChildren<PlayerController> ().ExitVehicle ();
			}
			// eventually spawn explosion
			Destroy(vechicle);
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

	IEnumerator HitAnimation() {
		ChangeToColor (hitColor);
		yield return new WaitForSeconds (hitWait);
		if (state != State.Decaying) {
			ResetColor ();
		}
	}

	void ChangeToColor(Color color) {
		foreach (var mr in mrs) {
			foreach (var mat in mr.materials) {
				mat.color = color;
			}
		}
	}

	void ResetColor() {
		for (int i = 0; i < mrs.Length; i++) {
			int colorCount = originalColors [i].Length;
			for (int j = 0; j < colorCount; j++) {
				mrs [i].materials [j].color = originalColors [i] [j];
			}
		}
	}
}
