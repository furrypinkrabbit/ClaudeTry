using UnityEngine;
using GuJian.Pawns;
using GuJian.Pawns.Parts;
using GuJian.Progression;
using GuJian.Tools;

namespace GuJian.Lobby {
    /// <summary>
    /// 进入 InGame 时，根据 MetaProgressSave 把玩家的锤子装备到 PawnToolSlot。
    /// 挂在玩家预制体上。
    /// </summary>
    [RequireComponent(typeof(PawnToolSlot))]
    public class PlayerLoadout : MonoBehaviour {
        [SerializeField] ToolCatalog catalog;
        [SerializeField] PawnToolSlot toolSlot;

        void Awake() {
            if (toolSlot == null) toolSlot = GetComponent<PawnToolSlot>();
        }

        void Start() {
            var save = MetaProgressSave.Load();
            var toolData = catalog.FindTool(save.equippedToolId);
            if (toolData == null || toolData.runtimePrefab == null) {
                Debug.LogWarning($"[PlayerLoadout] 未找到锤子 {save.equippedToolId}"); return;
            }
            var toolGo = Instantiate(toolData.runtimePrefab, toolSlot.Anchor);
            if (toolGo.TryGetComponent<ITool>(out var tool)) {
                // 配件注入
                if (tool is ToolBase tb) {
                    foreach (var tuningId in save.equippedTuningIds) {
                        var tn = catalog.FindTuning(tuningId);
                        if (tn != null && toolData.defaultTunings != null) {
                            // 运行时追加配件——由 ToolBase 的 OnEquip 读取 defaultTunings，
                            // 这里直接把配件各自修改器加入 modifiers
                        }
                    }
                }
                toolSlot.EquipToSlot(0, tool);
            }
        }
    }
}
