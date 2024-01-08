using System;
using System.Collections.Generic;
using Connection.Method;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Connection.Client;
using static Utils;

namespace Player {
    public class Player : MonoBehaviour {
        public static MapGenerator _mapGenerator;
        public static Info[] PlayersInfo;
        public static Info OwnPlayerInfo;
        public static PlayerFootPrint PlayerFootPrint;
        public static List<Material> FootprintsLogs;

        // private ChunkPos _currentChunkPos;
        [SerializeField] public Sprite[] faces;
        [SerializeField] public Sprite[] decorations;
        [SerializeField] public GameObject nameObj;
        [SerializeField] public GameObject decorateObj;


        private void Start() {
            _mapGenerator = gameObject.GetComponentInParent<MapGenerator>();

            // 開始渲染
            transform.position = OwnPlayerInfo.SpawnPoint;

            OwnPlayerInfo.PlayerColor = Colors[OwnPlayerInfo.UID];
            nameObj.GetComponent<TextMeshProUGUI>().text = OwnPlayerInfo.Name;

            if (OwnPlayerInfo.Decoration != 0) {
                transform.GetComponent<SpriteRenderer>().sprite = faces[OwnPlayerInfo.Face];
                decorateObj.GetComponent<SpriteRenderer>().sprite = decorations[OwnPlayerInfo.Decoration];
            } else {
                transform.GetComponent<SpriteRenderer>().sprite = faces[OwnPlayerInfo.Face];
            }

            PlayersInfo[OwnPlayerInfo.UID] = OwnPlayerInfo;
            StartCoroutine(ReceiveMessagesCoroutine());
        }

        private void FixedUpdate() {
            var logFootPrint = Move();
            SendMovementToServer(logFootPrint);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            // ReSharper disable once Unity.UnknownTag
            if (other.CompareTag("foot_print")) {
                if (other.name.Equals("self")) {
                    Debug.Log("You kill yourself !!!");
                    PlayerFootPrint.FootPrintStore.ForEach(Destroy);
                    PlayerFootPrint.FootPrintStore.Clear();
                    Suicide.SendReq(); // 自殺請求
                    SceneManager.LoadScene("StartMenu");
                } else {
                    // 設 other.name 為 uid
                    // TODO: 未確認有無 bug

                    Kill.SendReq(other.name); // 擊殺請求
                }
            } else if (other.CompareTag("square")) {
                var position = other.transform.position;
                var xPos = (sbyte)position.x;
                var yPos = (sbyte)position.y;
                var nowPos = new ChunkPos(xPos, yPos);

                UpdateMapBorder(xPos, yPos); // 更新邊界

                ref var currentMapState = ref QueryMap(nowPos.X, nowPos.Y);

                if (!PlayerFootPrint.FootprintRecord && (currentMapState & Map.Border) == 0)
                    PlayerFootPrint.FootprintRecord = true;

                if (PlayerFootPrint.FootprintRecord) {
                    currentMapState |= Map.FootPrint;
                    FootprintsLogs.Add(other.transform.GetComponent<Renderer>().material);
                }

                if (PlayerFootPrint.FootprintRecord && (currentMapState & Map.Border) > 0) {
                    PlayerFootPrint.FootprintRecord = false;
                    PlayerFootPrint.FootPrintStore.ForEach(Destroy);
                    PlayerFootPrint.FootPrintStore.Clear();

                    // 從 左上 到 右下 填
                    for (var y = Map.CurTop; y >= Map.CurBottom; --y)
                    for (var x = Map.CurLeft; x <= Map.CurRight; ++x) {
                        ref var tmpPos = ref QueryMap(x, y);
                        if ((tmpPos & (Map.Border | Map.FootPrint)) <= 0)
                            // tmpPos 不是領地 也不是 footprint
                            tmpPos |= Map.Empty;
                    }
                    
                    FootprintsLogs.ForEach(i => i.color = OwnPlayerInfo.PlayerColor);
                    FootprintsLogs.Clear();
                    
                    for (var y = Map.CurTop; y >= Map.CurBottom; --y)
                    for (var x = Map.CurLeft; x <= Map.CurRight; ++x) {
                        if ((QueryMap(x, y) & Map.Empty) <= 0) continue;
                        FloodFill(new ChunkPos(x, y)); // if (x, y) is empty , floodfill
                    }

                    for (var y = Map.CurTop; y >= Map.CurBottom; --y)
                    for (var x = Map.CurLeft; x <= Map.CurRight; ++x)
                        if ((QueryMap(x, y) & Map.FootPrint) > 0)
                            QueryMap(x, y) ^= Map.FootPrint | Map.Border;
                }
            }
        }

