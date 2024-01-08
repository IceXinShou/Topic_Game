using System;
using System.Collections.Concurrent;
using UnityEngine;
using player = Player.Player;

namespace Connection.Method {
	public class RenderPlayers : MonoBehaviour {
		private static readonly ConcurrentQueue<byte[]> WaitListQueue = new();
		[SerializeField] private GameObject playerExampleObj;
		[SerializeField] private Sprite[] faces;
		[SerializeField] private Sprite[] decorates;
		private readonly GameObject[] _players = new GameObject[10];
		private int _renderedPlayer;

		// [uid9] [uid8] [uid7] ... [uid0]
		public RenderPlayers(int uid) {
			_renderedPlayer |= 1 << uid;
		}

		private void FixedUpdate() {
			if (WaitListQueue.IsEmpty) return;
			var data = WaitListQueue.TryDequeue(out var message) ? message : null;
			if (data == null) return;

			var curRender = 0; // 用來紀錄當玩家消失
			for (var i = 0; i < data.Length; i += 7) {
				var uid = data[i];
				var realX = (Convert.ToUInt16(data[(i + 1)..(i + 3)]) - 112) / 100f;
				var realY = (Convert.ToUInt16(data[(i + 3)..(i + 5)]) - 112) / 100f;
				var rotZ = Convert.ToUInt16(data[(i + 3)..(i + 5)]) - 180;
				var face = data[5];
				var decoration = data[6];

				curRender |= 1 << uid;

				// if ((_renderedPlayer & (1 << uid)) <= 0)

				var curPlayer = _players[uid];
				if (curPlayer is null) {
					// 未曾渲染過

					_renderedPlayer |= 1 << uid;
					curPlayer = Instantiate(playerExampleObj, transform);
					curPlayer.name = Convert.ToString(uid);
					curPlayer.GetComponent<SpriteRenderer>().sprite = faces[face];
					curPlayer.GetComponentInChildren<SpriteRenderer>().sprite = decorates[decoration];
				}

				curPlayer.transform.position = new Vector3(realX, realY, 0);
				curPlayer.transform.rotation = Quaternion.Euler(0, 0, rotZ);

				if (curRender != _renderedPlayer) {
					// 有玩家沒有被更新到，可能是已經死亡，要刪除

					var deleteUids = curRender ^ _renderedPlayer;

					var deleteUid = 0;
					while (deleteUids > 0) {
						if ((deleteUids & 1) == 1) {
							Destroy(_players[deleteUid]);
							_renderedPlayer -= 1 << deleteUid;
						}

						deleteUid++;
						deleteUids >>= 1;
					}
				}
			}
		}

		public static void ProcessRecv(byte[] data) {
			WaitListQueue.Enqueue(data);
		}
	}
}