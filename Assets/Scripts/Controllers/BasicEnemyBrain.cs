using System;
using UnityEngine;
using GuJian.Pawns;

namespace GuJian.Controllers {
    /// <summary>
    /// 简单敌人 AI：寻找标记为 "Player" 的目标 -> 接近 -> 范围内攻击。
    /// 想换 AI 只需换这个 MonoBehaviour，不撨 Pawn。
    /// </summary>
    public class BasicEnemyBrain : MonoBehaviour, AIPawnController.IAIBrain {
        [SerializeField] float aggroRange  = 12f;
        [SerializeField] float attackRange = 1.8f;
        [SerializeField] float attackInterval = 1.4f;
        float _cd;

        public void Think(IPawn self, float dt, Action<PawnIntent> emit) {
            _cd -= dt;
            var player = GameObject.FindWithTag("Player");
            if (player == null) { emit(PawnIntent.Move(Vector2.zero)); return; }
            Vector3 to = player.transform.position - self.Transform.position;
            float dist = to.magnitude;
            if (dist > aggroRange) { emit(PawnIntent.Move(Vector2.zero)); return; }

            var dir = new Vector2(to.x, to.z).normalized;
            emit(PawnIntent.Look(dir));
            if (dist > attackRange) {
                emit(PawnIntent.Move(dir));
            } else {
                emit(PawnIntent.Move(Vector2.zero));
                if (_cd <= 0f) { emit(PawnIntent.Primary()); _cd = attackInterval; }
            }
        }
    }
}
