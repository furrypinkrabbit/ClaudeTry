using System;
using UnityEngine;
using GuJian.Progression;
using GuJian.Tools;

namespace GuJian.Lobby {
    /// <summary>
    /// 大厅商店。执行购买/装备的业务层，UI 只负责调用。
    /// </summary>
    public class ToolShop : MonoBehaviour {
        [SerializeField] ToolCatalog catalog;
        public event Action OnSaveChanged;

        public MetaProgressSave Save { get; private set; }

        void Awake() { Save = MetaProgressSave.Load(); }

        public bool TryBuyTool(string toolId, int price) {
            if (Save.matterSilver < price) return false;
            if (Save.unlockedToolIds.Contains(toolId)) return false;
            Save.matterSilver -= price;
            Save.unlockedToolIds.Add(toolId);
            Save.Save(); OnSaveChanged?.Invoke(); return true;
        }

        public bool TryBuyTuning(string tuningId, int price) {
            if (Save.matterSilver < price) return false;
            if (Save.ownedTuningIds.Contains(tuningId)) return false;
            Save.matterSilver -= price;
            Save.ownedTuningIds.Add(tuningId);
            Save.Save(); OnSaveChanged?.Invoke(); return true;
        }

        public void EquipTool(string toolId) {
            if (!Save.unlockedToolIds.Contains(toolId)) return;
            Save.equippedToolId = toolId;
            Save.Save(); OnSaveChanged?.Invoke();
        }

        public void EquipTuning(string tuningId, bool on) {
            if (on) {
                if (!Save.ownedTuningIds.Contains(tuningId)) return;
                if (!Save.equippedTuningIds.Contains(tuningId))
                    Save.equippedTuningIds.Add(tuningId);
            } else {
                Save.equippedTuningIds.Remove(tuningId);
            }
            Save.Save(); OnSaveChanged?.Invoke();
        }
    }
}
