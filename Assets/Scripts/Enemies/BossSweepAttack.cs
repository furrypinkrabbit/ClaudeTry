using UnityEngine;
using GuJian.Pawns;
using GuJian.Pawns.Parts;

namespace GuJian.Enemies {
    /// <summary>
    /// BOSS 的横扫攻击。作为 Pawn 的一个 Part 挂在斗拱上。
    /// 监听 AttackHeavy 意图——犰头(前方) 扮演 sweepArcDeg 半径 sweepRadius 的弧形检测器。
    /// 不涉 animation;纯数值,方便 DevTools 测试。
    /// </summary>
    public class BossSweepAttack : PawnPartBase {
        [SerializeField] float sweepRadius = 4.5f;
        [SerializeField] float sweepArcDeg = 140f;
        [SerializeField] float damage      = 28f;
        [SerializeField] string playerTag  = "Player";
        [SerializeField] float windupTime  = 0.35f;

        float _pendingUntil = -1f;

        public override bool HandlesIntent(PawnIntentKind k) => k == PawnIntentKind.AttackHeavy;

        public override void HandleIntent(in PawnIntent _) {
            // 打开 windup,在 Update 里到点结算
            _pendingUntil = Time.time + windupTime;
        }

        void Update() {
            if (_pendingUntil < 0f || Time.time < _pendingUntil) return;
            _pendingUntil = -1f;
            var p = GameObject.FindWithTag(playerTag);
            if (p == null) return;
            Vector3 to = p.transform.position - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist > sweepRadius) return;
            // 角度检测:玩家是否在 BOSS 正前方的弧内
            Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
            float ang = Vector3.Angle(fwd, to.normalized);
            if (ang * 2f > sweepArcDeg) return;
            var h = p.GetComponentInChildren<PawnHealth>();
            h?.TakeDamage(damage);
        }
    }
}
