using System;
using UnityEngine;
using GuJian.Pawns;

namespace GuJian.Enemies {
    /// <summary>
    /// 第二关 立柱:缓慢移动,偏元地点原地【1.立柱砂击地面】。
    /// 状态机: Approach → (达到攻击距离) → Slam → Recover → Approach
    /// Slam 预警期:不可移动但可被打——打断 一次=推回 Approach。
    /// </summary>
    public class PillarBrain : EnemyBrainBase {
        enum State { Approach, Telegraph, Slam, Recover }

        [SerializeField] float approachSpeedMul = 0.55f;
        [SerializeField] float attackRange      = 2.2f;
        [SerializeField] float telegraphTime    = 0.8f;
        [SerializeField] float slamDuration     = 0.25f;
        [SerializeField] float recoverTime      = 0.9f;
        [Tooltip("缓慢移动中每隔多久 回头看一下玩家(模拟晃动)。")]
        [SerializeField] float bobPeriod        = 0.5f;

        State _st;
        float _stTimer;
        float _bobTimer;

        public override void OnSpawn() {
            base.OnSpawn();
            _st = State.Approach; _stTimer = 0f; _bobTimer = 0f;
        }

        protected override void OnThink(IPawn self, Transform player, Vector3 to,
                                        float dist, float dt, Action<PawnIntent> emit) {
            _stTimer += dt; _bobTimer += dt;
            Vector2 dirXZ = new Vector2(to.x, to.z).normalized;
            emit(PawnIntent.Look(dirXZ));

            switch (_st) {
                case State.Approach:
                    // 晃动:每 bobPeriod 抒一下(等效于横向微调),制造“慢晃不稳”观感
                    float bob = Mathf.Sin(_bobTimer * Mathf.PI * 2f / bobPeriod) * 0.4f;
                    Vector2 drift = new Vector2(-dirXZ.y, dirXZ.x) * bob;
                    emit(PawnIntent.Move(dirXZ * approachSpeedMul + drift));
                    if (dist <= attackRange) { _st = State.Telegraph; _stTimer = 0f; }
                    break;

                case State.Telegraph:
                    // 预警:站着,主机 表现上由特效/SpriteAnim 颜色改变
                    emit(PawnIntent.Move(Vector2.zero));
                    if (_stTimer >= telegraphTime) { _st = State.Slam; _stTimer = 0f; emit(PawnIntent.Heavy()); }
                    break;

                case State.Slam:
                    emit(PawnIntent.Move(Vector2.zero));
                    if (_stTimer >= slamDuration) { _st = State.Recover; _stTimer = 0f; }
                    break;

                case State.Recover:
                    emit(PawnIntent.Move(Vector2.zero));
                    if (_stTimer >= recoverTime) { _st = State.Approach; _stTimer = 0f; }
                    break;
            }
        }
    }
}
