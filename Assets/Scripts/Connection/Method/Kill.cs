using System;
using Connection.Utils;
using UnityEngine;
using static Utils;

namespace Connection.Method {
	public static class Kill {
		public static void SendReq(string otherName) {
			// TODO: BUG
			Debug.Log(otherName);
			Debug.Log(Convert.ToByte(otherName));
			Client.Tcp.Send(ClientHeader.Kill, GetBytes(Convert.ToByte(otherName), 1)); // 擊殺請求
		}
	}
}