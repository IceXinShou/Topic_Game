using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GUI {
	public class MusicManager : MonoBehaviour {
		[SerializeField] private AudioMixer myMixer;
		[SerializeField] private Slider musicSlider;
		[SerializeField] private Slider sfxSlider;

		private void Start() {
			if (PlayerPrefs.HasKey("musicVolume") && PlayerPrefs.HasKey("SFXVolume")) {
				LoadVolume();
			}
			else {
				musicSlider.value = 0.3F;
				sfxSlider.value = 1.5F;
				myMixer.SetFloat("Music", Mathf.Log10(0.3F) * 20);
				myMixer.SetFloat("SFX", Mathf.Log10(1.5F) * 20);
				SetMusicVolume();
				SetSFXVolume();
			}
		}

		public void SetMusicVolume() {
			var volume = musicSlider.value;
			myMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
			PlayerPrefs.SetFloat("musicVolume", volume);
		}

		private void SetSFXVolume() {
			var volume = sfxSlider.value;
			myMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
			PlayerPrefs.SetFloat("SFXVolume", volume);
		}

		private void LoadVolume() {
			musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
			sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
			SetMusicVolume();
			SetSFXVolume();
		}
	}
}