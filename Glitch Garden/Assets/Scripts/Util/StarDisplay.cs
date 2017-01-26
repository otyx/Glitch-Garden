using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Text))]
public class StarDisplay : MonoBehaviour {
	
	private Text textDisplay;
	
	// Use this for initialization
	void Start () {
		textDisplay = gameObject.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void SetStarCount(int stars) {
		textDisplay.text = stars.ToString("0000");
	}
}
