using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Connection.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Utils;
using ThreadPriority = System.Threading.ThreadPriority;

namespace Connection {
    public class Tcp {
        public static readonly BlockingCollection<byte[]> DataQueue = new(new ConcurrentQueue<byte[]>());


        /**
         * 客戶端連線 Class，初始化 AES, RSA 與連線流
         */
        private readonly TcpClient _client;

        private readonly Crypto _crypto;

        private readonly NetworkStream _stream;
        private int _reqPacks;
        private int _rspPacks;

        public Tcp(string ip, int port) {
            _client = new TcpClient(ip, port);
            _stream = _client.GetStream();

            var buffer = new byte[1024];
            _stream.Read(buffer, 0, buffer.Length);
            Crypto.InitRsa(buffer);

            Crypto.InitAes();
            _stream.Write(Crypto.GetAesKeyIv());


            // 使用 Thread 來執行 StartReceiving 方法
            var receivingThread = new Thread(StartReceiving) {
                IsBackground = true, // 將其設置為背景線程
                Priority = ThreadPriority.AboveNormal
            };
            receivingThread.Start();
        }


        /**
         * 發送資料給伺服器 (經過AES加密)。
         * 傳送給伺服端
         */
        public void Send(ClientHeader header) {
            ++_reqPacks;
            var cipher = Crypto.EncryptWithAes(new[] { (byte)header });
            _stream.Write(CombineBytes(BitConverter.GetBytes(Convert.ToUInt16(cipher.Length)), cipher));
        }

        public void Send(ClientHeader header, byte[] data) {
            ++_reqPacks;
            Debug.Log($"current: req | rsp: {_reqPacks} | {_rspPacks}");
            var fullReq = CombineBytes(new[] { (byte)header }, data);
            var cipher = Crypto.EncryptWithAes(fullReq);
            try {
                _stream.Write(CombineBytes(BitConverter.GetBytes(Convert.ToUInt16(cipher.Length)), cipher));
            }
            catch (IOException e) {
                Close();
                Debug.LogError($"Error Get: {e.Message}");
                SceneManager.LoadScene("StartMenu");
            }
        }

        /**
         * 啟動接收訊息的處理程序
         */
        private async void StartReceiving() {
            try {
                var lengthBuffer = new byte[2];
                var buffer = new byte[65536];

                while (_client.Connected) {
                    await _stream.ReadAsync(lengthBuffer, 0, 2); // 讀取長度資訊(2 bytes)
                    var messageLength = BitConverter.ToUInt16(lengthBuffer, 0);

                    await _stream.ReadAsync(buffer, 0, messageLength); // 讀取資料
                    var receivedMessage = Crypto.DecryptWithAes(buffer, messageLength); // 解密資訊

                    // Debug.Log($"Get Data: {Encoding.UTF8.GetString(receivedMessage)}");

                    DataQueue.Add(receivedMessage); // 儲存接受到的訊息
                    // MessageQueue.Enqueue(receivedMessage);    
                    _rspPacks++;
                }
            }
            catch (Exception e) {
                Debug.LogError($"Error receiving data: {e.Message}");
                // TODO: 可以在這裡加入重連邏輯或其他處理
            }
        }

        public void Close() {
            _stream.Close();
            _client.Close();
        }
    }
}