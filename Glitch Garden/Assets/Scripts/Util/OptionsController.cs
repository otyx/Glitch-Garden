using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour {

	public Slider volumeSlider;
	public float defaultVolume;

	public Slider difficultySlider;
	public float defaultDifficulty;

	public LevelManager levelManager;

	private MusicPlayer musicPlayer;

	// Use this for initialization
	void Start () {
		musicPlayer = GameObject.FindObjectOfType<MusicPlayer> ();
		UpdateOptionControls ();
	}
	
	// Update is called once per frame
	void Update () {
		if (musicPlayer) {
			musicPlayer.SetVolume (Constants.MUSIC_AUDIOSRC_NAME, volumeSlider.value);
		}
	}

	// set the current values to the sliders
	private void UpdateOptionControls() {
		volumeSlider.value = PlayerPrefsManager.GetMasterVolume ();
		difficultySlider.value = PlayerPrefsManager.GetDifficulty ();
	}

	public void SaveAndExit() {
		PlayerPrefsManager.SetMasterVolume (volumeSlider.value);
		PlayerPrefsManager.SetDifficulty (difficultySlider.value);
		levelManager.LoadScene (Constants.SCN_STARTMENU);
	}

	public void SetDefaults() {
		PlayerPrefsManager.SetDifficulty (defaultDifficulty);
		PlayerPrefsManager.SetMasterVolume (defaultVolume);
		UpdateOptionControls ();
	}
}
