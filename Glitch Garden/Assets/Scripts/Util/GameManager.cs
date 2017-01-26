using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	
	public enum TRANSACTION_STATUS {SUCCESS, FAILURE};
	
	// starting balance of star counts
	public static int starCount = 300;
	
	// the Star Display 
	private StarDisplay starDisplay;

	// the Selector Panel
	private SelectorPanel selectorPanel;
	
	// persist!
	void Awake() {
		DontDestroyOnLoad(transform.gameObject);
	}
	
	// Use this for initialization
	void Start () {
		// finds the stardisplay script component object
		starDisplay = GameObject.FindObjectOfType<StarDisplay>();
		selectorPanel = GameObject.FindObjectOfType<SelectorPanel>();
		
		// initialise the display
		starDisplay.SetStarCount(starCount);
		selectorPanel.SetActiveButtonsForBalance(starCount);
	}
	
	public static int GetStarCount() {
		return starCount;
	} 
	
	public TRANSACTION_STATUS addStars(int stars) {
		starCount += stars;
		starDisplay.SetStarCount(starCount);
		selectorPanel.SetActiveButtonsForBalance(starCount);
		return TRANSACTION_STATUS.SUCCESS;
	}
	
	public TRANSACTION_STATUS useStars(int stars) {
		TRANSACTION_STATUS status = TRANSACTION_STATUS.FAILURE;
		if (stars<= starCount) {
			starCount -= stars;
			starCount = (starCount < 0 ) ? 0 : starCount;
			starDisplay.SetStarCount(starCount);
			selectorPanel.SetActiveButtonsForBalance(starCount);
			status = TRANSACTION_STATUS.SUCCESS;
		}
		return status;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
