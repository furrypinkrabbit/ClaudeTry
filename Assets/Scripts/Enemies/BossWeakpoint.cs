using System;
using UnityEngine;
using GuJian.Core;
using GuJian.Pooling;
using GuJian.Structures;

namespace GuJian.Enemies {
    /// <summary>
    /// BOSS 的一个“错位构件”。本身实现 IRepairable,可被锤子攻击。
    /// 但只有 IsActive=true 的时候打击才算数;否则发 Deflect 事件、伤害归 0。
    /// 通过 <see cref="SetActive"/> 切换状态——交给 DougongBossBrain 管理。
    /// </summary>
    public class BossWeakpoint : MonoBehaviour, IRepairable, IPoolable {
        [SerializeField] int maxBrokenness = 60;
        [SerializeField] StructureMaterial material = StructureMaterial.Bronze;
        [Header("高亮")]
        [SerializeField] Renderer highlightRenderer;
        [SerializeField] Color activeEmission = new(1f, 0.78f, 0.3f, 1f);
        [SerializeField] Color dormantEmission = new(0.05f, 0.05f, 0.05f, 1f);

        int  _current;
        bool _active;

        public int  CurrentBrokenness  => _current;
        public int  MaxBrokenness      => maxBrokenness;
        public StructureMaterial Material => material;
        public bool IsActive            => _active;
        public bool IsCompleted         => _current <= 0;
        public int  Index               { get; set; }

        public event Action<IRepairable> OnRepaired;
        public event Action<BossWeakpoint, int, bool> OnHitTaken; // (self, amount, wasActive)

        void Awake() { _current = maxBrokenness; ApplyVisual(); }

        public void OnSpawn() {
            _current = maxBrokenness;
            _active = false;
            ApplyVisual();
        }
        public void OnDespawn() { OnRepaired = null; OnHitTaken = null; }

        public void SetActive(bool on) {
            if (_active == on) return;
            _active = on;
            ApplyVisual();
        }

        public void ApplyHit(in RepairHitInfo hit) {
            if (IsCompleted) return;
            if (!_active) {
                // 偏转:并不算伤。发一个伤害=0 的命中事件,UI/特效都能接到
                EventBus.Publish(new StructureHitEvent(gameObject, hit.attacker, 0, false));
                OnHitTaken?.Invoke(this, 0, false);
                return;
            }
            _current = Mathf.Max(0, _current - hit.amount);
            EventBus.Publish(new StructureHitEvent(gameObject, hit.attacker, hit.amount, hit.isCritical));
            OnHitTaken?.Invoke(this, hit.amount, true);
            if (_current == 0) {
                OnRepaired?.Invoke(this);
                SetActive(false);
                EventBus.Publish(new StructureRepairedEvent(gameObject, 12));
            }
        }

        void ApplyVisual() {
            if (highlightRenderer == null) return;
            var mat = highlightRenderer.material;
            var c = _active ? activeEmission : dormantEmission;
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", c);
            if (mat.HasProperty("_BaseColor"))     mat.SetColor("_BaseColor",     Color.Lerp(Color.black, c, 0.7f));
            if (mat.HasProperty("_Color"))         mat.SetColor("_Color",         Color.Lerp(Color.black, c, 0.7f));
        }
    }
}
