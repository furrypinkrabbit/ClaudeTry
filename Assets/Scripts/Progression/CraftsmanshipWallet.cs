using System;
using UnityEngine;
using GuJian.Core;

namespace GuJian.Progression {
    /// <summary>
    /// 本次 Run 的手艺（经验）钱包。可订阅升级阈值事件。
    /// </summary>
    public class CraftsmanshipWallet : MonoBehaviour {
        [SerializeField] AnimationCurve levelCurve = AnimationCurve.Linear(0, 20, 10, 200);
        public int Level { get; private set; } = 1;
        public int Current { get; private set; }
        public int RequiredForNext => Mathf.RoundToInt(levelCurve.Evaluate(Level));

        public event Action<int,int> OnChanged; // (current, required)

        void OnEnable() {
            EventBus.Subscribe<StructureRepairedEvent>(OnRepaired);
        }
        void OnDisable() {
            EventBus.Unsubscribe<StructureRepairedEvent>(OnRepaired);
        }

        void OnRepaired(StructureRepairedEvent e) => Gain(e.craftsmanshipReward);

        public void Gain(int amount) {
            Current += amount;
            EventBus.Publish(new CraftsmanshipGainedEvent(amount, Current));
            while (Current >= RequiredForNext) {
                Current -= RequiredForNext;
                Level++;
                EventBus.Publish(new LevelUpRequestedEvent(Level));
            }
            OnChanged?.Invoke(Current, RequiredForNext);
        }
    }
}
