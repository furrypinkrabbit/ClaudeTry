using System.Collections.Generic;
using UnityEngine;
using GuJian.Core;

namespace GuJian.Rooms {
    /// <summary>
    /// 线性随机房间管理。序列：Outer -> Corridor -> Hall -> SideWing -> Event -> ... -> Boss
    /// 在 plan Day2 的基础上演化为“方块序列”，可扩展为分支。
    /// </summary>
    public class RoomManager : MonoBehaviour {
        [SerializeField] RoomSet set;
        [SerializeField] Transform spawnRoot;
        [SerializeField] RoomType[] sequence = {
            RoomType.OuterYard, RoomType.Corridor, RoomType.GreatHall,
            RoomType.SideWing, RoomType.Event, RoomType.GreatHall, RoomType.Boss
        };
        [SerializeField] int seed = 0;

        public int Index { get; private set; } = -1;
        public RoomData Current { get; private set; }
        GameObject _currentInstance;
        System.Random _rng;

        public void Begin() {
            _rng = seed == 0 ? new System.Random() : new System.Random(seed);
            Index = -1;
            Next();
        }

        public bool Next() {
            Index++;
            if (Index >= sequence.Length) return false;
            if (_currentInstance) Destroy(_currentInstance);
            Current = set.Pick(sequence[Index], _rng);
            if (Current?.prefab != null)
                _currentInstance = Instantiate(Current.prefab, spawnRoot);
            EventBus.Publish(new RoomEnteredEvent(Current?.roomId ?? "", Index));
            return true;
        }

        public bool HasNext => Index + 1 < sequence.Length;
        public bool IsLast  => Index == sequence.Length - 1;
    }
}
