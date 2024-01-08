using Connection.Utils;

namespace Connection.Method {
	public static class ClaimBlock {
		public static void SendReq(byte[] data) {
			Client.Tcp.Send(ClientHeader.BlockClaim, data);
		}
	}
}