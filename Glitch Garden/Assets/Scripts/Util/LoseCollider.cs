using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseCollider : MonoBehaviour {

	LevelManager levelManager;

	void Start() {
		levelManager = GameObject.FindObjectOfType<LevelManager> ();
		if (levelManager == null) {
			Debug.LogError("LoseCollider: Level Manager not found");
		}
	}


	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag.Equals(Constants.ATTACKER)) {
			// game is over!
			levelManager.LoadScene(Constants.SCN_LOSE);	
		}
	}
} 
