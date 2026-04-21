using UnityEngine;

namespace GuJian.LevelFlow {
    /// <summary>
    /// 一单份 Campaign = 按顺序排列的几个关卡。
    /// 默认流程:第一关 宫墙 → 第二关 立柱 → 第三关 瓦件 → BOSS 斗拱。
    /// </summary>
    [CreateAssetMenu(menuName = "GuJian/LevelFlow/CampaignDef", fileName = "Campaign")]
    public class CampaignDef : ScriptableObject {
        public string campaignId;
        public string displayName;
        public LevelStageDef[] stages;
    }
}
