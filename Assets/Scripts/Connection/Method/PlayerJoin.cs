using System.Text;
using Player;
using player = Player.Player;
using static Utils;

namespace Connection.Method {
	public static class PlayerJoin {
		public static void ProcessReq(byte[] data) {
			var uid = data[0];
			var face = data[1];
			var decoration = data[2];
			var name = Encoding.UTF8.GetString(data[3..]);

			player.PlayersInfo[uid] = new Info(uid, name, decoration, face, Colors[uid]);
		}
	}
}