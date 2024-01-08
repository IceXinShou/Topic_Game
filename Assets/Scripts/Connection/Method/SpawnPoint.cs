using System;
using System.Collections.Generic;
using System.Text;
using Connection.Utils;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using player = Player.Player;
using static Utils;

namespace Connection.Method {
	public static class SpawnPoint {
		public static void SendReq(string name, byte face, byte decorate) {
			// 初始化
			Map.Data = new int[15 * 15, 15 * 15];
			player.PlayersInfo = new Info[10];
			player.OwnPlayerInfo = new Info(10, 0.3f, 300f, Colors[10], new Vector3(0, 0));
			player.PlayerFootPrint = new PlayerFootPrint(0, 0.075f, new List<GameObject>());
			player.FootprintsLogs = new List<Material>(16_777_216);
			
			player.OwnPlayerInfo.Face = face;
			player.OwnPlayerInfo.Decoration = decorate;
			player.OwnPlayerInfo.Name = name;
			
			Client.Tcp.Send(
				ClientHeader.SpawnPoint,
				CombineBytes(
					new[] { face, decorate },
					Encoding.UTF8.GetBytes(name)
				)
			);
		}
		
		public static void ProcessRecv(byte[] data) {
			player.OwnPlayerInfo.UID = data[0];

			// 絕對座標
			var x = (sbyte)(data[1] - 112);
			var y = (sbyte)(data[2] - 112);
			var sum = Convert.ToInt32(data[3]);

			player.OwnPlayerInfo.Sum = sum;
			player.OwnPlayerInfo.SpawnPoint = new Vector3(x, y);

			SceneManager.LoadScene("GameScene");
		}
	}
}