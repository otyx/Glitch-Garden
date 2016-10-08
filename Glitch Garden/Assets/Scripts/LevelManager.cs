using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

	public float autoLoadLevelInterval;

	void Awake() {
		DontDestroyOnLoad (gameObject);
	}

	// Use this for initialization
	void Start () {
		Invoke ("LoadStartScene", autoLoadLevelInterval);
	}

	public void LoadStartScene() {
		LoadScene (Constants.LVL_STARTMENU);
	}

	public void LoadScene(string sceneName) {
		SceneManager.LoadScene (sceneName);
	}

	public void Quit() {
		Debug.Log ("LM: Quit called");
		Application.Quit ();
	}
}
