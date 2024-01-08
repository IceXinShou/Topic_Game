using System.Collections.Generic;
using UnityEngine;

namespace Player {
	public struct PlayerFootPrint {
		public float Timer;
		public readonly float UpdateInterval;
		public readonly List<GameObject> FootPrintStore;
		public bool FootprintRecord;
		public Vector3 LastFootprintPos;

		public PlayerFootPrint(float timer, float updateInterval, List<GameObject> footPrintStore) {
			Timer = timer;
			UpdateInterval = updateInterval;
			FootPrintStore = footPrintStore;
			FootprintRecord = false;
			LastFootprintPos = new Vector3();
		}
	}
}