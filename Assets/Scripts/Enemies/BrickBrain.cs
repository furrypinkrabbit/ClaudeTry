using System;
using UnityEngine;
using GuJian.Pawns;

namespace GuJian.Enemies {
    /// <summary>
    /// 第一关 宫墙砖:只会直线冲撞。
    /// 状态机: Wander(进入视野前) → Lock(锁定方向) → Charge(直线冲)— 到达、撞到都重置为 Lock。
    /// 行为关键点:冲撞开始后方向不再调整——玩家可预判、侧滑。
    /// </summary>
    public class BrickBrain : EnemyBrainBase {
        enum State { Wander, Lock, Charge }

        [Header("运动")]
        [SerializeField] float wanderInterval  = 1.6f;   // Wander 中每隔多久换一次随机方向
        [SerializeField] float lockTime        = 0.45f;  // 锁定方向的可见预警时间
        [SerializeField] float chargeMaxTime   = 1.2f;   // 冲撞最长时间(避免卡在增里)
        [SerializeField] float chargeArriveEps = 0.8f;   // 冲到目标容差
        [SerializeField] float chargeSpeedMul  = 1.75f;  // 冲撞时速度倍率

        State   _st;
        float   _stTimer;
        Vector2 _wanderDir;
        Vector2 _chargeDir;
        Vector3 _chargeTarget;

        public override void OnSpawn() {
            base.OnSpawn();
            _st = State.Wander;
            _stTimer = 0f;
            _wanderDir = Vector2.zero;
        }

        protected override void OnThink(IPawn self, Transform player, Vector3 to,
                                        float dist, float dt, Action<PawnIntent> emit) {
            _stTimer += dt;

            switch (_st) {
                case State.Wander:
                    // 见到玩家 → 锁定
                    _st = State.Lock; _stTimer = 0f;
                    _chargeDir = new Vector2(to.x, to.z).normalized;
                    _chargeTarget = player.position;
                    emit(PawnIntent.Look(_chargeDir));
                    emit(PawnIntent.Move(Vector2.zero));
                    break;

                case State.Lock:
                    // 短预警:保持朝向,让玩家读出“要冲了”的信息
                    emit(PawnIntent.Look(_chargeDir));
                    emit(PawnIntent.Move(Vector2.zero));
                    if (_stTimer >= lockTime) {
                        _st = State.Charge; _stTimer = 0f;
                    }
                    break;

                case State.Charge:
                    // 直线全速前冲,中途不改方向
                    emit(PawnIntent.Look(_chargeDir));
                    emit(PawnIntent.Move(_chargeDir * chargeSpeedMul));
                    bool arrived = Vector3.Distance(self.Transform.position, _chargeTarget) <= chargeArriveEps;
                    if (arrived || _stTimer >= chargeMaxTime) {
                        // 摧撞伤害由 PawnCombat.contactDamage 或 StructureData.contactDamage 处理
                        _st = State.Wander; _stTimer = 0f;
                    }
                    break;
            }
        }
    }
}
