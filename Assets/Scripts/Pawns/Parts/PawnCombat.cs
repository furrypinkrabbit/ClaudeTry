using UnityEngine;

namespace GuJian.Pawns.Parts {
    /// <summary>
    /// 响应攻击类意图，转发给 PawnToolSlot.Current。
    /// 负责蕀力记账、攻击力加成等系数传递。
    /// </summary>
    [RequireComponent(typeof(PawnToolSlot))]
    public class PawnCombat : PawnPartBase {
        public Animator playerAnimator;
        public int AttackCombo = 0;
        private bool _isAttacking;
        private int _isAttackParamId;
        
        [Header("Bonuses")]
        public float DamageMul = 1f;
        public float CritChanceAdd = 0f;
        public float CritDamageAdd = 0f;

        [Header("Stamina")]
        [SerializeField] float staminaMax = 100f;
        [SerializeField] float staminaRegen = 18f;
        public float StaminaMulBonus { get; set; } = 0f; // 1 = +100%

        public float Stamina { get; private set; }
        private PawnToolSlot _tool;
        private float _chargeStartTime = -1f;
        private bool _isCharging = false;

        protected override void OnBind() {
            _tool = GetComponent<PawnToolSlot>();
            Stamina = staminaMax;
            _isAttackParamId = Animator.StringToHash("isAttack");
        }

        public override bool HandlesIntent(PawnIntentKind k) =>
            k == PawnIntentKind.AttackPrimary || k == PawnIntentKind.AttackHeavy
            || k == PawnIntentKind.ChargeStart  || k == PawnIntentKind.ChargeRelease
            || k == PawnIntentKind.Dodge  || k==PawnIntentKind.MoveAxis;

        public override void HandleIntent(in PawnIntent intent) {
            if (_tool?.Current == null) return;
            
            if (intent.Kind is PawnIntentKind.MoveAxis or PawnIntentKind.Dodge)
            {
                ClearCombo();
                return;
            }
            
            var ctx = new Tools.ToolContext {
                DamageMul = DamageMul,
                CritChanceAdd = CritChanceAdd,
                CritDamageAdd = CritDamageAdd,
                Facing = transform.forward,
                Origin = transform.position,
            };
            switch (intent.Kind) {
                case PawnIntentKind.AttackPrimary:
                    if (TrySpend(_tool.Current.Data.staminaCost))
                    {            
                        _tool.Current.Trigger(Tools.ToolActionType.Primary, ctx);
                        playerAnimator.SetTrigger("Attack");
                        AttackCombo++;
                    }

                    break;
                case PawnIntentKind.AttackHeavy:
                    if (TrySpend(_tool.Current.Data.staminaCost * 1.6f))
                    {      
                        _tool.Current.Trigger(Tools.ToolActionType.Heavy, ctx);
                        playerAnimator.SetTrigger("Attack");
                        AttackCombo++;
                    }
                    
                    break;
                case PawnIntentKind.ChargeStart:
                    _chargeStartTime = Time.time;
                    _isCharging = true;
                    _tool.Current.Trigger(Tools.ToolActionType.ChargeStart, ctx);
                    playerAnimator.SetBool("ChargeHeld",true);
                    break;
                case PawnIntentKind.ChargeRelease:
                    _isCharging = false;
                    float held = _chargeStartTime > 0 ? Time.time - _chargeStartTime : intent.Scalar;
                    _chargeStartTime = -1f;
                    ctx.ChargeSeconds = held;
                    playerAnimator.SetBool("ChargingHeld", false);
                    if (TrySpend(_tool.Current.Data.staminaCost * 1.2f))
                        _tool.Current.Trigger(Tools.ToolActionType.ChargeRelease, ctx);
                    break;
            }

            AttackCombo %= 2;
            playerAnimator.SetInteger("AttackNum",AttackCombo);
        }

        bool TrySpend(float cost) {
            cost *= 1f / (1f + StaminaMulBonus);
            if (Stamina < cost) return false;
            Stamina -= cost;
            return true;
        }
        
        private void ClearCombo()
        {
            AttackCombo = 0;
            playerAnimator.SetInteger("AttackNum", 0);
        }

        void Update() {
            if (!_isCharging) {
                Stamina = Mathf.Min(staminaMax, Stamina + staminaRegen * Time.deltaTime);
            }
            
            bool isAttacking = playerAnimator.GetBool(_isAttackParamId);
            if (!isAttacking && AttackCombo != 0)
            {
                ClearCombo();
            }
        }
    }
}
