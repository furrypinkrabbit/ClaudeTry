using UnityEngine;
using GuJian.Structures;

namespace GuJian.Rooms {
    [CreateAssetMenu(menuName = "GuJian/Room/RoomData", fileName = "NewRoomData")]
    public class RoomData : ScriptableObject {
        public string roomId;
        public string displayName;
        [TextArea] public string cultureNote;
        public RoomType type;
        public GameObject prefab;

        [Header("刷怪表（结构敌人）")]
        public SpawnWave[] waves;
    }

    [System.Serializable]
    public class SpawnWave {
        public SpawnEntry[] entries;
        public float startDelay = 0.5f;
    }

    [System.Serializable]
    public struct SpawnEntry {
        public StructureData structure;
        public int count;
    }
}
