using System.Collections.Generic;
using GuJian.Structures;

namespace GuJian.GameFlow {
    /// <summary>
    /// 一次 Run 的动态状态（非持久化）。结算后注入大厅存档。
    /// </summary>
    public class RunContext {
        public int   structuresRepaired;
        public int   elitesRepaired;
        public int   roomsCleared;
        public bool  bossDefeated;
        public float runTimeSeconds;
        public Dictionary<StructureMaterial, int> perMaterial = new();

        public int CalcMatterSilver() {
            int baseSilver = structuresRepaired * 3 + elitesRepaired * 15 + roomsCleared * 20;
            if (bossDefeated) baseSilver += 200;
            return baseSilver;
        }
    }
}