        private bool Move() {
            if (Input.GetKey(KeyCode.A))
                transform.Rotate(0, 0, OwnPlayerInfo.RotateSpeed * Time.deltaTime);
            else if (Input.GetKey(KeyCode.D))
                transform.Rotate(0, 0, -OwnPlayerInfo.RotateSpeed * Time.deltaTime);

            if (OutOfBoard()) return false;

            var logFootPrint = false;
            if (PlayerFootPrint.FootprintRecord)
                logFootPrint = SetFootPrint();


            transform.Translate(0, OwnPlayerInfo.ForwardSpeed, 0);

            return logFootPrint;
        }

        private void SendMovementToServer(bool logFootPrint) {
            var position = transform.position;
            // var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            // var timeBytes = BitConverter.GetBytes(time); // 8 bytes 
            var posXBytes = BitConverter.GetBytes(Convert.ToUInt16((position.x + 112) * 100)); // 2 bytes 
            var posYBytes = BitConverter.GetBytes(Convert.ToUInt16((position.y + 112) * 100)); // 2 bytes 
            var rotBytes = BitConverter.GetBytes(Convert.ToUInt16(transform.eulerAngles.z + 180)); // 2 bytes 
            var footprintBoolBytes = BitConverter.GetBytes(logFootPrint);

            UpdatePos.SendReq(
                CombineBytes(
                    // timeBytes,
                    posXBytes,
                    posYBytes,
                    rotBytes,
                    footprintBoolBytes
                )
            );
        }

        private bool OutOfBoard() {
            var nextPosition = transform.position + transform.up;
            return !(-112 <= nextPosition.x) || !(nextPosition.x <= 112) ||
                   !(-112 <= nextPosition.y) || !(nextPosition.y <= 112);
        }

        private bool SetFootPrint() {
            PlayerFootPrint.Timer += Time.deltaTime;
            if (PlayerFootPrint.Timer < PlayerFootPrint.UpdateInterval) return false;
            if (PlayerFootPrint.LastFootprintPos == transform.position) {
                PlayerFootPrint.Timer = 0;
                return false;
            }

            var curTransform = transform;
            var footPrint =
                Instantiate(_mapGenerator.foot_printPrefab, curTransform.position, curTransform.rotation,
                    curTransform.parent);
            footPrint.GetComponent<Renderer>().material.color = OwnPlayerInfo.PlayerColor;
            footPrint.name = "self";
            PlayerFootPrint.FootPrintStore.Add(footPrint);
            PlayerFootPrint.Timer = 0;
            PlayerFootPrint.LastFootprintPos = transform.position;

            return true;
        }

