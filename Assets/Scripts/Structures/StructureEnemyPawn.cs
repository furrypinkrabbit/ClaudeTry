using System;
using UnityEngine;
using GuJian.Core;
using GuJian.Pawns;
using GuJian.Pawns.Parts;

namespace GuJian.Structures {
    /// <summary>
    /// 残缺结构敌人。没有传统 "血量"，而是 "残缺度"，打它=修补。
    /// 自己也是一个 IPawn（由 AIPawnController 驱动）。
    /// </summary>
    [RequireComponent(typeof(PawnMovement))]
    public class StructureEnemyPawn : PawnBase, IRepairable {
        [SerializeField] StructureData data;
        [SerializeField] SpriteRenderer stageRenderer;

        int _current;
        public int CurrentBrokenness => _current;
        public int MaxBrokenness     => data.maxBrokenness;
        public StructureMaterial Material => data.material;
        public StructureData Data => data;
        public event Action<IRepairable> OnRepaired;

        protected override void Awake() {
            base.Awake();
            _current = data.maxBrokenness;
            if (GetPart<PawnMovement>() is { } mv) mv.MoveSpeedMul = data.moveSpeed / 4.0f; // 4=base
            UpdateStageSprite();
        }

        public void ApplyHit(in RepairHitInfo hit) {
            if (!IsAlive) return;
            _current = Mathf.Max(0, _current - hit.amount);
            EventBus.Publish(new StructureHitEvent(gameObject, hit.attacker, hit.amount, hit.isCritical));
            UpdateStageSprite();
            // 击退
            if (hit.knockbackForce > 0f && GetPart<PawnMovement>() is { } m) {
                // 简单自动位移（生产级可改成 impulse）
                transform.position += hit.knockbackDir.normalized * (hit.knockbackForce * 0.1f);
            }
            if (_current == 0) RepairComplete();
        }

        void UpdateStageSprite() {
            if (stageRenderer == null || data.repairStages == null || data.repairStages.Length == 0) return;
            // 0..MaxBrokenness → stage index（0即完全修复）
            float t = 1f - (float)_current / data.maxBrokenness; // 0 破 → 1 好
            int idx = Mathf.Clamp(Mathf.RoundToInt(t * (data.repairStages.Length - 1)), 0, data.repairStages.Length - 1);
            stageRenderer.sprite = data.repairStages[idx];
        }

        void RepairComplete() {
            Kill();
            OnRepaired?.Invoke(this);
            EventBus.Publish(new StructureRepairedEvent(gameObject, data.craftsmanshipReward));
            // 掉落
            if (data.drops != null) {
                foreach (var d in data.drops) {
                    if (d.prefab != null && UnityEngine.Random.value < d.chance)
                        Instantiate(d.prefab, transform.position, Quaternion.identity);
                }
            }
            // 播一个 "定格" 视觉——由特效组件自己听 StructureRepairedEvent 处理
            Destroy(gameObject, 0.3f);
        }
    }
}
