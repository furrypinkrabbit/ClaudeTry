using System.Collections.Generic;
using UnityEngine;

namespace GuJian.Rooms {
    /// <summary>
    /// 一条房间链的抽样池。RoomManager 从此决定每次出到哪种房间。
    /// </summary>
    [CreateAssetMenu(menuName = "GuJian/Room/RoomSet", fileName = "RoomSet")]
    public class RoomSet : ScriptableObject {
        public List<RoomData> outerYards;
        public List<RoomData> corridors;
        public List<RoomData> greatHalls;
        public List<RoomData> sideWings;
        public List<RoomData> events;
        public List<RoomData> bosses;

        public RoomData Pick(RoomType type, System.Random rng) {
            var list = PickList(type);
            if (list == null || list.Count == 0) return null;
            return list[rng.Next(list.Count)];
        }

        List<RoomData> PickList(RoomType t) => t switch {
            RoomType.OuterYard => outerYards,
            RoomType.Corridor  => corridors,
            RoomType.GreatHall => greatHalls,
            RoomType.SideWing  => sideWings,
            RoomType.Event     => events,
            RoomType.Boss      => bosses,
            _ => null,
        };
    }
}
