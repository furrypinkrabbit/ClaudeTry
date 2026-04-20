using System.Collections.Generic;
using UnityEngine;
using GuJian.Tools;

namespace GuJian.Lobby {
    /// <summary>
    /// 游戏中所有工具 + 配件的在上查询目录。按 toolId / tuningId 寻查。
    /// </summary>
    [CreateAssetMenu(menuName = "GuJian/Lobby/ToolCatalog", fileName = "ToolCatalog")]
    public class ToolCatalog : ScriptableObject {
        public List<ToolData> tools = new();
        public List<ToolTuning> tunings = new();

        public ToolData FindTool(string id) {
            foreach (var t in tools) if (t != null && t.toolId == id) return t;
            return null;
        }
        public ToolTuning FindTuning(string id) {
            foreach (var t in tunings) if (t != null && t.tuningId == id) return t;
            return null;
        }
    }
}
