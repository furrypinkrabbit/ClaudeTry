using System;

namespace GuJian.Input {
    /// <summary>
    /// 按键事件源抽象。涵盖：玩家（InputActionAsset）/ AI / 回放 / 设备宿主。
    /// 控制器的专属输入接口：订阅 OnInput 即可，无需接触 InputAction。
    /// </summary>
    public interface IPawnInputSource {
        void Enable();
        void Disable();
        event Action<PawnInputEvent> OnInput;
    }
}
