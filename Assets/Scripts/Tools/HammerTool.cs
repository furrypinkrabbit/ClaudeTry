using UnityEngine;
using GuJian.Combat;

namespace GuJian.Tools {
    /// <summary>
    /// 锤子工具实现。前方扇形击打，森锹 IRepairable。
    /// 排列：普攻 / 重击非传把显示 / 蓄力放大。
    /// </summary>
    public class HammerTool : ToolBase {
        [Header("语义特效")]
        [SerializeField] float heavyKnockback   = 6f;
        [SerializeField] float heavyDamageMul   = 1.8f;
        [SerializeField] float heavyCooldownMul = 1.4f;

        float _chargeStart = -1f;

        public override void Trigger(ToolActionType type, in ToolContext ctx) {
            switch (type) {
                case ToolActionType.Primary:
                    if (cdTimer > 0f) return;
                    DoSwing(ctx, damageMul: 1f, rangeMul: 1f, knockback: 1f);
                    cdTimer = ComputeCooldown();
                    break;
                case ToolActionType.Heavy:
                    if (cdTimer > 0f) return;
                    DoSwing(ctx, damageMul: heavyDamageMul, rangeMul: 1.15f, knockback: heavyKnockback);
                    cdTimer = ComputeCooldown() * heavyCooldownMul;
                    break;
                case ToolActionType.ChargeStart:
                    if (data.supportsCharge) _chargeStart = Time.time;
                    break;
                case ToolActionType.ChargeRelease:
                    if (data.supportsCharge && _chargeStart > 0) {
                        float t = Mathf.Clamp(Time.time - _chargeStart, 0, data.chargeMaxSeconds);
                        float k = t / data.chargeMaxSeconds;      // 0..1
                        float dmg = Mathf.Lerp(1f, data.chargeDamageMul, k);
                        float rng = Mathf.Lerp(1f, data.chargeRangeMul,  k);
                        DoSwing(ctx, damageMul: dmg, rangeMul: rng, knockback: 2f + 4f * k);
                        cdTimer = ComputeCooldown() * 1.5f;
                        _chargeStart = -1f;
                    }
                    break;
                case ToolActionType.Special:
                    // 预留给将来特殊技能
                    break;
            }
        }

        void DoSwing(in ToolContext ctx, float damageMul, float rangeMul, float knockback) {
            var hit = new SwingResolver.SwingArgs {
                origin    = ctx.Origin + Vector3.up * 0.8f,
                forward   = ctx.Facing,
                arcDeg    = data.swingArcDeg,
                range     = ComputeRange() * rangeMul,
                knockback = knockback,
                tool      = this,
                ctx       = ctx,
                extraDamageMul = damageMul,
            };
            SwingResolver.Resolve(hit);
        }
    }
}
