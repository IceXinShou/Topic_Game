using System;
using System.Linq;
using UnityEngine;

public abstract class Utils {
	public static readonly Color[] Colors = {
		new(0xFE / 255f, 0xEC / 255f, 0xF9 / 255f),
		new(0xEE / 255f, 0x8A / 255f, 0x2D / 255f),
		new(0xD6 / 255f, 0xB8 / 255f, 0x36 / 255f),
		new(0x88 / 255f, 0x9E / 255f, 0xDB / 255f),
		new(0xC2 / 255f, 0x70 / 255f, 0xE3 / 255f),
		new(0x98 / 255f, 0xFF / 255f, 0x04 / 255f),
		new(0x00 / 255f, 0x95 / 255f, 0x5C / 255f),
		new(0x00 / 255f, 0x8F / 255f, 0x88 / 255f),
		new(0xBD / 255f, 0x00 / 255f, 0x19 / 255f),
		new(0xB2 / 255f, 0x98 / 255f, 0x00 / 255f),
		new(0xff / 255f, 0xff / 255f, 0xff / 255f)
	};

	public static double Distance(Vector3 a, Vector3 b) {
		// 沒有開根號
		return Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2);
	}

	/**
     * 回傳座標在 chunk 中的位置編號，左下角為 0，往右遞增
     */
	public static int PosToIndex(sbyte posX, sbyte posY) {
		return ((posY + 7) % 15 + 15) % 15 * 15 + ((posX + 7) % 15 + 15) % 15;
	}

	public static ChunkPos ChunkPosToBlockPos(int chunkX, int chunkY, int index) {
		// Chunk 的座標 轉為 絕對座標
		return new ChunkPos((sbyte)(chunkX * 15 + index % 15 - 7), (sbyte)(chunkY * 15 + index / 15 - 7));
	}

	public static byte BlockPosToChunkIndex(float posX, float posY) {
		/* 小方塊絕對座標(posX, posY) => 所在的 chunk id*/
		var x = (int)Math.Floor((posX + 7) / 15);
		var y = (int)Math.Floor((posY + 7) / 15);
		return (byte)(((x & 0x0f) << 4) | (y & 0x0f));
	}

	public static byte ChunkPosToChunkIndex(int posX, int posY) {
		return (byte)(((posX & 0x0f) << 4) | (posY & 0x0f));
	}

	public static ChunkPos BlockPosToChunkPos(float posX, float posY) {
		var x = (sbyte)Math.Floor((posX + 7) / 15);
		var y = (sbyte)Math.Floor((posY + 7) / 15);
		return new ChunkPos(x, y);
	}


	public static ref int QueryMap(sbyte posX, sbyte posY) {
		// 絕對座標
		return ref Map.Data[posX + 112, posY + 112];
	}

	public static ref int QueryMap(ChunkPos chunkPos) {
		// 絕對座標
		return ref QueryMap(chunkPos.X, chunkPos.Y);
	}

	public static byte[] CombineBytes(params byte[][] arrays) {
		var ret = new byte[arrays.Sum(x => x.Length)];
		var offset = 0;
		foreach (var data in arrays) {
			Buffer.BlockCopy(data, 0, ret, offset, data.Length);
			offset += data.Length;
		}

		return ret;
	}

	public static void UpdateMapBorder(sbyte x, sbyte y) {
		Map.CurRight = Math.Max(Map.CurRight, x);
		Map.CurLeft = Math.Min(Map.CurLeft, x);
		Map.CurTop = Math.Max(Map.CurTop, y);
		Map.CurBottom = Math.Min(Map.CurBottom, y);
	}

	public static Color FromRGB(Vector3 rgbVector) {
		return new Color(rgbVector.x / 255, rgbVector.y / 255, rgbVector.z / 255);
	}

	public static byte[] GetBytes(int num, int length) {
		return BitConverter.GetBytes(num)[..length];
	}

	public readonly struct ChunkPos {
		public readonly sbyte X;
		public readonly sbyte Y;

		/**
         * 透過遊戲絕對座標初始化
         */
		public ChunkPos(int x, int y) {
			X = (sbyte)(x / 15);
			Y = (sbyte)(y / 15);
		}

		/**
         * 透過 index 初始化
         */
		public ChunkPos(byte index) {
			X = (sbyte)(index >> 4);
			Y = (sbyte)(index & 0x0F);
		}

		/**
         * 透過 Chunk 位置初始化
         */
		public ChunkPos(sbyte x, sbyte y) {
			X = x;
			Y = y;
		}

		/**
         * 透過遊戲絕對座標向量初始化
         */
		public ChunkPos(Vector3 vector3) {
			X = (sbyte)(vector3.x / 15);
			Y = (sbyte)(vector3.y / 15);
		}

		/**
         * 取得相對 Chunk 偏移量的 ChunkPos 物件
         */
		public ChunkPos GetOffsetPos(sbyte x, sbyte y) {
			return new ChunkPos((sbyte)(X + x), (sbyte)(Y + y));
		}

		/**
         * 取得相對 Chunk 偏移量的 Vector3 物件
         */
		public Vector3 GetOffsetVec(sbyte x, sbyte y) {
			return new Vector3(15 * (X + x), 15 * (Y + y));
		}

		public byte GetOffsetIndex(sbyte x, sbyte y) {
			return (byte)((((X + x) & 0x0F) << 4) | ((Y + y) & 0x0F));
		}


		/**
         * 計算陣列用 Index
         */
		public byte ToIndex() {
			return (byte)(((X & 0x0F) << 4) | (Y & 0x0F));
		}

		/**
         * 取得 Vector3 物件
         */
		public Vector3 ToVector3() {
			return new Vector3(X * 15, Y * 15);
		}

		public override bool Equals(object obj) {
			return obj is ChunkPos pos && base.Equals(pos);
		}

		public bool Equals(ChunkPos other) {
			return X == other.X && Y == other.Y;
		}

		public override int GetHashCode() {
			return ToIndex();
		}
	}
}