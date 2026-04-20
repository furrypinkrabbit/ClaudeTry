using UnityEngine;

namespace GuJian.Structures {
    /// <summary>残缺结构敌人的设计数据。</summary>
    [CreateAssetMenu(menuName = "GuJian/Structure/StructureData", fileName = "NewStructureData")]
    public class StructureData : ScriptableObject {
        [Header("身份")]
        public string       structureId;
        public string       displayName;
        [TextArea] public string culturalNote;
        public Sprite       portrait;

        [Header("形态与材质")]
        public StructureType     type;
        public StructureMaterial material;
        [Tooltip("初始残缺度 = 血量。")]
        public int   maxBrokenness = 40;
        [Tooltip("残缺阶段展示用 Sprite：第0个是最破，最后一个是完整。")]
        public Sprite[] repairStages;

        [Header("AI 行为")]
        public float moveSpeed = 2.2f;
        public float contactDamage = 8f;

        [Header("给予")]
        public int craftsmanshipReward = 6;
        public DropEntry[] drops;
    }

    [System.Serializable]
    public struct DropEntry {
        public GameObject prefab;
        [Range(0,1)] public float chance;
    }
}
