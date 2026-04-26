using System;
using UnityEngine;

namespace GuJian.Tools {
    [CreateAssetMenu(menuName = "GuJian/Tool/ToolData", fileName = "NewToolData")]
    public class ToolData : ScriptableObject {
        [Header("标识")]
        public string toolId;
        public string displayName;
        [TextArea] public string flavorText;
        public Sprite icon;
        public ToolKind kind = ToolKind.Hammer;

        [Header("核心数值")]
        public float baseDamage     = 18f;
        public float swingCooldown  =  0.5f;
        public float range          =  2.0f;
        public float swingArcDeg    = 110f;
        public float staminaCost    = 12f;

        [Header("蓄力")]
        public bool  supportsCharge     = true;
        public float chargeMaxSeconds  = 1.2f;
        public float chargeDamageMul   = 2.0f;
        public float chargeRangeMul    = 1.2f;

        [Header("材质亲和")]
        public MaterialAffinity[] affinities;

        [Header("配件")]
        public int     tuningSlots = 2;
        public ToolTuning[] defaultTunings;

        [Header("运行时")]
        /// <summary>预制体根节点必须挂 ToolBase 的子类。</summary>
        public GameObject runtimePrefab;
    }
}
