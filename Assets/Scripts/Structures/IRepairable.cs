using System;
using UnityEngine;

namespace GuJian.Structures {
    public struct RepairHitInfo {
        public int   amount;
        public bool  isCritical;
        public GameObject attacker;
        public Vector3 knockbackDir;
        public float   knockbackForce;
    }

    public interface IRepairable {
        /// <summary>当前残缺值（相当于血量）。</summary>
        int CurrentBrokenness { get; }
        int MaxBrokenness { get; }
        StructureMaterial Material { get; }

        /// <summary>应用一次修复打击。</summary>
        void ApplyHit(in RepairHitInfo hit);

        event Action<IRepairable> OnRepaired;
    }
}
