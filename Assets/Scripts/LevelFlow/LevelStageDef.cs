using UnityEngine;
using GuJian.Structures;

namespace GuJian.LevelFlow {
    /// <summary>
    /// 一个关卡关。用 ScriptableObject 组合为 Campaign 打包。
    /// 普通关:kind=Wave + waves。BOSS 关:kind=Boss + bossPrefab。
    /// </summary>
    [CreateAssetMenu(menuName = "GuJian/LevelFlow/StageDef", fileName = "Stage")]
    public class LevelStageDef : ScriptableObject {
        public enum Kind { Wave, Boss }

        [Header("身份")]
        public string stageId;
        public string displayName;
        [TextArea] public string storyNote;

        public Kind kind = Kind.Wave;

        [Header("Wave 关:刷怪")]
        public EnemyArchetype enemyArchetype;
        public StructureData  enemyData;           // 被刷的残缺结构数据
        public GameObject     enemyPrefab;         // 必须带 StructureEnemyPawn + 对应 Brain 与 AIPawnController
        public int  poolPrewarm = 8;
        public SpawnBurst[]   bursts;

        [Header("Boss 关")]
        public GameObject bossPrefab;              // 带 DougongBossBrain
        public GameObject fallingTilePrefab;       // 传给 Brain,方便数据驱动
        public int fallingTilePoolPrewarm = 12;
    }

    /// <summary>关卡中用哪种小怪,便于未来扩展割裂/类型切换。</summary>
    public enum EnemyArchetype {
        Brick  = 0,  // 第一关 墙砖
        Pillar = 1,  // 第二关 立柱
        Tile   = 2,  // 第三关 瓦件
        Custom = 99,
    }

    /// <summary>一波刷怪(一波批几个 + 主间隔)。</summary>
    [System.Serializable]
    public struct SpawnBurst {
        public int   count;
        public float startDelay;
        public float spreadRadius;
    }
}
