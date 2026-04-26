using System.Collections.Generic;
using UnityEngine;
using GuJian.Pawns;
using GuJian.Structures;

namespace GuJian.Tools {
    /// <summary>
    /// 工具通用底实现。负责：
    ///  - 维护冷却
    ///  - 聚合配件修饰器
    ///  - 调用 SwingResolver 做击中结算
    /// </summary>
    public abstract class ToolBase : MonoBehaviour, ITool {
        [SerializeField] protected ToolData data;
        public ToolData Data => data;
        public IPawn Owner { get; private set; }

        protected readonly List<IToolModifier> modifiers = new();
        protected float cdTimer;

        public virtual void OnEquip(IPawn owner) {
            Owner = owner;
            modifiers.Clear();
            if (data.defaultTunings != null)
                foreach (var t in data.defaultTunings) if (t != null) modifiers.Add(t);
        }

        public virtual void OnUnequip() { Owner = null; }

        public virtual void Tick(float dt) {
            if (cdTimer > 0f) cdTimer -= dt;
        }

        public abstract void Trigger(ToolActionType type, in ToolContext ctx);

        /// <summary>
        /// 给子类用：结合配件与亲和的最终伤害计算。
        /// </summary>
        public float ComputeDamage(StructureMaterial mat, float baseDamage, in ToolContext ctx, bool isCritical) {
            float d = baseDamage * ctx.DamageMul;
            // 材质亲和
            if (data.affinities != null) {
                for (int i = 0; i < data.affinities.Length; i++)
                    if (data.affinities[i].material == mat) d *= data.affinities[i].damageMul;
            }
            // 配件修饰
            for (int i = 0; i < modifiers.Count; i++) modifiers[i].ModifyDamage(mat, ref d);
            if (isCritical) {
                float critDmg = 1.5f + ctx.CritDamageAdd;
                for (int i = 0; i < modifiers.Count; i++) modifiers[i].ModifyCritDamage(ref critDmg);
                d *= critDmg;
            }
            return d;
        }

        public float ComputeCritChance(in ToolContext ctx) {
            float c = ctx.CritChanceAdd;
            for (int i = 0; i < modifiers.Count; i++) modifiers[i].ModifyCritChance(ref c);
            return Mathf.Clamp01(c);
        }

        protected float ComputeRange() {
            float r = data.range;
            for (int i = 0; i < modifiers.Count; i++) modifiers[i].ModifyRange(ref r);
            return r;
        }

        protected float ComputeCooldown() {
            float cd = data.swingCooldown;
            for (int i = 0; i < modifiers.Count; i++) modifiers[i].ModifyCooldown(ref cd);
            return cd;
        }
    }
}
