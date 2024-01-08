using System;
using System.Diagnostics;
using Connection.Utils;
using UnityEngine;
using player = Player.Player;
using static Utils;
using Debug = UnityEngine.Debug;

namespace Connection.Method {
    public static class ChunkData {
        /**
         * Packet-Format
         * [chunk-X] [chunk-Y] [block-0] [block-1] ... [block-224] ([footprint-uid][fp-X][fp-X][fp-Y][fp-Y][fp-rot][fp-rot] ... )
         */
        public static void ProcessRecv(byte[] data) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var chunkX = Convert.ToSByte(data[0] - 7);
            var chunkY = Convert.ToSByte(data[1] - 7);

            // 初始化 chunk
            var chunk = player._mapGenerator.GenerateChunk(new ChunkPos(chunkX, chunkY));
            // if (chunk is null) // 渲染失敗 (地圖邊界外) TODO: 由伺服器端過去，待檢查 2/2
            //     return;
            Debug.LogWarning($"P1.1: {stopwatch.Elapsed.TotalMilliseconds * 1000:n3}μs");
            Debug.LogWarning($"P1.2: {stopwatch.Elapsed.TotalMilliseconds * 1000:n3}μs");


            // 渲染子區塊
            for (var i = 0; i < 225; ++i) {
                var owner = data[i + 2];

                if (owner == 10) // 若無擁有者，渲然為預設顏色 (不做更改) 
                    continue;

                // 調整 block 顏色
                var block = chunk.transform.GetChild(i);
                block.GetComponent<Renderer>().material.color = Colors[data[i + 2]]; // 2 bytes offset

                // 若擁有者為自己，為 FloodFill 紀錄用
                if (data[i + 2] == player.OwnPlayerInfo.UID) {
                    var blockPos = ChunkPosToBlockPos(chunkX, chunkY, i); // TODO: 簡化
                    QueryMap(blockPos) |= Map.Border;
                }
            }

            Debug.LogWarning($"P2: {stopwatch.Elapsed.TotalMilliseconds * 1000:n3}μs");

            // 渲染 FootPrint
            var footprintData = data[227..];
            for (var i = 0; i < footprintData.Length; i += 7) {
                var id = footprintData[i];

                var posX = BitConverter.ToUInt16(footprintData[(i + 1)..(i + 3)]);
                var posY = BitConverter.ToUInt16(footprintData[(i + 3)..(i + 5)]);
                var rotZ = BitConverter.ToUInt16(footprintData[(i + 5)..(i + 7)]);

                var footPrint = player._mapGenerator.GenerateFootPrint();
                footPrint.GetComponent<Renderer>().material.color = Colors[id];
                footPrint.transform.position = new Vector3(posX / 100f - 112, posY / 100f - 112, 0);
                footPrint.transform.rotation = Quaternion.Euler(0, 0, rotZ - 180);
                footPrint.name = Convert.ToString(id);
            }
            
            Debug.LogWarning($"P3: {stopwatch.Elapsed.TotalMilliseconds * 1000:n3}μs");
        }
    }
}