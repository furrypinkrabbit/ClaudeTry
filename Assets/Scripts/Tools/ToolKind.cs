namespace GuJian.Tools {
    /// <summary>
    /// 工具大类。加新工具只需扩枚举 + 写一个 ToolBase 实现。
    /// </summary>
    public enum ToolKind {
        Hammer,   // 锤
        Chisel,   // 凿
        Brush,    // 刷
        Plane,    // 刨
        Awl,      // 锥
        Saw,      // 锯
    }

    public enum ToolActionType {
        Primary,
        Heavy,
        Special,
        ChargeStart,
        ChargeRelease,
    }
}
