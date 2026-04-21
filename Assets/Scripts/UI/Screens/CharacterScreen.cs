using UnityEngine.UIElements;
using GuJian.Pawns.Parts;

namespace GuJian.UI.Screens {
    /// <summary>
    /// 角色属性:HP / 体力 / 伤害倍率 / 暴击率 等的平铺展示。
    /// 数据通过 <see cref="Bind"/> 从玩家 Pawn 的部件注入。
    /// </summary>
    public class CharacterScreen : UIScreen {
        Label _hp, _stamina, _dmgMul, _critChance, _critDmg, _speedMul;
        Button _close;

        PawnHealth _health;
        PawnCombat _combat;

        public void Bind(PawnHealth h, PawnCombat c) {
            _health = h; _combat = c;
            Rebuild();
        }

        protected override void OnBind(VisualElement root) {
            _hp         = root.Q<Label>("stat-hp");
            _stamina    = root.Q<Label>("stat-stamina");
            _dmgMul     = root.Q<Label>("stat-dmg-mul");
            _critChance = root.Q<Label>("stat-crit-chance");
            _critDmg    = root.Q<Label>("stat-crit-dmg");
            _speedMul   = root.Q<Label>("stat-speed-mul");
            _close      = root.Q<Button>("btn-close");
            if (_close != null) _close.clicked += () => UIRouter.Instance.Pop();
        }

        protected override void OnEnter() { Rebuild(); }

        void Rebuild() {
            if (_health != null) {
                if (_hp      != null) _hp.text      = $"气血: {_health.CurrentHp:0}/{100 + _health.MaxHpBonus:0}";
            }
            if (_combat != null) {
                if (_stamina   != null) _stamina.text    = $"体力: {_combat.Stamina:0}";
                if (_dmgMul    != null) _dmgMul.text     = $"伤害系数: ×{_combat.DamageMul:0.00}";
                if (_critChance!= null) _critChance.text = $"暴击率: +{_combat.CritChanceAdd * 100f:0.0}%";
                if (_critDmg   != null) _critDmg.text    = $"暴击伤害: +{_combat.CritDamageAdd * 100f:0.0}%";
            }
            if (_speedMul != null) _speedMul.text = "移动速度: 100% (基础)";
        }
    }
}
