using UnityEngine;

namespace GuJian.Pawns.Parts {
    /// <summary>
    /// 响应 Move/Look/Dodge 意图。俯视 XZ 平面移动。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PawnMovement : PawnPartBase {
       [SerializeField] Animator playerAnimator;
        
        [Header("Base Stats (受升级影响)")]
        [SerializeField] float baseMoveSpeed = 4.0f;
        [SerializeField] float rotateSpeed   = 720f;
        [SerializeField] float dodgeSpeed    = 12f;
        [SerializeField] float dodgeDuration = 0.22f;
        [SerializeField] float dodgeCooldown = 0.7f;

        private Vector3 localMove;
        public float MoveSpeedMul { get; set; } = 1f;

        private CharacterController _cc;
        private Vector2 _moveAxis;
        private Vector2 _lookAxis;
        private float   _dodgeTimer;
        private float   _cdTimer;
        private Vector3 _dodgeDir;

        protected override void OnBind() {
            _cc = GetComponent<CharacterController>();
            if (playerAnimator == null)
            {
                playerAnimator = GetComponentInChildren<Animator>();
            }
            
        }

        public override bool HandlesIntent(PawnIntentKind k) =>
            k == PawnIntentKind.MoveAxis || k == PawnIntentKind.LookAxis || k == PawnIntentKind.Dodge;

        public override void HandleIntent(in PawnIntent intent) {
            switch (intent.Kind) {
                case PawnIntentKind.MoveAxis: _moveAxis = Vector2.ClampMagnitude(intent.Vector, 1f); break;
                case PawnIntentKind.LookAxis: _lookAxis = intent.Vector; break;
                case PawnIntentKind.Dodge:
                    if (_cdTimer <= 0f) {
                        var dir = intent.Vector.sqrMagnitude > 0.01f ? intent.Vector : _moveAxis;
                        if (dir.sqrMagnitude < 0.01f) dir = new Vector2(transform.forward.x, transform.forward.z);
                        _dodgeDir = new Vector3(dir.x, 0, dir.y).normalized;
                        _dodgeTimer = dodgeDuration;
                        _cdTimer = dodgeCooldown;
                    }
                    break;
            }
        }

        void Update() {
            float dt = Time.deltaTime;
            if (_cdTimer > 0f) _cdTimer -= dt;

            Vector3 velocity;
            if (_dodgeTimer > 0f) {
                _dodgeTimer -= dt;
                velocity = _dodgeDir * dodgeSpeed;
            } else {
                var move = new Vector3(_moveAxis.x, 0, _moveAxis.y);
                velocity = move * (baseMoveSpeed * MoveSpeedMul);
            }
            // 简易重力
            if (!_cc.isGrounded) velocity.y = -9.8f;
            _cc.Move(velocity * dt);

            // 朝向：只跟移动方向，鼠标不影响旋转
             var moveWorld = new Vector3(_moveAxis.x, 0, _moveAxis.y);
             if (moveWorld.sqrMagnitude > 0.01f)
             {
                 var target = Quaternion.LookRotation(moveWorld);
                 transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotateSpeed * dt);
            }
            
            localMove = transform.InverseTransformDirection(velocity);
            localMove.y = 0; // 去掉高度影响
            localMove.Normalize();
            if (playerAnimator is not null)
            {
                playerAnimator.SetFloat("X", localMove.z);
                playerAnimator.SetFloat("Y", localMove.x);
            }
            
        }
    }
}
