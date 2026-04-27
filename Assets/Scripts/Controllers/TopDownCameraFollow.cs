namespace GuJian.Controllers
{
    using UnityEngine;
    using Cinemachine;

    namespace GuJian.Rendering {
        /// <summary>
        /// 挂在 InGame 场景的 CinemachineVirtualCamera 所在 GameObject 上。
        /// 自动找到玩家并设为 Follow/LookAt 目标。
        /// </summary>
        [RequireComponent(typeof(CinemachineVirtualCamera))]
        public class TopDownCameraFollow : MonoBehaviour {
            [Header("俯视角设置")]
            [SerializeField] float height = 15f;       // 摄像机高度
            [SerializeField] float pitch  = 45f;       // 俯仰角（度），90=正顶视
            [SerializeField] float yaw = -5f;
            [Tooltip("跟随目标 Tag，场景里 Player GameObject 的 Tag")]
            [SerializeField] string playerTag = "Player";

            CinemachineVirtualCamera _vcam;

            void Awake() {
                _vcam = GetComponent<CinemachineVirtualCamera>();
                TryBindPlayer();
            }

            void Start() {
                // Awake 时玩家可能还未生成，Start 再试一次
                if (_vcam.Follow == null) TryBindPlayer();
                ApplyTopDownBody();
            }

            void TryBindPlayer() {
                var player = GameObject.FindGameObjectWithTag(playerTag);
                if (player == null) return;
                _vcam.Follow  = player.transform;
                _vcam.LookAt  = player.transform;
            }

            void ApplyTopDownBody() {
                // 使用 Transposer 偏移保持固定高度俯视
                var body = _vcam.GetCinemachineComponent<CinemachineTransposer>();
                if (body != null) {
                    body.m_FollowOffset = new Vector3(0, height, yaw);
                    body.m_BindingMode  = CinemachineTransposer.BindingMode.WorldSpace;
                }
                // 调整旋转让镜头朝下
                transform.rotation = Quaternion.Euler(pitch, 0, 0);
            }
        }
    }

}