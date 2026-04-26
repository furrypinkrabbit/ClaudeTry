using System.Collections.Generic;
using UnityEngine;

namespace GuJian.Progression {
    /// <summary>
    /// 升级池：保存所有候选升级，按随机/权重抽取。
    /// </summary>
    [CreateAssetMenu(menuName = "GuJian/Progression/UpgradePool", fileName = "UpgradePool")]
    public class UpgradePool : ScriptableObject {
        public List<UpgradeOption> options = new();

        public List<UpgradeOption> Draw(int count, System.Random rng) {
            var pool = new List<UpgradeOption>(options);
            var result = new List<UpgradeOption>(count);
            for (int i = 0; i < count && pool.Count > 0; i++) {
                int idx = rng.Next(pool.Count);
                result.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
            return result;
        }
    }
}
