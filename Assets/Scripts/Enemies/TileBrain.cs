using System;
using UnityEngine;
using GuJian.Pawns;

namespace GuJian.Enemies {
    /// <summary>
    /// 第三关 瓦当/筒瓦:不停上下弹跳,偶尔水平移动。
    /// 因为 CharacterController 的 y 位移由 PawnMovement 内部的重力控制,我们不能直接发 intent 控制 y。
    /// 解法:把弹跳用一个可视化子节点(精灵/模型)在内部 y 上下欢折——主体接触器依然在地面平面。
    /// </summary>
    public class TileBrain : EnemyBrainBase {
        enum MoveMode { Hop, Strafe }

        [Header("弹跳")]
        [SerializeField] Transform bobChild;          // 可选:这个子节点在 y 方向上下欢折以做视觉“弹跳”
        [SerializeField] float bobHeight    = 0.6f;
        [SerializeField] float bobFrequency = 2.6f;

        [Header("移动")]
        [SerializeField] float strafeChance  = 0.35f;  // 每轮结束时会不会平移
        [SerializeField] float hopDuration   = 1.1f;
        [SerializeField] float strafeDuration= 0.9f;
        [SerializeField] float strafeSpeedMul= 1.2f;

        MoveMode _mode;
        float    _stTimer;
        Vector2  _strafeDir;
        float    _bobPhase;

        public override void OnSpawn() {
            base.OnSpawn();
            _mode = MoveMode.Hop; _stTimer = 0f;
            _bobPhase = UnityEngine.Random.value * Mathf.PI * 2f;
        }

        protected override void OnThink(IPawn self, Transform player, Vector3 to,
                                        float dist, float dt, Action<PawnIntent> emit) {
            _stTimer += dt;

            // 视觉弹跳(y 上下)——真实业逻还在地面平面
            if (bobChild != null) {
                float t = Time.time * bobFrequency + _bobPhase;
                float y = Mathf.Abs(Mathf.Sin(t)) * bobHeight;
                var p = bobChild.localPosition;
                bobChild.localPosition = new Vector3(p.x, y, p.z);
            }

            Vector2 dirXZ = new Vector2(to.x, to.z).normalized;
            emit(PawnIntent.Look(dirXZ));

            switch (_mode) {
                case MoveMode.Hop:
                    // 向玩家慢接近,配合视觉弹跳
                    emit(PawnIntent.Move(dirXZ * 0.8f));
                    if (_stTimer >= hopDuration) {
                        _mode = UnityEngine.Random.value < strafeChance ? MoveMode.Strafe : MoveMode.Hop;
                        _stTimer = 0f;
                        if (_mode == MoveMode.Strafe) {
                            // 正交方向(左 或 右)水平移动
                            Vector2 perp = new Vector2(-dirXZ.y, dirXZ.x);
                            _strafeDir = UnityEngine.Random.value < 0.5f ? perp : -perp;
                        }
                    }
                    break;

                case MoveMode.Strafe:
                    emit(PawnIntent.Move(_strafeDir * strafeSpeedMul));
                    if (_stTimer >= strafeDuration) { _mode = MoveMode.Hop; _stTimer = 0f; }
                    break;
            }
        }
    }
}
