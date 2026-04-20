using System.Collections;
using UnityEngine;
using GuJian.Core;
using GuJian.Structures;

namespace GuJian.Rooms {
    /// <summary>
    /// 按房间的 RoomData.waves 刷怪。当房间内全部修复时开门。
    /// </summary>
    public class WaveDirector : MonoBehaviour {
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] RoomData data;

        int _aliveCount;

        void OnEnable()  { EventBus.Subscribe<StructureRepairedEvent>(OnRepaired); }
        void OnDisable() { EventBus.Unsubscribe<StructureRepairedEvent>(OnRepaired); }

        void Start() { StartCoroutine(RunWaves()); }

        IEnumerator RunWaves() {
            foreach (var w in data.waves) {
                yield return new WaitForSeconds(w.startDelay);
                foreach (var e in w.entries) {
                    for (int i = 0; i < e.count; i++) {
                        SpawnOne(e.structure);
                        yield return new WaitForSeconds(0.08f);
                    }
                }
                // 等浪清
                while (_aliveCount > 0) yield return null;
            }
            // 本房间完源→通知 RoomManager 推进
            EventBus.Publish(new RoomClearedEvent(data.roomId));
        }

        void SpawnOne(StructureData sd) {
            if (sd == null || spawnPoints == null || spawnPoints.Length == 0) return;
            var sp = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

            GameObject prefab = Resources.Load<GameObject>("Prefabs/StructureEnemyBase");

            if (prefab != null)
            {
                GameObject inst = Instantiate(prefab, sp.position, sp.rotation);
            }

            // var inst = Instantiate(sd.portrait != null ? Resources.Load<GameObject>("Prefabs/StructureEnemyBase") : null,
            //                      sp.position, sp.rotation);
            // 实际项目里应从 data 直接实例化它的专属 prefab
            _aliveCount++;
        }

        void OnRepaired(StructureRepairedEvent _) { _aliveCount = Mathf.Max(0, _aliveCount - 1); }
    }

    public readonly struct RoomClearedEvent {
        public readonly string roomId;
        public RoomClearedEvent(string id) { roomId = id; }
    }
}
