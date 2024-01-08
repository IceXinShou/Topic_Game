using System;
using Connection.Method;
using JetBrains.Annotations;
using UnityEngine;
using player = Player.Player;
using static Utils;


public abstract class Map {
    public const int Empty = 0b0001;
    public const int Border = 0b0010;
    public const int FootPrint = 0b0100;
    public const int Visited = 0b1000;

    public static sbyte CurLeft;
    public static sbyte CurRight;
    public static sbyte CurTop;
    public static sbyte CurBottom;

    public static int[,] Data;
}

public class MapGenerator : MonoBehaviour {
    public static GameObject[] Chunks;
    [SerializeField] public GameObject chunkPrefab;
    [SerializeField] public GameObject foot_printPrefab;

    private void Start() {
        Chunks = new GameObject[16 * 16];
    }


    [CanBeNull]
    public GameObject GenerateChunk(ChunkPos pos) {
        // 地圖外不渲染
        // if (Math.Abs(pos.X) > 7 || Math.Abs(pos.Y) > 7) // TODO: 由伺服器端過去，待檢查 1/2
        //     return null;

        // 已經渲染過
        if (Chunks[pos.ToIndex()] is not null)
            return Chunks[pos.ToIndex()];

        var index = pos.ToIndex();
        var block = Instantiate(chunkPrefab, transform);
        block.transform.position = pos.ToVector3();
        block.name = $"{index}";
        Chunks[index] = block;

        return block;
    }

    public GameObject GenerateFootPrint() {
        return Instantiate(foot_printPrefab, transform);
    }
}