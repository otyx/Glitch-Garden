using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour {
	
	// possible button states
	public enum BTN_STATE {AVAILABLE, UNAVAILABLE, SELECTED};
	private BTN_STATE currentState = BTN_STATE.UNAVAILABLE;
	
	// the selectorpanel managing this button
	private SelectorPanel selectorPanel;
	
	// this prefab associated with this instance of Button which is 
	// set into the static variable currentObject for retrieval via 
	// a static method.
	public GameObject objectForButton;
	
	
	void Start() {
		selectorPanel = GameObject.FindObjectOfType<SelectorPanel>();
	}
	
	// pass the message up to the selector panel parent for handling
	void OnMouseDown() {
		selectorPanel.HandleButtonMouseDown(this);
	}
	
	// the cost of the object associated with this button
	public int getObjectCost() {
		return objectForButton.GetComponent<Defender>().starCost;
	}
	
	public void setState(Button.BTN_STATE state) {
		currentState = state;
		refreshDisplay();
	}
	
	public BTN_STATE getState() {
		return currentState;
	}
	
	public void refreshDisplay() {
		switch (currentState) {
			case BTN_STATE.AVAILABLE:
				gameObject.GetComponent<SpriteRenderer>().color = Color.white;
				break;
			case BTN_STATE.SELECTED:
				gameObject.GetComponent<SpriteRenderer>().color = Color.green;
				break;
			case BTN_STATE.UNAVAILABLE:
			default:
				gameObject.GetComponent<SpriteRenderer>().color = Color.black;
				break;
		}
	}
}
