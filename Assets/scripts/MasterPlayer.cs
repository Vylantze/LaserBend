﻿using UnityEngine;
using System.Collections;

public class MasterPlayer : PlayerController {
	// singleton design pattern
	public static MasterPlayer mainPlayer = null;

	public PlatformController platformer;
	public ShipController ship;
	public GunScript gun;
	public bool flip = true; // true = faceright, false = faceleft
	public bool in_air = false;
	bool saved_before = false; // at the start of the game, shouldn't have saved before

	// health
	public bool dead = false;

	//sound
	public AudioClip switch_mode;

	// Use this for initialization
	void Awake() {
		if (mainPlayer == null) {
			DontDestroyOnLoad (gameObject);
			mainPlayer = this;
		} else if (mainPlayer!=this) {
			Destroy (gameObject);
		}
	}

	void Start () {
		saved_before = false;
		loadedFromFile = false;
		gun = GetComponentInChildren<GunScript> ();
		ship = GetComponentInChildren<ShipController> ();
		platformer = GetComponentInChildren<PlatformController> ();
		reset ();
	}

	void loadAnimator() {
		// force the renders to eitehr be in existence or not
		platformer.female_chara.GetComponent<SpriteRenderer>().enabled = female;
		platformer.male_chara.GetComponent<SpriteRenderer>().enabled= !female;
		if (female) {
			anim = platformer.female_chara.GetComponent<Animator>(); // load female animator
		} else { // else if male
			anim = platformer.male_chara.GetComponent<Animator>();; // load male animator
			setCharge (0f); // set 'charging' layer to 0
		}
		platformer.setAnimator(anim);
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Switch Mode")&&STATUS.debug) {
			female = !female;
			// true is female
			// false is male
			loadAnimator ();
		}
		if (!dead) {
			colourChange ();
		}
		/*
		if (Input.GetButtonDown("SwitchScene")&&STATUS.debug) {
			shipMode = !shipMode;
			ship.gameObject.SetActive(shipMode);
			platformer.gameObject.SetActive(!shipMode);
		}*/
	}

	void colourChange() {
		for (int i=1; i<4; i++) { // start from 1, aka fire
			if (Input.GetButtonDown (commands[i]) && // if correct command
			    elements [i]) { // and element is now assessible
				AudioSource.PlayClipAtPoint(switch_mode, transform.position);
				GetComponent<ModeChange> ().currentColour = i;
				break; // once colour changed, can break loop
			}
		}
	}

	public void Restart() {
		reset();
		Application.LoadLevel (Application.loadedLevel);
		PlatformCamera camera = PlatformCamera.mainCamera;
		camera.lockCamera = true;
		camera.enabled = false;
		loadFromFile ();
		if (saved_before) {
			mainPlayer.loadPosition (); 
		}
		camera.transform.position = transform.position;
		reset ();
		camera.lockCamera = false;
		camera.enabled = true;
	}

	public void SetTrigger(string value) {
		anim.SetTrigger (value);
	}

	public void reset() {
		dead = false;
		if (!shipMode) {
			platformer.enabled = true;
			loadAnimator();
			platformer.reset();
			ship.disable ();
		} else {
			ship.enabled = true; 
			ship.reset();
			platformer.disable ();
		}
		gun.reset ();
	}

	public void enableAll(bool value) {
		platformer.enabled = value;
		ship.enabled = value;
		gun.enableGuns (value);
		gun.enabled = value;
	}

	public void enableGuns(bool value) {
		gun.enableGuns (value);
		gun.enabled = value;
	}

	public void loadPosition() {
		transform.position = loadedPosition;
	}

	public void Death(){
		if (!dead) {
			dead = true;
			if (!shipMode) {
				PlatformCamera camera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<PlatformCamera> ();
				camera.lockCamera = true;
				anim.SetBool ("dead", true);
			}
			Collider2D[] colliders = transform.GetComponentsInChildren<Collider2D> ();
			foreach (Collider2D collider in colliders) {
				collider.isTrigger = true;
			}
			Rigidbody2D rb2d = transform.GetComponentInChildren<Rigidbody2D> ();
			if (!shipMode) {
				platformer.enabled = false;
			} else {
				ship.enabled = false;
			}
			rb2d.velocity = Vector2.zero;
			rb2d.AddForce (new Vector2 (0f, 500));
			rb2d.gravityScale = 1f;
		}
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.CompareTag ("EnemyBullet")&&!dead) {
			Death ();
		}
	}

	public Vector3 shipPosition() {
		return ship.gameObject.transform.position;
	}

	public Vector3 platformPosition() {
		return platformer.gameObject.transform.position;
	}

	public void setCharge(float value) {
		if (!female&&!shipMode) { // only if male chara and platformer mode
			anim.SetLayerWeight (1, value);
		}
	}

	public void save() {
		save (transform.position);
	}

	public void save(Vector3 position) {
		saved_before = true;
		saveToFile ("autoSave", position.x, position.y);
	}
}
