using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour {
	
	// the currently active button
	private static GameObject currentButton;
	
	// the currently active object associated with the active button
	private static GameObject currentObject;
	
	// this prefab associated with this instance of Button which is 
	// set into the static variable currentObject for retrieval via 
	// a static method.
	public GameObject objectForButton;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnMouseDown() {
		print(name + " pressed");
		if (currentButton) {
			currentButton.GetComponent<SpriteRenderer>().color = Color.black;
		}
		gameObject.GetComponent<SpriteRenderer>().color = Color.white;
		currentButton = gameObject; 
		currentObject = objectForButton;
	}
	
	public static GameObject getCurrentSelection (){
		return currentObject;
	}
}
