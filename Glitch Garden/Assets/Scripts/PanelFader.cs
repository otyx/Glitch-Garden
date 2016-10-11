using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PanelFader : MonoBehaviour {

	// the time for the fade in effect
	public float fadeInTime;

	private Image fadePanelImage;
	private Color currentColor = Color.black;

	void Awake() {
		fadePanelImage = gameObject.GetComponent<Image> ();
		fadePanelImage.color = currentColor;
	}

	// Use this for initialization
	void Start () {
		fadePanelImage.CrossFadeAlpha (0f, fadeInTime, false);
	}
	
	void Update() {
		if (Time.timeSinceLevelLoad >= fadeInTime) {
			fadePanelImage.enabled = false;
		}
	}
}
