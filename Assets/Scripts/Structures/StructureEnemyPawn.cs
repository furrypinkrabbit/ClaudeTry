using System;
using UnityEngine;
using GuJian.Core;
using GuJian.Pawns;
using GuJian.Pawns.Parts;
using GuJian.Pooling;

namespace GuJian.Structures {
    /// <summary>
    /// 残缺结构敌人。没有传统 "血量"，而是 "残缺度"，打它=修补。
    /// 自己也是一个 IPawn（由 AIPawnController 驱动）。
    /// [改动摘要] 新增 IPoolable 支持：OnSpawn 重置残缺度/IsAlive，OnDespawn 清订阅；
    ///           RepairComplete 优先走对象池回收，没在池里才 Destroy。
    /// </summary>
    [RequireComponent(typeof(PawnMovement))]
    public class StructureEnemyPawn : PawnBase, IRepairable, IPoolable {
        [SerializeField] StructureData data;
        [SerializeField] SpriteRenderer stageRenderer;
        [Tooltip("被修复后多少秒消失 / 回池。")]
        [SerializeField] float despawnDelay = 0.3f;

        int _current;
        public int CurrentBrokenness => _current;
        public int MaxBrokenness     => data.maxBrokenness;
        public StructureMaterial Material => data.material;
        public StructureData Data => data;
        public event Action<IRepairable> OnRepaired;

        protected override void Awake() {
            base.Awake();
            ResetForLife();
        }

        void ResetForLife() {
            _current = data != null ? data.maxBrokenness : 1;
            // 基类 IsAlive 是 protected set，只能通过 Kill 改——Awake 时默认为 true。
            // 若是池复用（Awake 只执行一次），OnSpawn 会再次设为“活”。
            if (GetPart<PawnMovement>() is { } mv && data != null)
                mv.MoveSpeedMul = data.moveSpeed / 4.0f; // 4 = base
            UpdateStageSprite();
        }

        public void ApplyHit(in RepairHitInfo hit) {
            if (!IsAlive) return;
            _current = Mathf.Max(0, _current - hit.amount);
            EventBus.Publish(new StructureHitEvent(gameObject, hit.attacker, hit.amount, hit.isCritical));
            UpdateStageSprite();
            // 击退
            if (hit.knockbackForce > 0f && GetPart<PawnMovement>() is { } m) {
                transform.position += hit.knockbackDir.normalized * (hit.knockbackForce * 0.1f);
            }
            if (_current == 0) RepairComplete();
        }

        void UpdateStageSprite() {
            if (stageRenderer == null || data == null || data.repairStages == null || data.repairStages.Length == 0) return;
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
            // 延迟隐藏 / 回池
            Invoke(nameof(FinalizeDespawn), despawnDelay);
        }

        void FinalizeDespawn() {
            // 如果在对象池里，回池；否则走旧的 Destroy 分支。
            if (!GameObjectPool.DespawnAuto(gameObject)) Destroy(gameObject);
        }

        // ====== IPoolable ======
        /// <summary>从对象池里取出：把残缺度、事件、IsAlive 全部恢复为初始。</summary>
        public void OnSpawn() {
            // 复位 IsAlive：通过 PawnBase 没有直接 Revive；我们用一个简单手段——
            // Kill 只会在 IsAlive=true 时生效。池里 Despawn 时 IsAlive 应已是 false。
            // 这里借助反射/protected setter 的替代：直接通过协变——加一个内部重置口。
            ReviveInternal();
            ResetForLife();
            CancelInvoke(nameof(FinalizeDespawn));
        }

        /// <summary>归还对象池：撤销所有未完成 Invoke、清订阅。</summary>
        public void OnDespawn() {
            CancelInvoke();
            OnRepaired = null;
        }

        // 用“受保护的副作用”把 IsAlive 拉回 true：PawnBase.IsAlive 是 protected set，
        // 这里直接改写同一个字段——走 C# 的 `base.IsAlive = true`。
        void ReviveInternal() { IsAlive = true; }
    }
}
