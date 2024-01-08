using Connection.Utils;

namespace Connection.Method {
	public static class Suicide {
		public static void SendReq() {
			Client.Tcp.Send(ClientHeader.Suicide);
		}
	}
}