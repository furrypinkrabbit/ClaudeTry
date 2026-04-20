using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GuJian.Input {
    /// <summary>
    /// 玩家输入源。封装 Unity New Input System 的 InputActionAsset。
    /// 玩家在 Inspector 绑定 InputActionAsset + 指定各 Action 名称即可，
    /// 绝对不接触 InputAction.CallbackContext。
    /// </summary>
    [Serializable]
    public class PlayerInputSource : IPawnInputSource {
        [Serializable]
        public struct Bindings {
            public string ActionMap;          // e.g. "Gameplay"
            public string Move;               // Value<Vector2>
            public string Look;               // Value<Vector2>
            public string AttackPrimary;      // Button
            public string AttackHeavy;        // Button
            public string Dodge;              // Button
            public string Interact;           // Button
            public string UseTool;            // Button
            public string SwitchToolNext;     // Button
            public string SwitchToolPrev;     // Button
            public string SlotSelect;         // Value<int> or delta from scroll
            public string Charge;             // Button (Press + Release)
        }

        readonly InputActionAsset _asset;
        readonly Bindings _b;
        InputActionMap _map;
        public event Action<PawnInputEvent> OnInput;

        InputAction a_move, a_look, a_atk, a_heavy, a_dodge, a_interact,
                    a_useTool, a_switchNext, a_switchPrev, a_slot, a_charge;

        public PlayerInputSource(InputActionAsset asset, Bindings bindings) {
            _asset = asset;
            _b = bindings;
        }

        public void Enable() {
            _map = _asset.FindActionMap(_b.ActionMap, throwIfNotFound: true);
            a_move        = _map.FindAction(_b.Move,         throwIfNotFound: false);
            a_look        = _map.FindAction(_b.Look,         throwIfNotFound: false);
            a_atk         = _map.FindAction(_b.AttackPrimary,throwIfNotFound: false);
            a_heavy       = _map.FindAction(_b.AttackHeavy,  throwIfNotFound: false);
            a_dodge       = _map.FindAction(_b.Dodge,        throwIfNotFound: false);
            a_interact    = _map.FindAction(_b.Interact,     throwIfNotFound: false);
            a_useTool     = _map.FindAction(_b.UseTool,      throwIfNotFound: false);
            a_switchNext  = _map.FindAction(_b.SwitchToolNext, throwIfNotFound: false);
            a_switchPrev  = _map.FindAction(_b.SwitchToolPrev, throwIfNotFound: false);
            a_slot        = _map.FindAction(_b.SlotSelect,   throwIfNotFound: false);
            a_charge      = _map.FindAction(_b.Charge,       throwIfNotFound: false);

            Bind(a_atk,        PawnInputKind.AttackPrimary_Down, PawnInputKind.AttackPrimary_Up);
            Bind(a_heavy,      PawnInputKind.AttackHeavy_Down,   PawnInputKind.AttackHeavy_Up);
            Bind(a_dodge,      PawnInputKind.Dodge_Down,         null);
            Bind(a_interact,   PawnInputKind.Interact_Down,      null);
            Bind(a_useTool,    PawnInputKind.UseTool_Down,       null);
            Bind(a_switchNext, PawnInputKind.SwitchTool_Next,    null);
            Bind(a_switchPrev, PawnInputKind.SwitchTool_Prev,    null);
            Bind(a_charge,     PawnInputKind.Charge_Down,        PawnInputKind.Charge_Up);

            if (a_slot != null) {
                a_slot.performed += ctx => {
                    int slot = Mathf.RoundToInt(ctx.ReadValue<float>());
                    OnInput?.Invoke(new PawnInputEvent(PawnInputKind.SwitchTool_Slot, default, 0f, slot));
                };
            }

            _map.Enable();
        }

        void Bind(InputAction a, PawnInputKind? onDown, PawnInputKind? onUp) {
            if (a == null) return;
            if (onDown.HasValue) a.performed += ctx => OnInput?.Invoke(new PawnInputEvent(onDown.Value));
            if (onUp.HasValue)   a.canceled  += ctx => OnInput?.Invoke(new PawnInputEvent(onUp.Value));
        }

        public void Disable() {
            _map?.Disable();
        }

        /// <summary>需要控制器每帧轮询移动轴，没必要用事件。</summary>
        public Vector2 ReadMove() => a_move != null ? a_move.ReadValue<Vector2>() : default;
        public Vector2 ReadLook() => a_look != null ? a_look.ReadValue<Vector2>() : default;
    }
}
