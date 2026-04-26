using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GuJian.Controllers;
using GuJian.Core;
using GuJian.Enemies;
using GuJian.Pawns;
using GuJian.Pooling;
using GuJian.Structures;

namespace GuJian.LevelFlow {
    /// <summary>
    /// 关卡流程的“大脑”。按 <see cref="CampaignDef"/> 顺序执行关卡:
    ///   - Wave 关 → 循环 bursts,待当前波全部修复后推下一波;全波完 → 进入下一关。
    ///   - Boss 关 → 生成 BOSS,监听 <see cref="DougongBossBrain.OnBossDefeated"/>。
    /// 生成都走 <see cref="PoolRegistry"/>;间隔扣完或 BOSS 死亡时发 <see cref="LevelStageChangedEvent"/>。
    /// </summary>
    public class LevelFlowController : MonoBehaviour {
        [SerializeField] CampaignDef campaign;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] Transform   bossAnchor;
        [SerializeField] bool        autoStart = true;
        [Tooltip("Boss 掉落瓦件 的父节点(跟 Boss 预置体位置关联)。")]
        [SerializeField] Transform   bossTileParent;

        public CampaignDef Campaign => campaign;
        public int StageIndex { get; private set; } = -1;
        public LevelStageDef CurrentStage { get; private set; }

        readonly List<GameObject> _alive = new();
        DougongBossBrain _activeBoss;
        Coroutine _routine;

        void Start() {
            if (autoStart && campaign != null) Begin();
        }

        public void Begin() {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Run());
        }

        IEnumerator Run() {
            for (int i = 0; i < campaign.stages.Length; i++) {
                StageIndex = i;
                CurrentStage = campaign.stages[i];
                if (CurrentStage == null) continue;
                EventBus.Publish(new LevelStageChangedEvent(CurrentStage.stageId ?? "", i, (int)CurrentStage.kind));
                if (CurrentStage.kind == LevelStageDef.Kind.Wave) yield return RunWaveStage(CurrentStage);
                else                                               yield return RunBossStage(CurrentStage);
            }
            EventBus.Publish(new CampaignCompletedEvent(campaign != null ? campaign.campaignId : ""));
        }

        // ======================== Wave 关 ========================
        IEnumerator RunWaveStage(LevelStageDef stage) {
            if (stage.enemyPrefab == null || stage.enemyData == null) {
                Debug.LogWarning($"[LevelFlow] 关卡 {stage.stageId} 没有配 prefab/data");
                yield break;
            }
            var pool = PoolRegistry.Instance?.Register(stage.enemyPrefab, stage.poolPrewarm);
            if (pool == null) {
                Debug.LogError("[LevelFlow] PoolRegistry.Instance 为空,请先在场景里放一个 PoolRegistry");
                yield break;
            }

            foreach (var b in stage.bursts) {
                yield return new WaitForSeconds(b.startDelay);
                for (int i = 0; i < b.count; i++) {
                    SpawnPooledEnemy(stage, pool, b.spreadRadius);
                    yield return new WaitForSeconds(0.08f);
                }
                // 等当前波全死/修复
                while (AliveCount() > 0) yield return null;
            }
        }

        void SpawnPooledEnemy(LevelStageDef stage, GameObjectPool pool, float spread) {
            Vector3 basePos = PickSpawnPoint();
            if (spread > 0.01f) {
                var off = UnityEngine.Random.insideUnitCircle * spread;
                basePos += new Vector3(off.x, 0, off.y);
            }
            var go = pool.Spawn(basePos, Quaternion.identity);
            var pawn = go.GetComponentInChildren<StructureEnemyPawn>();
            if (pawn == null) { Debug.LogError($"{stage.enemyPrefab.name} 缺少 StructureEnemyPawn"); return; }
            // 跟踪死亡 → 回池
            var tracker = go.GetComponent<PooledEnemyTracker>();
            if (tracker == null) tracker = go.AddComponent<PooledEnemyTracker>();
            tracker.Bind(pawn, () => {
                _alive.Remove(go);
                GameObjectPool.DespawnAuto(go);
            });
            _alive.Add(go);
        }

        Vector3 PickSpawnPoint() {
            if (spawnPoints == null || spawnPoints.Length == 0) return transform.position;
            var sp = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            return sp != null ? sp.position : transform.position;
        }

        int AliveCount() {
            for (int i = _alive.Count - 1; i >= 0; i--)
                if (_alive[i] == null || !_alive[i].activeInHierarchy) _alive.RemoveAt(i);
            return _alive.Count;
        }

        // ======================== BOSS 关 ========================
        IEnumerator RunBossStage(LevelStageDef stage) {
            if (stage.bossPrefab == null) {
                Debug.LogWarning("[LevelFlow] BOSS 关卡没对预置体"); yield break;
            }
            var pos = bossAnchor != null ? bossAnchor.position : transform.position;
            var rot = bossAnchor != null ? bossAnchor.rotation : Quaternion.identity;
            var pool = PoolRegistry.Instance.Register(stage.bossPrefab, 1);
            var bossGo = pool.Spawn(pos, rot);

            _activeBoss = bossGo.GetComponentInChildren<DougongBossBrain>();
            if (_activeBoss == null) { Debug.LogError("BOSS 预置体上缺 DougongBossBrain"); yield break; }

            bool defeated = false;
            Action onDefeated = () => defeated = true;
            _activeBoss.OnBossDefeated += onDefeated;

            while (!defeated) yield return null;

            _activeBoss.OnBossDefeated -= onDefeated;
            GameObjectPool.DespawnAuto(bossGo);
            _activeBoss = null;
        }
    }

    // 关卡事件
    public readonly struct LevelStageChangedEvent {
        public readonly string stageId;
        public readonly int    index;
        public readonly int    kind; // 0=wave, 1=boss
        public LevelStageChangedEvent(string id, int i, int k) { stageId = id; index = i; kind = k; }
    }

    public readonly struct CampaignCompletedEvent {
        public readonly string campaignId;
        public CampaignCompletedEvent(string id) { campaignId = id; }
    }
}
