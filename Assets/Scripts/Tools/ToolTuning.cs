using UnityEngine;
using GuJian.Structures;

namespace GuJian.Tools {
    /// <summary>
    /// 配件底类。继承他写具体配件逻辑。
    /// 例子子类： IronFerruleTuning（+味展射/伤害）、JadeInsetTuning（+暴击）。
    /// </summary>
    public abstract class ToolTuning : ScriptableObject, IToolModifier {
        public string tuningId;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public int price = 100;

        public virtual void ModifyDamage(StructureMaterial m, ref float d) { }
        public virtual void ModifyCritChance(ref float c) { }
        public virtual void ModifyCritDamage(ref float c)  { }
        public virtual void ModifyCooldown(ref float cd)   { }
        public virtual void ModifyRange(ref float r)       { }
    }

    /// <summary>鸢钉铜锥：+10% 攻击、+5% 暴击。</summary>
    [CreateAssetMenu(menuName = "GuJian/Tool/Tuning/IronFerrule")]
    public class IronFerruleTuning : ToolTuning {
        public float damageAdd = 0.10f;
        public float critAdd   = 0.05f;
        public override void ModifyDamage(StructureMaterial m, ref float d) { d *= 1f + damageAdd; }
        public override void ModifyCritChance(ref float c) { c += critAdd; }
    }

    /// <summary>洛铃紫锤：+20% 对玉材伤害。</summary>
    [CreateAssetMenu(menuName = "GuJian/Tool/Tuning/JadeEater")]
    public class JadeEaterTuning : ToolTuning {
        public override void ModifyDamage(StructureMaterial m, ref float d) {
            if (m == StructureMaterial.Jade) d *= 1.2f;
        }
    }
}
