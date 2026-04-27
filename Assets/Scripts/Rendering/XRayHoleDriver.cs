using UnityEngine;

namespace GuJian.Rendering {
    /// <summary>
    /// 挂在 Player 上。每帧把玩家世界坐标写入全局 Shader 变量，
    /// 所有使用 GuJian/XRayHole 的材质自动生效，无需逐材质引用。
    /// </summary>
    public class XRayHoleDriver : MonoBehaviour {
        [SerializeField] float holeRadius  = 3.5f;
        [SerializeField] float holeSoft    = 0.8f;

        static readonly int PropPos    = Shader.PropertyToID("_PlayerPos");
        static readonly int PropRadius = Shader.PropertyToID("_HoleRadius");
        static readonly int PropSoft   = Shader.PropertyToID("_HoleSoft");

        void Start() {
            // 写一次静态参数
            Shader.SetGlobalFloat(PropRadius, holeRadius);
            Shader.SetGlobalFloat(PropSoft,   holeSoft);
        }

        void Update() {
            Shader.SetGlobalVector(PropPos, transform.position);
        }

        // 方便运行时调整半径（如升级/技能）
        public void SetRadius(float r) {
            holeRadius = r;
            Shader.SetGlobalFloat(PropRadius, r);
        }
    }
}