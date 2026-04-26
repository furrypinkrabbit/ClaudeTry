using UnityEngine;

namespace GuJian.Rendering
{
    /// <summary>
    /// 根据游戏事件驱动 GuJian/Fullscreen 的 _CombatTension / _HitFlash。
    ///
    /// 新方案:不再依赖 GuJianPostFXCamera / GuJianPostFXFeature。
    /// URP 侧用内置的 "Full Screen Pass Renderer Feature" 吃 <see cref="material"/>,
    /// 这里只负责每帧把紧绷度/闪击值写到同一张材质上。
    ///
    /// 使用:
    ///   1. 项目里创建 Material: Assets/Materials/GuJianPostFX.mat,shader 选 "GuJian/Fullscreen"。
    ///   2. URP Renderer Asset → Add Renderer Feature → Full Screen Pass Renderer Feature
    ///      → Material = GuJianPostFX.mat,Injection Point = After Rendering Post Processing。
    ///   3. Scene 里放一个 [PostFX] GameObject,挂本脚本,把同一张材质拖到 material 字段。
    ///      (放在 Boot 场景并 DontDestroyOnLoad,整局游戏共用。)
    ///
    /// 业务代码通过 <see cref="PushCombat"/>/<see cref="ReportHit"/>/<see cref="ReportDamage"/> 推动。
    /// </summary>
    public class GuJianPostFXDriver : MonoBehaviour
    {
        public static GuJianPostFXDriver Instance { get; private set; }

        [Tooltip("GuJian/Fullscreen 的材质,必须和 Full Screen Pass Renderer Feature 引用的是同一张。")]
        [SerializeField] Material material;

        [Header("紧绷改变")]
        [SerializeField] float tensionRampSpeed = 2.0f;   // 保留字段,后续可能用于加速爬坡
        [SerializeField] float tensionDecay = 0.35f;
        [SerializeField] float hitFlashDecay = 6f;
        [SerializeField] float combatTargetIdle = 0f;
        [SerializeField] float combatTargetFight = 0.7f;

        [Header("跨场景")]
        [SerializeField] bool dontDestroyOnLoad = true;

        float _tension;
        float _hitFlash;
        float _externalCombat;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            if (material == null)
                Debug.LogWarning("[GuJianPostFXDriver] material 未绑定,战斗紧绷/命中红闪不会生效。");
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void PushCombat(bool inCombat)
        {
            _externalCombat = inCombat ? combatTargetFight : combatTargetIdle;
        }

        public void ReportHit() { _hitFlash = Mathf.Max(_hitFlash, 0.55f); _tension = Mathf.Max(_tension, 0.3f); }
        public void ReportDamage() { _hitFlash = Mathf.Max(_hitFlash, 0.85f); _tension = Mathf.Max(_tension, 0.6f); }

        /// <summary>允许外部在场景切换后重新指向材质(一般不需要)。</summary>
        public void SetMaterial(Material m) { material = m; }

        void Update()
        {
            float dt = Time.deltaTime;
            _tension = Mathf.MoveTowards(_tension, _externalCombat, tensionDecay * dt);
            _hitFlash = Mathf.Max(0f, _hitFlash - hitFlashDecay * dt);

            if (material == null) return;
            material.SetFloat("_CombatTension", _tension);
            material.SetFloat("_HitFlash", _hitFlash);
        }
    }
}