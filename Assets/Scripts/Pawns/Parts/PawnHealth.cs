using System;
using UnityEngine;
using GuJian.Core;

namespace GuJian.Pawns.Parts {
    /// <summary>
    /// 被动部件：不响应意图。用于玩家角色被伤。
    /// 结构敌人的‘残缺度’不在此处，在 RepairableState。
    /// </summary>
    public class PawnHealth : PawnPartBase {
        public Animator playerAnimator;
        [SerializeField] float maxHp = 100f;
        public float deadTime = 1.5f;
        private bool isDead = false;

        private float deadTimer = 0f;
        public float MaxHpBonus { get; set; } = 0f;
        public float ShieldBonus { get; set; } = 0f;
        public float CurrentHp { get; private set; }
        public float CurrentShield { get; private set; }

        public event Action<float,float> OnHpChanged; // (cur,max)
        public event Action OnDied;

        protected override void OnBind() {
            CurrentHp = maxHp;
            CurrentShield = 0f;
        }

        void Update()
        {
            if (isDead)
            {
                deadTimer += Time.deltaTime;
                if (deadTimer >= deadTime)
                {
                    OnDied?.Invoke();
                    Pawn.Kill();
                }
            }
        }

        public void SyncMax() {
            CurrentHp = Mathf.Min(CurrentHp, maxHp + MaxHpBonus);
            CurrentShield = ShieldBonus;
            OnHpChanged?.Invoke(CurrentHp, maxHp + MaxHpBonus);
        }

        public void TakeDamage(float dmg) {
            if (!Pawn.IsAlive || dmg <= 0f) return;
            if (CurrentShield > 0f) {
                float a = Mathf.Min(dmg, CurrentShield);
                CurrentShield -= a; dmg -= a;
            }
            CurrentHp -= dmg;
            EventBus.Publish(new PawnDamagedEvent(Pawn.gameObject, dmg));
            OnHpChanged?.Invoke(CurrentHp, maxHp + MaxHpBonus);
            if (CurrentHp <= 0f) {
                playerAnimator.SetTrigger("Dead");
            }
            playerAnimator.SetTrigger("getHit");
        }

        public void Heal(float amount) {
            if (!Pawn.IsAlive) return;
            CurrentHp = Mathf.Min(CurrentHp + amount, maxHp + MaxHpBonus);
            OnHpChanged?.Invoke(CurrentHp, maxHp + MaxHpBonus);
        }

        public override bool HandlesIntent(PawnIntentKind k) => false;
        public override void HandleIntent(in PawnIntent _) { }
    }
}
