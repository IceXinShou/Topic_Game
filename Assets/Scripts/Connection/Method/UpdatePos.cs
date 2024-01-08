using Connection.Utils;

namespace Connection.Method {
	public static class UpdatePos {
		public static void SendReq(byte[] data) {
			Client.Tcp.Send(ClientHeader.UpdatePos, data);
		}
	}
}