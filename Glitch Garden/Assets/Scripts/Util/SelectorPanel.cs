using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorPanel : MonoBehaviour {
	// the currently active button
	private static Button currentButton;
	
	// the currently active object associated with the active button
	private static GameObject currentObject;
	
	// this panels buttons
	private Button[] buttons;
	
	// Use this for initialization
	void Start () {
		buttons = GetComponentsInChildren<Button>();
	}
	
	
	public void HandleButtonMouseDown(Button btn) {
		if (   btn.getState() == Button.BTN_STATE.AVAILABLE) {
			if (currentButton != null) {
				// unset the current button 
				SetButtonForBalance(currentButton, GameManager.GetStarCount());
			}
			
			// select
			btn.setState(Button.BTN_STATE.SELECTED);	
			currentButton = btn; 
			currentObject = btn.objectForButton;	
			UpdateButtons();
		} else {
			Debug.Log("Trying to select inactive button: " + name);
		}
		
	}
	public void UpdateButtons() {
		foreach (Button button in buttons) {
			if  (GameManager.GetStarCount() < button.getObjectCost()) {
				button.setState(Button.BTN_STATE.UNAVAILABLE);
			}
			button.refreshDisplay();
		}
	}
	
	public void SetActiveButtonsForBalance(int balance) {
		foreach (Button button in buttons) {
			SetButtonForBalance(button, balance);
		}
	}
	
	public void SetButtonForBalance(Button btn, int balance) {
		if (balance >= btn.getObjectCost()) {
			btn.setState(Button.BTN_STATE.AVAILABLE);	
		} else {
			btn.setState(Button.BTN_STATE.UNAVAILABLE);
		}
	}
	
	public static GameObject getCurrentSelection (){
		return currentObject;
	}
	
}
