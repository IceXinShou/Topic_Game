using System;
using System.Collections;
using System.Diagnostics;
using Connection.Method;
using Connection.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;
using player = Player.Player;

namespace Connection {
    public class Client : MonoBehaviour {
        public static Tcp Tcp;

        private void OnDestroy() {
            Tcp.Close();
        }


        /**
         * 處理伺服端傳來的訊息
         */
        private static void HandleMessage(byte[] message) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var header = message[0];
            var data = message[1..];

            // Debug.Log($"Get header {header}");

            switch (header) {
                case (byte)ServerHeader.SpawnPoint: {
                    SpawnPoint.ProcessRecv(data);
                    break;
                }

                case (byte)ServerHeader.ChunkData: {
                    ChunkData.ProcessRecv(data);
                    break;
                }

                case (byte)ServerHeader.LoseBlock: {
                    LoseBlock.ProcessRecv(data);
                    break;
                }

                case (byte)ServerHeader.PlayerJoin: {
                    PlayerJoin.ProcessReq(data);
                    break;
                }

                case (byte)ServerHeader.Scoreboard: {
                    Scoreboard.ProcessRecv(data);
                    break;
                }

                case (byte)ServerHeader.Dead: {
                    Dead.ProcessReq();
                    break;
                }

                case (byte)ServerHeader.PlayersPos: {
                    RenderPlayers.ProcessRecv(data);
                    break;
                }
            }

            Debug.LogWarning($"P0: {stopwatch.Elapsed.TotalMilliseconds * 1000:n3}μs");
        }


        public static IEnumerator ReceiveMessagesCoroutine() {
            while (true) {
                if (Tcp.DataQueue.TryTake(out var result)) {
                    HandleMessage(result);
                }

                // 讓出控制權，直到下一個 Frame
                yield return null;
            }
        }
    }
}