namespace GuJian.Structures {
    /// <summary>结构大类：决定造型/攻击风格。</summary>
    public enum StructureType {
        DouGong,    // 斗拱
        SunMao,     // 榫卯
        WaDang,     // 瓦当
        ChiWen,     // 鸱吻
        QueTi,      // 雀暿
        LatticeWin, // 棱花窗
    }

    /// <summary>结构材质：决定硬度与稀有程度。玉/漆 属于精英。</summary>
    public enum StructureMaterial {
        Wood   = 0,
        Stone  = 1,
        Bronze = 2,
        Jade   = 3,   // 精英
        Lacquer = 4,  // 精英（漆）
    }
}
