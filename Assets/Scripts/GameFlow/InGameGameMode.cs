using UnityEngine;
using GuJian.Core;
using GuJian.Progression;
using GuJian.Rooms;
using GuJian.Structures;

namespace GuJian.GameFlow {
    public class InGameGameMode : GameModeBase {
        [SerializeField] RoomManager roomManager;
        [SerializeField] CraftsmanshipWallet wallet;
        [SerializeField] LevelUpSystem levelUp;

        public RunContext Run { get; private set; }

        public override void Enter() {
            Run = new RunContext();
            EventBus.Subscribe<StructureRepairedEvent>(OnRepaired);
            EventBus.Subscribe<RoomClearedEvent>(OnRoomCleared);
            EventBus.Subscribe<RunFinishedEvent>(OnRunFinished);
            if (roomManager != null) roomManager.Begin();
        }

        public override void Exit() {
            EventBus.Unsubscribe<StructureRepairedEvent>(OnRepaired);
            EventBus.Unsubscribe<RoomClearedEvent>(OnRoomCleared);
            EventBus.Unsubscribe<RunFinishedEvent>(OnRunFinished);
        }

        void Update() {
            if (Run != null) Run.runTimeSeconds += Time.deltaTime;
        }

        void OnRepaired(StructureRepairedEvent e) {
            Run.structuresRepaired++;
            // 精英/材质统计：读取被修物体的 StructureEnemyPawn
            if (e.structure != null && e.structure.TryGetComponent<StructureEnemyPawn>(out var enemy)) {
                if (!Run.perMaterial.TryAdd(enemy.Material, 1))
                    Run.perMaterial[enemy.Material]++;
                if (enemy.Material == StructureMaterial.Jade || enemy.Material == StructureMaterial.Lacquer)
                    Run.elitesRepaired++;
            }
        }

        void OnRoomCleared(RoomClearedEvent _)
        {
            Run.roomsCleared++;
            if (roomManager == null) return;
            if (roomManager.HasNext)
            {
                // 还有下一关 → 带遮罩切换
                int nextIndex = roomManager.Index + 1;
                string label = $"第 {nextIndex + 1} 关";
                GameBootstrap.Instance?.GoLevelIndex(nextIndex, label);

            }
            else
            {
                // 最后一关打完 → 结算
                EventBus.Publish(new RunFinishedEvent(false, Run.structuresRepaired, Run.CalcMatterSilver()));

            }
        }

        void OnRunFinished(RunFinishedEvent e) {
            Run.bossDefeated = e.victory;
            var save = MetaProgressSave.Load();
            save.matterSilver += Run.CalcMatterSilver();
            save.bestRunStructures = Mathf.Max(save.bestRunStructures, Run.structuresRepaired);
            save.totalRuns++;
            save.Save();
        }
    }
}