        private static void FloodFill(ChunkPos start) {
            var queue = new Queue<ChunkPos>(); // 下一個點
            var skip = false; // 判斷是不是需要算做領地
            var noTouchFootprint = true;
            queue.Enqueue(start); // 初始化第一個點
            QueryMap(start.X, start.Y) |= Map.Visited;

            // 還有要走的點沒有走過
            while (queue.Count != 0) {
                var pos = queue.Dequeue(); // pop
                var x = pos.X;
                var y = pos.Y;

                if (x + 1 <= Map.CurRight) {
                    // 優先檢查下一點是否在 range 裡 ，不會放進 visit 裡
                    var tmpPos = new ChunkPos((sbyte)(x + 1), y);
                    
                    // 如果是 empty 且沒有走過
                    if ((QueryMap((sbyte)(x + 1), y) & (Map.Empty | Map.Visited)) == Map.Empty) {
                        queue.Enqueue(tmpPos); // push
                        QueryMap((sbyte)(x + 1), y) ^= (Map.Visited | Map.Empty); // 轉成有 visit 過，並從empty移除
                    } else if (noTouchFootprint) {
                        // 優先檢查是否已被設定
                        if ((QueryMap((sbyte)(x + 1), y) & Map.FootPrint) == Map.FootPrint)
                            noTouchFootprint = false;
                    }
                } else {
                    skip = true;
                }

                if (x - 1 >= Map.CurLeft) {
                    var tmpPos = new ChunkPos((sbyte)(x - 1), y);
                    if ((QueryMap((sbyte)(x - 1), y) & (Map.Empty | Map.Visited)) == Map.Empty) {
                        queue.Enqueue(tmpPos); // push
                        QueryMap((sbyte)(x - 1), y) ^= Map.Visited | Map.Empty;
                    } else if (noTouchFootprint) {
                        // 優先檢查是否已被設定
                        if ((QueryMap((sbyte)(x - 1), y) & Map.FootPrint) == Map.FootPrint)
                            noTouchFootprint = false;
                    }
                } else {
                    skip = true;
                }

                if (y + 1 <= Map.CurTop) {
                    var tmpPos = new ChunkPos(x, (sbyte)(y + 1));
                    if ((QueryMap(x, (sbyte)(y + 1)) & (Map.Empty | Map.Visited)) == Map.Empty) {
                        queue.Enqueue(tmpPos); // push
                        QueryMap(x, (sbyte)(y + 1)) ^= Map.Visited | Map.Empty;
                    } else if (noTouchFootprint) {
                        // 優先檢查是否已被設定
                        if ((QueryMap(x, (sbyte)(y + 1)) & Map.FootPrint) == Map.FootPrint)
                            noTouchFootprint = false;
                    }
                } else {
                    skip = true;
                }

                if (y - 1 >= Map.CurBottom) {
                    var tmpPos = new ChunkPos(x, (sbyte)(y - 1));
                    if ((QueryMap(x, (sbyte)(y - 1)) & (Map.Empty | Map.Visited)) == Map.Empty) {
                        queue.Enqueue(tmpPos); // push
                        QueryMap(x, (sbyte)(y - 1)) ^= Map.Visited | Map.Empty;
                    } else if (noTouchFootprint) {
                        // 優先檢查是否已被設定
                        if ((QueryMap(x, (sbyte)(y - 1)) & Map.FootPrint) == Map.FootPrint)
                            noTouchFootprint = false;
                    }
                } else {
                    skip = true;
                }
            }

            skip |= noTouchFootprint;
            // if (skip or no_touch_footprint) don't draw
            var counter = 0;
            var byteAry = new List<byte>();
            for (var y = Map.CurTop; y >= Map.CurBottom; --y)
            for (var x = Map.CurLeft; x <= Map.CurRight; ++x) {
                ref var tmpPos = ref QueryMap(x, y);

                if ((tmpPos & Map.Visited) <= 0) continue;
                tmpPos ^= Map.Visited;

                if (skip) continue;

                QueryMap(x, y) |= Map.Border;

                var index = BlockPosToChunkIndex(x, y); // 要渲染方塊的 chunk index
                var chunk = MapGenerator.Chunks[index]; // 透過 index 取得 chunk
                if (chunk == null) // 如果 chunk 尚未被渲染
                    chunk = _mapGenerator.GenerateChunk(BlockPosToChunkPos(x, y));

                chunk.transform
                    .GetChild(PosToIndex(x, y))
                    .GetComponent<Renderer>().material.color = OwnPlayerInfo.PlayerColor;

                byteAry.Add(BitConverter.GetBytes(x)[0]);
                byteAry.Add(BitConverter.GetBytes(y)[0]);
                ++counter;
            }

            // 送出絕對座標
            if (byteAry.Count != 0) {
                ClaimBlock.SendReq(byteAry.ToArray());
                OwnPlayerInfo.Sum += counter;
            }
        }
    }
}