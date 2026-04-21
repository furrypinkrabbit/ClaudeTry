using UnityEngine;
using GuJian.Core;

namespace GuJian.Audio {
    /// <summary>
    /// 把 EventBus 的战斗/进度事件 桥接到 AudioManager,免得战斗代码直接引用 Audio。
    /// 放一份到场景里就能听到声音。
    /// </summary>
    public class AudioEventBridge : MonoBehaviour {
        [Header("打击/修复")]
        [SerializeField] string onHitId        = "sfx_hammer_hit";
        [SerializeField] string onCritId       = "sfx_hammer_crit";
        [SerializeField] string onRepairId     = "sfx_repair_complete";
        [SerializeField] string onPawnHurtId   = "sfx_player_hurt";
        [Header("进度")]
        [SerializeField] string onLevelUpId    = "sfx_levelup";
        [SerializeField] string onRoomEnterId  = "sfx_room_enter";
        [SerializeField] string onRunFinishId  = "sfx_run_finish";

        void OnEnable() {
            EventBus.Subscribe<StructureHitEvent>(OnHit);
            EventBus.Subscribe<StructureRepairedEvent>(OnRepair);
            EventBus.Subscribe<PawnDamagedEvent>(OnPawnHurt);
            EventBus.Subscribe<LevelUpRequestedEvent>(OnLevelUp);
            EventBus.Subscribe<RoomEnteredEvent>(OnRoomEnter);
            EventBus.Subscribe<RunFinishedEvent>(OnRunFinish);
        }
        void OnDisable() {
            EventBus.Unsubscribe<StructureHitEvent>(OnHit);
            EventBus.Unsubscribe<StructureRepairedEvent>(OnRepair);
            EventBus.Unsubscribe<PawnDamagedEvent>(OnPawnHurt);
            EventBus.Unsubscribe<LevelUpRequestedEvent>(OnLevelUp);
            EventBus.Unsubscribe<RoomEnteredEvent>(OnRoomEnter);
            EventBus.Unsubscribe<RunFinishedEvent>(OnRunFinish);
        }

        static AudioManager Am => AudioManager.Instance;
        void OnHit(StructureHitEvent e)        { Am?.Play(e.isCritical ? onCritId : onHitId); }
        void OnRepair(StructureRepairedEvent e){ Am?.Play(onRepairId); }
        void OnPawnHurt(PawnDamagedEvent e)    { Am?.Play(onPawnHurtId); }
        void OnLevelUp(LevelUpRequestedEvent e){ Am?.Play(onLevelUpId); }
        void OnRoomEnter(RoomEnteredEvent e)   { Am?.Play(onRoomEnterId); }
        void OnRunFinish(RunFinishedEvent e)   { Am?.Play(onRunFinishId); }
    }
}
