using System.Collections.Generic;
using UnityEngine;
using GuJian.Core;
using GuJian.Pawns;
using GuJian.Pawns.Parts;
using GuJian.Progression;
using GuJian.Structures;

namespace GuJian.DevTools {
    /// <summary>
    /// 覆盖层调试 HUD。只在 DevTools 场景使用。
    /// 显示:HP / 体力 / 手艺等级与进度 / 最近敌人残缺度 / 最后 N 条事件日志。
    /// </summary>
    public class DebugHUD : MonoBehaviour {
        PawnHealth          _health;
        PawnCombat          _combat;
        PlayerPawn          _player;
        CraftsmanshipWallet _wallet;

        readonly Queue<string> _log = new();
        const int MaxLogLines = 12;

        GUIStyle _label, _title, _logStyle;

        public void Bind(PlayerPawn player, CraftsmanshipWallet wallet) {
            _player = player;
            _wallet = wallet;
            if (_player != null) {
                _health = _player.GetPart<PawnHealth>();
                _combat = _player.GetPart<PawnCombat>();
            }
        }

        void OnEnable() {
            EventBus.Subscribe<StructureHitEvent>(OnHit);
            EventBus.Subscribe<StructureRepairedEvent>(OnRepaired);
            EventBus.Subscribe<CraftsmanshipGainedEvent>(OnCraft);
            EventBus.Subscribe<LevelUpRequestedEvent>(OnLevelUp);
            EventBus.Subscribe<PawnDamagedEvent>(OnDamaged);
            EventBus.Subscribe<RunFinishedEvent>(OnFinished);
        }
        void OnDisable() {
            EventBus.Unsubscribe<StructureHitEvent>(OnHit);
            EventBus.Unsubscribe<StructureRepairedEvent>(OnRepaired);
            EventBus.Unsubscribe<CraftsmanshipGainedEvent>(OnCraft);
            EventBus.Unsubscribe<LevelUpRequestedEvent>(OnLevelUp);
            EventBus.Unsubscribe<PawnDamagedEvent>(OnDamaged);
            EventBus.Unsubscribe<RunFinishedEvent>(OnFinished);
        }

        void OnHit(StructureHitEvent e)
            => Push($"<color=#C9A24C>HIT</color>  {(e.isCritical ? "CRIT " : "")}{e.repairAmount} → {Name(e.structure)}");
        void OnRepaired(StructureRepairedEvent e)
            => Push($"<color=#7A1E12>REPAIRED</color>  {Name(e.structure)}  +{e.craftsmanshipReward}手艺");
        void OnCraft(CraftsmanshipGainedEvent e)
            => Push($"<color=#3E5C76>+手艺</color> {e.delta} (累计 {e.total})");
        void OnLevelUp(LevelUpRequestedEvent e)
            => Push($"<color=#C9A24C>LEVEL UP!</color> → {e.newLevel}");
        void OnDamaged(PawnDamagedEvent e)
            => Push($"<color=#B8352F>DMG</color>  {Name(e.pawn)}  -{e.amount:F0}");
        void OnFinished(RunFinishedEvent e)
            => Push($"<b>RUN FINISHED</b>  victory={e.victory}  结构={e.structuresRepaired}  匠银={e.matterSilverEarned}");

        static string Name(GameObject go) => go != null ? go.name : "(null)";

        void Push(string line) {
            _log.Enqueue($"[{Time.time:F1}] {line}");
            while (_log.Count > MaxLogLines) _log.Dequeue();
        }

        void EnsureStyles() {
            if (_label != null) return;
            _label = new GUIStyle(GUI.skin.label) {
                fontSize = 14, richText = true,
                normal = { textColor = new Color(0.95f, 0.90f, 0.78f) },
            };
            _title = new GUIStyle(_label) {
                fontSize = 16, fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.79f, 0.64f, 0.30f) },
            };
            _logStyle = new GUIStyle(_label) {
                fontSize = 12, alignment = TextAnchor.UpperLeft,
                wordWrap = true,
            };
        }

        void OnGUI() {
            EnsureStyles();
            // 左上:角色状态
            var panel = new Rect(12, 12, 340, 170);
            GUI.color = new Color(0, 0, 0, 0.55f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(panel.x + 10, panel.y + 8, panel.width - 20, panel.height - 16));
            GUILayout.Label("<b>— 古建修复者 —  [DEBUG]</b>", _title);
            if (_health != null) GUILayout.Label($"HP       {_health.CurrentHp:F0} / {100f + _health.MaxHpBonus:F0}   盾 {_health.CurrentShield:F0}", _label);
            if (_combat != null) GUILayout.Label($"体力     {_combat.Stamina:F0}    伤害×{_combat.DamageMul:F2}   暴击+{_combat.CritChanceAdd:P0}", _label);
            if (_wallet != null) GUILayout.Label($"手艺     Lv {_wallet.Level}   {_wallet.Current} / {_wallet.RequiredForNext}", _label);

            var nearest = FindNearestEnemy();
            if (nearest.e != null)
                GUILayout.Label($"最近靶    {nearest.e.CurrentBrokenness}/{nearest.e.MaxBrokenness}  {nearest.e.Material}  ({nearest.dist:F1} m)", _label);
            GUILayout.EndArea();

            // 右下:事件日志
            var logRect = new Rect(Screen.width - 440, Screen.height - 260, 428, 248);
            GUI.color = new Color(0, 0, 0, 0.55f);
            GUI.DrawTexture(logRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(logRect.x + 10, logRect.y + 8, logRect.width - 20, logRect.height - 16));
            GUILayout.Label("<b>事件日志</b>", _title);
            foreach (var line in _log) GUILayout.Label(line, _logStyle);
            GUILayout.EndArea();

            // 左下:按键提示
            var tips = new Rect(12, Screen.height - 120, 340, 108);
            GUI.color = new Color(0, 0, 0, 0.45f);
            GUI.DrawTexture(tips, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(tips.x + 10, tips.y + 6, tips.width - 20, tips.height - 12));
            GUILayout.Label("WASD 移动  鼠标 朝向", _logStyle);
            GUILayout.Label("J/左键 普攻   K/右键 重击   L 按住蓄力 松开挥出", _logStyle);
            GUILayout.Label("Space 闪避   E 交互   Q UseTool   [ ] 切工具", _logStyle);
            GUILayout.Label("R 再刷一只敌人", _logStyle);
            GUILayout.EndArea();
        }

        (StructureEnemyPawn e, float dist) FindNearestEnemy() {
            if (_player == null) return (null, 0);
            StructureEnemyPawn best = null; float bd = float.MaxValue;
            // 轻量级:每帧一次 FindObjectsOfType 在 Dev 下可以接受
            foreach (var e in FindObjectsOfType<StructureEnemyPawn>()) {
                if (!e.IsAlive) continue;
                float d = Vector3.Distance(_player.transform.position, e.transform.position);
                if (d < bd) { bd = d; best = e; }
            }
            return (best, bd);
        }
    }
}
