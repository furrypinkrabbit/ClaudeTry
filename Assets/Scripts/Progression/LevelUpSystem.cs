using System;
using UnityEngine;
using GuJian.Core;
using GuJian.Pawns;

namespace GuJian.Progression {
    /// <summary>
    /// 升级调度器。监听 LevelUpRequestedEvent，弹出三选一，玩家选择后 Apply。
    /// 本类只负责"何时抽"与"抽给谁"；UI 由 UpgradePanel 负责。
    /// </summary>
    public class LevelUpSystem : MonoBehaviour {
        [SerializeField] UpgradePool pool;
        [SerializeField] int choicesPerLevel = 3;
        [SerializeField] PawnBase targetPawn; // 运行时挂接玩家
        System.Random _rng = new();

        public event Action<System.Collections.Generic.List<UpgradeOption>> OnChoicesPresented;

        void OnEnable()  { EventBus.Subscribe<LevelUpRequestedEvent>(OnLevelUp); }
        void OnDisable() { EventBus.Unsubscribe<LevelUpRequestedEvent>(OnLevelUp); }

        public void SetPawn(PawnBase p) => targetPawn = p;

        void OnLevelUp(LevelUpRequestedEvent _) {
            if (pool == null) { Debug.LogWarning("UpgradePool 未设置"); return; }
            var choices = pool.Draw(choicesPerLevel, _rng);
            OnChoicesPresented?.Invoke(choices);
        }

        /// <summary>UI 层调用，应用玩家选择。</summary>
        public void ApplyChoice(UpgradeOption option) {
            if (option == null || targetPawn == null) return;
            option.Apply(targetPawn);
        }
    }
}
