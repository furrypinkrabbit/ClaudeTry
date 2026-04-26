namespace GuJian.Audio {
    /// <summary>
    /// 音频分类。一个分类 → 一个 Mixer Group + 一个播放池状态机。
    /// 与用户需求一一对应:特效/场景/动作/转场/切换/界面。
    /// </summary>
    public enum AudioCategory {
        /// <summary>通用一次性特效(例:修复完成的一碬)。</summary>
        Sfx        = 0,
        /// <summary>场景 BGM。同时最多一首,交叉淡入/淡出。</summary>
        Scene      = 1,
        /// <summary>动作音(撞击/风响/脚步),密集,需多通道并发。</summary>
        Action     = 2,
        /// <summary>转场/过场音。一般独占,播完自停。</summary>
        Transition = 3,
        /// <summary>界面内通用切换音(页面 → 页面)。</summary>
        Switch     = 4,
        /// <summary>界面反馈(点击/悬停/确认)。</summary>
        Ui         = 5,
    }
}
