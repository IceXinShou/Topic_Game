using UnityEngine;

public class AudioManager : MonoBehaviour {
	[Header("Audio Source")] [SerializeField]
	private AudioSource Background;

	[SerializeField] private AudioSource Buttonclick;

	private void Start() {
		DontDestroyOnLoad(gameObject);
	}
}