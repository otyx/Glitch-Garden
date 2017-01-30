using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

	public float autoLoadLevelInterval;
	
	private MusicPlayer musicPlayer;
	private GameObject winText, loseText;
	
	// No need to keep between levels since only using the prefab for level changes.
	// Also, by default, autoloadlevel is 0 so we don't get automatic level loading
	// Therefore, to move from splash to start,change the value to 3 seconds (for example)
	//
	//void Awake() {
	//	DontDestroyOnLoad (gameObject);
	//}

	// Use this for initialization
	void Start () {
		if (autoLoadLevelInterval <= 0) {
			Debug.Log ("LM: Level Auto Load - disabled - use a positive seconds if to be enabled");
		} else {
			Invoke ("LoadStartScene", autoLoadLevelInterval);
		}
		// hide the texts
		winText = GameObject.FindGameObjectWithTag(Constants.TAG_WINTEXT);
		loseText = GameObject.FindGameObjectWithTag(Constants.TAG_LOSETEXT);
		
		if (winText == null || loseText == null) {
			Debug.LogError("LevelManager: unable to find one of the win / lose texts: " + winText + " / " + loseText);
		} else {
			winText.SetActive(false);
			loseText.SetActive(false);
		}
		
		
		musicPlayer = GameObject.FindObjectOfType<MusicPlayer>();
		musicPlayer.PlayMusicClip(musicPlayer.musicLibrary[SceneManager.GetActiveScene().buildIndex]);
	}

	public void LoadStartScene() {
		LoadScene (Constants.SCN_STARTMENU);
	}

	public void LoadScene(string sceneName) {
		if (Application.CanStreamedLevelBeLoaded(sceneName)) {
			SceneManager.LoadScene(sceneName);	
		} else {
			Debug.LogError("LM: Unable to load scene: " + sceneName);
		}
	}
	
	public void LoadScene(int idx) {
		if (Application.CanStreamedLevelBeLoaded(idx)) {
			SceneManager.LoadScene(idx);	
		} else {
			Debug.LogError("LM: Unable to load scene: " + idx);
		}
	}
	
	public IEnumerator FinishLevel(Constants.LEVEL_STATE state) {
		Debug.Log("LM: Finishing level with a " + state);	
		if (state == Constants.LEVEL_STATE.WIN) {
			musicPlayer.PlayEffectClip(musicPlayer.effectLibrary[Constants.EFFECT_WIN], false, 0.8f);
			winText.SetActive(true);
		}
		GameManager.isPaused = true;
		
		int nextSceneIdx = SceneManager.GetActiveScene().buildIndex + 1;
		Time.timeScale = 0.0f;
		yield return new WaitForSecondsRealtime(musicPlayer.effectLibrary[Constants.EFFECT_WIN].length);
		LoadScene(nextSceneIdx);
	}
	
	public void Quit() {
		Debug.Log ("LM: Quit called");
		Application.Quit ();
	}
}
