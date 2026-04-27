using UnityEngine;
using GuJian.Pooling;
using GuJian.Pawns.Parts;

namespace GuJian.Enemies {
    /// <summary>
    /// BOSS 下落的小瓦件。纯视觉/物理小将,不是 Pawn。
    /// 落到 y<=groundY 时对 playerTag 目标做一次范围伤害、然后自动返回池子。
    /// </summary>
    public class FallingTile : MonoBehaviour, IPoolable {
        [SerializeField] float fallSpeed = 9f;
        [SerializeField] float damage    = 18f;
        [SerializeField] float lifeTime  = 4f;
        [SerializeField] float groundY   = 0.05f;
        [SerializeField] float hitRadius = 1.1f;
        [SerializeField] string playerTag = "Player";

        float _age;
        static GameObject _playerCache;

        public void OnSpawn()   { _age = 0f; }
        public void OnDespawn() { }

        void Update() {
            float dt = Time.deltaTime;
            _age += dt;
            transform.position += Vector3.down * fallSpeed * dt;

            if (transform.position.y <= groundY) { Impact(); return; }
            if (_age >= lifeTime) GameObjectPool.DespawnAuto(gameObject);
        }

        void Impact() {
            if (_playerCache == null) _playerCache = GameObject.FindWithTag(playerTag);
            var p = _playerCache;
            if (p != null) {
                var dd = Vector3.Distance(p.transform.position, transform.position);
                if (dd <= hitRadius) {
                    var h = p.GetComponentInChildren<PawnHealth>();
                    h?.TakeDamage(damage);
                }
            }
            GameObjectPool.DespawnAuto(gameObject);
        }
    }
}
