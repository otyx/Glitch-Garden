using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarTrophy : MonoBehaviour {
	
	private GameManager manager;
	public int valuePerStar;
	
	void Start() {
		manager = GameObject.FindObjectOfType<GameManager>();
	}
	
	public void collectStar() {
		manager.addStars(valuePerStar);
	}
}
