using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GuJian.Progression {
    /// <summary>
    /// 大厅的持久化存档。保存：匠银、已解锁工具 ID、已购配件、装备方案。
    /// 使用 JSON 存 persistentDataPath 下。
    /// </summary>
    [Serializable]
    public class MetaProgressSave {
        public int matterSilver;
        public List<string> unlockedToolIds = new();
        public List<string> ownedTuningIds  = new();
        public string equippedToolId;
        public List<string> equippedTuningIds = new();
        public int bestRunStructures;
        public int totalRuns;

        const string FileName = "meta_save.json";
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static MetaProgressSave Load() {
            try {
                if (File.Exists(Path)) {
                    var json = File.ReadAllText(Path);
                    return JsonUtility.FromJson<MetaProgressSave>(json) ?? new MetaProgressSave();
                }
            } catch (Exception e) { Debug.LogWarning($"Meta 存档读取失败: {e.Message}"); }
            var s = new MetaProgressSave();
            // 初始默认：送一把木槌
            s.unlockedToolIds.Add("hammer_wood");
            s.equippedToolId = "hammer_wood";
            return s;
        }

        public void Save() {
            try {
                var json = JsonUtility.ToJson(this, true);
                File.WriteAllText(Path, json);
            } catch (Exception e) { Debug.LogError($"Meta 存档写入失败: {e.Message}"); }
        }
    }
}
