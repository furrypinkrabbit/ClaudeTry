using UnityEngine;

namespace GuJian.Core {
    // ==== 战斗类 ====
    public readonly struct StructureHitEvent {
        public readonly GameObject structure;
        public readonly GameObject attacker;
        public readonly int repairAmount;
        public readonly bool isCritical;
        public StructureHitEvent(GameObject s, GameObject a, int r, bool c) {
            structure = s; attacker = a; repairAmount = r; isCritical = c;
        }
    }

    public readonly struct StructureRepairedEvent {
        public readonly GameObject structure;
        public readonly int craftsmanshipReward;
        public StructureRepairedEvent(GameObject s, int r) { structure = s; craftsmanshipReward = r; }
    }

    public readonly struct PawnDamagedEvent {
        public readonly GameObject pawn;
        public readonly float amount;
        public PawnDamagedEvent(GameObject p, float a) { pawn = p; amount = a; }
    }

    // ==== 进度类 ====
    public readonly struct CraftsmanshipGainedEvent {
        public readonly int delta;
        public readonly int total;
        public CraftsmanshipGainedEvent(int d, int t) { delta = d; total = t; }
    }

    public readonly struct LevelUpRequestedEvent {
        public readonly int newLevel;
        public LevelUpRequestedEvent(int l) { newLevel = l; }
    }

    public readonly struct RoomEnteredEvent {
        public readonly string roomId;
        public readonly int index;
        public RoomEnteredEvent(string id, int i) { roomId = id; index = i; }
    }

    public readonly struct RunFinishedEvent {
        public readonly bool victory;
        public readonly int structuresRepaired;
        public readonly int matterSilverEarned;
        public RunFinishedEvent(bool v, int s, int m) { victory = v; structuresRepaired = s; matterSilverEarned = m; }
    }
}
