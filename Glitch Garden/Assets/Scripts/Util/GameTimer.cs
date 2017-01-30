using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Slider))]
public class GameTimer : MonoBehaviour {
	
	private Slider timeSlider;
	private LevelManager levelManager;
	
	public float levelDurationInSecs;
	private bool levelEnded = false;
	
	// Use this for initialization
	void Start () {
		timeSlider = GetComponent<Slider>();
		levelManager = GameObject.FindObjectOfType<LevelManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (levelEnded == false) {
			timeSlider.value = Time.timeSinceLevelLoad / levelDurationInSecs ;
			if (timeSlider.value >= 1) {
				Debug.Log("Triggering end of level");
				TriggerEndOfLevel();
			}
		}
	}
	
	public void TriggerEndOfLevel() {
		Debug.Log("in Trigger end of level");
		levelEnded = true;
		StartCoroutine(levelManager.FinishLevel(Constants.LEVEL_STATE.WIN));
	}
}
