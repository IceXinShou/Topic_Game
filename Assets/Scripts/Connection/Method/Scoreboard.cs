using System;
using System.Collections.Concurrent;
using TMPro;
using UnityEngine;
using player = Player.Player;

namespace Connection.Method {
	public class Scoreboard : MonoBehaviour {
		private static readonly ConcurrentQueue<Tuple<byte, int>[]> WaitListQueue = new();
		[SerializeField] private GameObject[] rank;

		private readonly TextMeshProUGUI[] _names = new TextMeshProUGUI[10];
		private readonly TextMeshProUGUI[] _values = new TextMeshProUGUI[10];

		private void Start() {
			for (var i = 0; i < 10; ++i) {
				_names[i] = rank[i].transform.Find("Name").GetComponent<TextMeshProUGUI>();
				_values[i] = rank[i].transform.Find("Num").GetComponent<TextMeshProUGUI>();
			}
		}

		private void FixedUpdate() {
			// 定期檢查並更新 scoreboard

			if (WaitListQueue.IsEmpty) return;

			var data = WaitListQueue.TryDequeue(out var message) ? message : null;
			if (data == null) return;

			for (var i = 0; i < data.Length; i++) {
				var cur = data[i];
				if (cur == null) {
					_names[i].text = "";
					_values[i].text = "";
					continue;
				}

				_names[i].text = player.PlayersInfo[cur.Item1].Name;
				_values[i].text = Convert.ToString(cur.Item2);
			}
		}

		public static void ProcessRecv(byte[] data) {
			// 將接收到的新資料加入進 Queue 內
			// 不直接修改值，反而使用 Queue 的原因是要 static
			// 取得 TextMeshProUGUI 元素的過程不能使用 static

			Debug.Log(data.Length);
			var store = new Tuple<byte, int>[10];
			for (var i = 0; i < data.Length; i += 3) {
				var uid = data[i];
				var count = BitConverter.ToInt16(data[(i + 1)..(i + 3)]);

				store[i] = new Tuple<byte, int>(uid, count);
			}

			WaitListQueue.Enqueue(store);
		}
	}
}