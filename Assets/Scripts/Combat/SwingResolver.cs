using UnityEngine;
using GuJian.Structures;
using GuJian.Tools;

namespace GuJian.Combat {
    /// <summary>
    /// 已改耐用的扇形击中解算器。用 OverlapSphere 筛选后用角度过滤。
    /// 所有工具共用，统一命中结算行为。
    /// </summary>
    public static class SwingResolver {
        static readonly Collider[] _buf = new Collider[24];

        public struct SwingArgs {
            public Vector3 origin;
            public Vector3 forward;
            public float   range;
            public float   arcDeg;
            public float   knockback;
            public float   extraDamageMul;
            public ToolBase tool;
            public ToolContext ctx;
        }

        public static int Resolve(in SwingArgs a) {
            int n = Physics.OverlapSphereNonAlloc(a.origin, a.range, _buf);
            int hits = 0;
            float cosHalf = Mathf.Cos(a.arcDeg * 0.5f * Mathf.Deg2Rad);
            for (int i = 0; i < n; i++) {
                var col = _buf[i];
                if (!col || !col.TryGetComponent<IRepairable>(out var repairable)) continue;
                var to = col.transform.position - a.origin; to.y = 0;
                if (to.sqrMagnitude < 0.01f) { HitOne(repairable, col, a); hits++; continue; }
                var dot = Vector3.Dot(a.forward.normalized, to.normalized);
                if (dot >= cosHalf) { HitOne(repairable, col, a); hits++; }
            }
            return hits;
        }

        static void HitOne(IRepairable r, Collider col, in SwingArgs a) {
            var tool = a.tool;
            bool crit = Random.value < tool.GetCritChance(in a.ctx);
            float dmg = tool.GetDamage(r.Material, in a.ctx, crit) * a.extraDamageMul;
            var hit = new RepairHitInfo {
                amount = Mathf.Max(1, Mathf.RoundToInt(dmg)),
                isCritical = crit,
                attacker = tool.Owner?.Transform.gameObject,
                knockbackDir = (col.transform.position - a.origin).normalized,
                knockbackForce = a.knockback,
            };
            r.ApplyHit(hit);
        }
    }

    /// <summary>ToolBase 的辅助扩展——收敛调用点。</summary>
    public static class ToolBaseCombatExt {
        public static float GetDamage(this ToolBase t, StructureMaterial m, in ToolContext ctx, bool crit) =>
            t.ComputeDamage(m, t.Data.baseDamage, ctx, crit);
        public static float GetCritChance(this ToolBase t, in ToolContext ctx) =>
            t.ComputeCritChance(in ctx);
    }
}
