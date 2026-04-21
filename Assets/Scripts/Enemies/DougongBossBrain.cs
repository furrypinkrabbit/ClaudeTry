using System;
using System.Collections.Generic;
using UnityEngine;
using GuJian.Core;
using GuJian.Pawns;
using GuJian.Pooling;

namespace GuJian.Enemies {
    /// <summary>
    /// BOSS 斗拱的大脑。三阶段:
    ///  Phase1 (100%..60% 弱点未修): 1 弱点激活,慢轮换,仅 Sweep
    ///  Phase2 (60%..30%):             2 弱点同活,加 TileDrop
    ///  Phase3 (<30%):                 2 弱点同活,轮换加速,Sweep + 高频 TileDrop
    /// Sweep 由同挂 <see cref="BossSweepAttack"/> 的 Part 处理;TileDrop 由本 Brain 直接生产(池化)。
    /// </summary>
    public class DougongBossBrain : EnemyBrainBase {
        [Header("弱点(按顺序排列)")]
        [SerializeField] BossWeakpoint[] weakpoints;

        [Header("横扫")]
        [SerializeField] float sweepIntervalP1 = 5.5f;
        [SerializeField] float sweepIntervalP2 = 4.0f;
        [SerializeField] float sweepIntervalP3 = 2.8f;
        [SerializeField] float sweepTriggerRange = 6f;

        [Header("瓦件下落(池化)")]
        [SerializeField] GameObject fallingTilePrefab;
        [SerializeField] Transform  dropParent;
        [SerializeField] int   dropPoolWarm = 8;
        [SerializeField] int   dropsPerBurst = 3;
        [SerializeField] float dropHeight = 12f;
        [SerializeField] float dropSpreadRadius = 5f;
        [SerializeField] float dropIntervalP2 = 3.2f;
        [SerializeField] float dropIntervalP3 = 1.8f;

        [Header("弱点切换")]
        [SerializeField] float rotateIntervalP1 = 4.0f;
        [SerializeField] float rotateIntervalP2 = 3.0f;
        [SerializeField] float rotateIntervalP3 = 2.0f;

        GameObjectPool _tilePool;
        float _sweepTimer;
        float _dropTimer;
        float _rotateTimer;
        int   _phase;           // 0,1,2
        int   _cursor;          // 轮到哪个弱点
        int   _totalRepaired;

        public event Action<int> OnPhaseChanged;    // phase index
        public event Action       OnBossDefeated;

        void Awake() {
            if (fallingTilePrefab != null)
                _tilePool = new GameObjectPool(fallingTilePrefab, dropParent, dropPoolWarm);
            for (int i = 0; i < weakpoints.Length; i++) {
                if (weakpoints[i] == null) continue;
                int capturedIndex = i;
                weakpoints[i].Index = i;
                weakpoints[i].OnRepaired += _ => OnWeakpointRepaired(capturedIndex);
            }
        }

        public override void OnSpawn() {
            base.OnSpawn();
            _phase = 0;
            _cursor = 0;
            _totalRepaired = 0;
            _sweepTimer = _dropTimer = _rotateTimer = 0f;
            ActivateInitial();
        }

        public override void OnDespawn() {
            base.OnDespawn();
            // 召回所有已发射的瓦件
        }

        void ActivateInitial() {
            for (int i = 0; i < weakpoints.Length; i++) weakpoints[i]?.SetActive(false);
            RotateActiveWeakpoints(initial: true);
        }

        protected override void OnThink(IPawn self, Transform player, Vector3 to,
                                        float dist, float dt, Action<PawnIntent> emit) {
            _sweepTimer  += dt;
            _dropTimer   += dt;
            _rotateTimer += dt;

            // 面向玩家但不移动(BOSS 不走动)
            Vector2 dir = new Vector2(to.x, to.z).normalized;
            emit(PawnIntent.Look(dir));
            emit(PawnIntent.Move(Vector2.zero));

            // 1) 横扫
            float sweepEvery = _phase == 0 ? sweepIntervalP1 :
                               _phase == 1 ? sweepIntervalP2 : sweepIntervalP3;
            if (_sweepTimer >= sweepEvery && dist <= sweepTriggerRange) {
                _sweepTimer = 0f;
                emit(PawnIntent.Heavy()); // BossSweepAttack 会接收
            }

            // 2) 瓦件下落(池化) — Phase2+
            if (_phase >= 1 && _tilePool != null) {
                float dropEvery = _phase == 1 ? dropIntervalP2 : dropIntervalP3;
                if (_dropTimer >= dropEvery) {
                    _dropTimer = 0f;
                    SpawnTileBurst(player.position);
                }
            }

            // 3) 轮换弱点
            float rotateEvery = _phase == 0 ? rotateIntervalP1 :
                                _phase == 1 ? rotateIntervalP2 : rotateIntervalP3;
            if (_rotateTimer >= rotateEvery) {
                _rotateTimer = 0f;
                RotateActiveWeakpoints();
            }
        }

        void SpawnTileBurst(Vector3 anchor) {
            for (int i = 0; i < dropsPerBurst; i++) {
                var off = UnityEngine.Random.insideUnitCircle * dropSpreadRadius;
                var pos = new Vector3(anchor.x + off.x, dropHeight, anchor.z + off.y);
                _tilePool.Spawn(pos, Quaternion.identity);
            }
        }

        /// <summary>根据当前阶段需要的激活数量,循环点亮下几个未完成的弱点。</summary>
        void RotateActiveWeakpoints(bool initial = false) {
            // 关掉所有活动的非完成弱点
            for (int i = 0; i < weakpoints.Length; i++) {
                var w = weakpoints[i];
                if (w != null && !w.IsCompleted) w.SetActive(false);
            }
            int want = _phase == 0 ? 1 : 2;
            var candidates = _tempList;
            candidates.Clear();
            for (int i = 0; i < weakpoints.Length; i++) {
                var idx = (_cursor + i) % weakpoints.Length;
                var w = weakpoints[idx];
                if (w != null && !w.IsCompleted) candidates.Add(idx);
            }
            int pick = Mathf.Min(want, candidates.Count);
            for (int i = 0; i < pick; i++) weakpoints[candidates[i]].SetActive(true);
            if (candidates.Count > 0)
                _cursor = (candidates[0] + (initial ? 0 : 1)) % weakpoints.Length;
        }
        static readonly List<int> _tempList = new(8);

        void OnWeakpointRepaired(int index) {
            _totalRepaired++;
            int totalActive = 0;
            for (int i = 0; i < weakpoints.Length; i++)
                if (weakpoints[i] != null && !weakpoints[i].IsCompleted) totalActive++;
            // 阶段切换:完成 ≥ 40% / ≥ 70% 时步进
            float done = 1f - (float)totalActive / Mathf.Max(1, weakpoints.Length);
            int wantPhase = done >= 0.7f ? 2 : done >= 0.4f ? 1 : 0;
            if (wantPhase != _phase) {
                _phase = wantPhase;
                OnPhaseChanged?.Invoke(_phase);
            }
            if (totalActive == 0) {
                OnBossDefeated?.Invoke();
                EventBus.Publish(new RunFinishedEvent(true, _totalRepaired, _totalRepaired * 50));
            } else {
                RotateActiveWeakpoints();
            }
        }
    }
}
