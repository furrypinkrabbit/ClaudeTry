using System;
using GuJian.Structures;

namespace GuJian.Tools {
    /// <summary>
    /// 工具 vs 结构材质的产値修改。例：铜锤打珉瓪产伤 0.6x，石锤打本产伤 1.2x。
    /// </summary>
    [Serializable]
    public struct MaterialAffinity {
        public StructureMaterial material;
        public float damageMul; // 1 = 中性
    }
}
