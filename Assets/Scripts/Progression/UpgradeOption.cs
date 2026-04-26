using UnityEngine;
using GuJian.Pawns;
using GuJian.Pawns.Parts;

namespace GuJian.Progression {
    /// <summary>
    /// 单个升级选项（卡牌）。加新升级只要新建一个这个 SO。
    /// </summary>
    [CreateAssetMenu(menuName = "GuJian/Progression/UpgradeOption", fileName = "NewUpgrade")]
    public class UpgradeOption : ScriptableObject, IUpgradeEffect {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public UpgradeCategory category;
        [Range(1,3)] public int rarity = 1;

        [Header("效果（任选其一非零）")]
        public float damageAddPct    = 0f;   // 榫卯加固
        public float maxHpAdd        = 0f;   // 台基筑基
        public float moveSpeedMul    = 0f;   // 曲径通幽（0.1 = +10%）
        public float shieldAdd       = 0f;   // 瓦当护佑
        public float critChanceAdd   = 0f;   // 斗拱承力
        public float critDamageAdd   = 0f;

        public void Apply(IPawn pawn) {
            if (pawn.GetPart<PawnCombat>() is { } c) {
                c.DamageMul        += damageAddPct;
                c.CritChanceAdd    += critChanceAdd;
                c.CritDamageAdd    += critDamageAdd;
            }
            if (pawn.GetPart<PawnHealth>() is { } h) {
                h.MaxHpBonus += maxHpAdd;
                h.ShieldBonus += shieldAdd;
                h.SyncMax();
            }
            if (pawn.GetPart<PawnMovement>() is { } m) {
                m.MoveSpeedMul *= (1f + moveSpeedMul);
            }
        }
    }
}
