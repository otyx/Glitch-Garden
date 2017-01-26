using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderSpawner : MonoBehaviour {
	
	private static GameObject defenderParent;
	private GameManager manager;
	
	// Use this for initialization
	void Start () {
		manager = GameObject.FindObjectOfType<GameManager>();	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnMouseUp() {
		GameObject obj = SelectorPanel.getCurrentSelection();
		if (obj != null) {
			// first can we pay for this object?
			if (manager.useStars(obj.GetComponent<Defender>().starCost) == GameManager.TRANSACTION_STATUS.SUCCESS) {
				Vector2 spawnPt = CalculateMouseClickToWorldPoint();
				SpawnDefender(obj, spawnPt);	
			} else {
				Debug.LogWarning("Can't pay for spawn of " + obj.name);
			}
			
			
		}
	}
	
	Vector2 CalculateMouseClickToWorldPoint(){
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 worldPoint = new Vector2(Mathf.RoundToInt(mouseWorldPos.x), Mathf.RoundToInt(mouseWorldPos.y));
		return worldPoint;
	}
	
	void SpawnDefender(GameObject prefab, Vector3 worldPoint) {
		GameObject spawn = GameObject.Instantiate(prefab, worldPoint, Quaternion.identity);
		if (defenderParent == null) {
			defenderParent = new GameObject(Constants.OBJ_DEFENDERS);
			defenderParent.transform.position = Vector3.zero;
		}
		spawn.transform.parent = defenderParent.transform; 
		spawn.layer = Constants.LYR_DEFENDERS;
		spawn.name = prefab.name;
	}
}
