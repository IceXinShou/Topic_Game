using UnityEngine;
using UnityEngine.SceneManagement;
using player = Player.Player;

namespace Connection.Method {
	public class Dead : MonoBehaviour {
		public static void ProcessReq() {
			Client.Tcp.Close();
			SceneManager.LoadScene("StartMenu");
		}
	}
}