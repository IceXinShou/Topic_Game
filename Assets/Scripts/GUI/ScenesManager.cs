using System;
using Connection;
using Connection.Method;
using TMPro;
using UnityEngine;

namespace GUI {
	public class ScenesManager : MonoBehaviour {
		private static string _iPv4 = "territory.xserver.tw";
		private static int _port = 2001;
		[SerializeField] public string curName = "Guest";
		[SerializeField] public GameObject characterObj;
		[SerializeField] public GameObject IpInputField;
		private CharacterManager _characterManager;

		private void Start() {
			_characterManager = characterObj.GetComponent<CharacterManager>();
		}

		public void Click() {
			if (Client.Tcp is not null) {
				Client.Tcp.Close();
			}
			
			Client.Tcp = new Tcp(_iPv4, _port);
			StartCoroutine(Client.ReceiveMessagesCoroutine());

			SpawnPoint.SendReq(
				curName,
				_characterManager.faceIndex,
				_characterManager.decIndex
			);
		}

		public void IPstatus() {
			IpInputField.SetActive(!IpInputField.activeSelf);
		}

		public void SetAddress(string newAddress) {
			try {
				if (newAddress.IndexOf('.') == -1)
					throw new FormatException();

				var parts = newAddress.Split(':');
				if (parts.Length == 2) {
					_iPv4 = parts[0];
					_port = Convert.ToInt32(parts[1]);
				}
				else {
					throw new FormatException();
				}
			}
			catch (FormatException) {
				IPstatus();
				IpInputField.GetComponent<TMP_InputField>().text = "";
				_iPv4 = "territory.xserver.tw";
				_port = 2001;
			}

			Debug.Log($"Address Set: {_iPv4} and {_port}");
		}


		// button method
		public void SetName(string newName) {
			if (newName.Equals(""))
				curName = "Guest";
			else
				curName = newName;
			Debug.Log($"Name Set: {curName}");
		}
	}
}