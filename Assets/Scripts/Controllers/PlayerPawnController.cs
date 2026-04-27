using UnityEngine;
using UnityEngine.InputSystem;
using GuJian.Input;
using GuJian.Pawns;

namespace GuJian.Controllers {
    /// <summary>
    /// 玩家控制器。
    /// —— 玩家只需在 Inspector 中指定 InputActionAsset 与各 Action 名称，
    ///     本类负责订阅 -> 翻译 -> 下发 PawnIntent 给所附身 Pawn。
    /// 玩家不接触 InputAction.CallbackContext。
    /// </summary>
    public class PlayerPawnController : PawnControllerBase {
        [Header("输入配置（玩家自定义）")]
        [SerializeField] Camera gameCamera;
        [SerializeField] private InputActionAsset inputAsset;
        [SerializeField] private PlayerInputSource.Bindings bindings = new() {
            ActionMap       = "Gameplay",
            Move            = "Move",
            Look            = "Look",
            AttackPrimary   = "Attack",
            AttackHeavy     = "Heavy",
            Dodge           = "Dodge",
            Interact        = "Interact",
            UseTool         = "UseTool",
            SwitchToolNext  = "ToolNext",
            SwitchToolPrev  = "ToolPrev",
            SlotSelect      = "SlotSelect",
            Charge          = "Charge",
        };

        [Header("初始附身的 Pawn（可选：场景直接拖入）")]
        [SerializeField] PlayerPawn autoPossess;

        private PlayerInputSource _src;

        void Awake() {
            if (inputAsset != null) {
                _src = new PlayerInputSource(inputAsset, bindings);
                _src.OnInput += HandleInput;
            } else Debug.LogError("[PlayerPawnController] InputActionAsset 未设置！");
        }

        void OnEnable()  { _src?.Enable(); }
        void OnDisable() { _src?.Disable(); }

        void Start() {
            if (autoPossess != null) Possess(autoPossess);
        }

        public override void Tick(float dt) {
            if (_src == null || Pawn == null) return;
            Pawn.ReceiveIntent(PawnIntent.Move(_src.ReadMove()));

            // 将鼠标屏幕坐标射线投影到 Y=0 平面，得到世界坐标方向
            var screenPos = _src.ReadLook();
            if (screenPos.sqrMagnitude > 0.01f && gameCamera != null) {
                Ray ray = gameCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
                var plane = new UnityEngine.Plane(Vector3.up, Vector3.zero);
                if (plane.Raycast(ray, out float dist)) {
                    Vector3 worldPoint = ray.GetPoint(dist);
                    Vector3 dir = worldPoint - Pawn.Transform.position;
                    dir.y = 0;
                    if (dir.sqrMagnitude > 0.01f)
                        Pawn.ReceiveIntent(PawnIntent.Look(new Vector2(dir.x, dir.z)));
                }
            }
        }

        /// <summary>核心翻译：输入事件 → 语义意图。</summary>
        void HandleInput(PawnInputEvent e) {
            if (Pawn == null) return;
            switch (e.Kind) {
                case PawnInputKind.AttackPrimary_Down: Pawn.ReceiveIntent(PawnIntent.Primary()); break;
                case PawnInputKind.AttackHeavy_Down:   Pawn.ReceiveIntent(PawnIntent.Heavy());   break;
                case PawnInputKind.Dodge_Down:
                    Pawn.ReceiveIntent(PawnIntent.Dodge(_src.ReadMove()));
                    break;
                case PawnInputKind.Interact_Down:  Pawn.ReceiveIntent(PawnIntent.Interact()); break;
                case PawnInputKind.UseTool_Down:   Pawn.ReceiveIntent(PawnIntent.UseTool());  break;
                case PawnInputKind.Charge_Down:    Pawn.ReceiveIntent(PawnIntent.ChargeStart()); break;
                case PawnInputKind.Charge_Up:      Pawn.ReceiveIntent(PawnIntent.ChargeRelease(0f)); break;
                case PawnInputKind.SwitchTool_Next: Pawn.ReceiveIntent(PawnIntent.SwitchTool(+1)); break;
                case PawnInputKind.SwitchTool_Prev: Pawn.ReceiveIntent(PawnIntent.SwitchTool(-1)); break;
                case PawnInputKind.SwitchTool_Slot: Pawn.ReceiveIntent(PawnIntent.SwitchTool(e.IntPayload)); break;
            }
        }
    }
}
