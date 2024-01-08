using UnityEngine;

namespace Player {
	public class Info {
		public readonly float ForwardSpeed;
		public readonly float RotateSpeed;
		public byte Decoration;
		public byte Face;
		public string Name = "Guest";
		public Color PlayerColor;
		public Vector3 SpawnPoint;
		public int Sum = 0;
		public byte UID;

		public Info(byte uid, float forwardSpeed, float rotateSpeed, Color playerColor, Vector3 spawnPoint) {
			UID = uid;
			ForwardSpeed = forwardSpeed;
			RotateSpeed = rotateSpeed;
			PlayerColor = playerColor;
			SpawnPoint = spawnPoint;
		}

		public Info(byte uid, string name, byte decoration, byte face, Color playerColor) {
			UID = uid;
			Name = name;
			Decoration = decoration;
			Face = face;
			PlayerColor = playerColor;
		}
	}
}