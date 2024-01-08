using System;

namespace Connection.Method {
	public static class LoseBlock {
		public static void ProcessRecv(byte[] data) {
			var loseCount = BitConverter.ToInt32(data);
			Player.Player.OwnPlayerInfo.Sum -= loseCount;
		}
	}
}