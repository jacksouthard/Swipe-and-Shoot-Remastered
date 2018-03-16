using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour {
	public enum State
	{
		Alive,
		Dying,
		Decaying
	};
	[HideInInspector]
	public State state = State.Alive;

	//make this a flag later?
	public enum DamageType
	{
		None,
		Bullets,
		Explosions
	};

	[Header("Stats")]
	public float maxHealth;
	public DamageType immunity;

	[Header("Regen")]
	public float regenSpeed;
	public float regenWait;

	[Header("Hit")]
	public float hitWait;
	public Color hitColor;
	public float resistance;
	public System.Action<float> onHit;

	[Header("Death")]
	public float dyingTimer;
	public float decayTimer;
	public System.Action onDeath;

	[Header("Explosion")]
	public bool explodesOnDeath;
	public float explosionDamage;
	public float explosionForce;

	[Header("Smoke")]
	public bool smokes;
	public Transform smokeCenter;

	EffectFollow smokeEffect;
	EffectFollow fireEffect;

	[Header("Debug")]
	public float waitTimer;
	public float health;

	bool isPlayer;
	bool shouldUpdateRenderers;
	List<MeshRenderer> mrs;
	List<Color[]> originalColors;

	// regening
	bool regening = false;
	EffectFollow curRegenEffect;

	void Start () {
		health = maxHealth;
		UpdateRenderers ();

		isPlayer = (GetComponent<PlayerController> () != null);
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

		if (isPlayer) {
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

	public void TakeDamage (float damage, DamageType type) {
		if (state == State.Alive) {
			if (type == immunity) {
				return;
			}

			if (onHit != null) {
				onHit.Invoke (damage);
			}

			health -= (damage / Mathf.Max(1f, resistance));

			if (health <= 0f) {
				Die ();
			} else {
				StartCoroutine (HitAnimation ());
			}

			EndRegen ();

			if (!smokes) {
				return;
			}

			if (smokeEffect == null) {
				if ((health / maxHealth) <= 0.5f) {
					smokeEffect = EffectFollow.Create ("SmokeEffect", smokeCenter);
				}
			}

			if (fireEffect == null) {
				if ((health / maxHealth) <= 0.25f) {
					fireEffect = EffectFollow.Create ("FireEffect", smokeCenter);
				}
			}
		}
	}

	public void Die () {
		if (state != State.Alive) {
			return;
		}

		state = State.Dying;
		EndRegen ();
		ResetColor();

		if (fireEffect != null) {
			fireEffect.End ();
		}
		if (smokeEffect != null) {
			smokeEffect.End ();
		}

		if (onDeath != null) {
			onDeath.Invoke ();
		}

		if (explodesOnDeath) {
			Explosion.Create (transform.Find("Center").position, 5, explosionForce, explosionDamage);
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
			if (mr == null) {
				UpdateRenderers ();
				return;
			}
			foreach (var mat in mr.materials) {
				mat.color = color;
			}
		}
	}

	void StartRegen () {
		curRegenEffect = EffectFollow.Create("RegenEffect", transform);
		regening = true;
	}

	void EndRegen () {
		if (curRegenEffect != null) {
			curRegenEffect.End ();
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
