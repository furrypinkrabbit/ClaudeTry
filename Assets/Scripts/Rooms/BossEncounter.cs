using UnityEngine;
using GuJian.Core;
using GuJian.Structures;

namespace GuJian.Rooms {
    /// <summary>
    /// Boss 房控制器。Boss = 一帧 "巨型残缺结构"，由多个可修复部位组成。
    /// 每修复一个部位，Boss 进入下一阶段——触发电角冲击/斗拱镇压等技能。
    /// </summary>
    public class BossEncounter : MonoBehaviour {
        [SerializeField] StructureEnemyPawn[] parts;
        [SerializeField] BossPhase[] phases;
        int _phase;
        int _repairedParts;

        [System.Serializable]
        public struct BossPhase {
            public float moveSpeedMul;
            public float attackIntervalMul;
            public string skillId; // "CheckImpact", "DougongSuppress"
        }

        void OnEnable()  { EventBus.Subscribe<StructureRepairedEvent>(OnPartRepaired); }
        void OnDisable() { EventBus.Unsubscribe<StructureRepairedEvent>(OnPartRepaired); }

        void OnPartRepaired(StructureRepairedEvent e) {
            foreach (var p in parts) {
                if (p != null && p.gameObject == e.structure) {
                    _repairedParts++;
                    TryAdvancePhase();
                    if (_repairedParts >= parts.Length) {
                        EventBus.Publish(new RunFinishedEvent(true, _repairedParts, _repairedParts * 50));
                    }
                    break;
                }
            }
        }

        void TryAdvancePhase() {
            float ratio = (float)_repairedParts / parts.Length;
            int want = Mathf.Clamp(Mathf.FloorToInt(ratio * phases.Length), 0, phases.Length - 1);
            if (want != _phase) { _phase = want; TriggerSkill(phases[_phase].skillId); }
        }

        void TriggerSkill(string id) {
            // 预留：在实现里发射特效 + 出击区隔
            Debug.Log($"[Boss] 触发技能: {id}");
        }
    }
}
