using UnityEngine;
using GuJian.Progression;

namespace GuJian.GameFlow {
    /// <summary>
    /// 大厅玩法模式。负责读存档 + 推送到 UI。
    /// </summary>
    public class LobbyGameMode : GameModeBase {
        public MetaProgressSave Save { get; private set; }
        public event System.Action<MetaProgressSave> OnSaveLoaded;

        public override void Enter() {
            Save = MetaProgressSave.Load();
            OnSaveLoaded?.Invoke(Save);
        }

        public override void Exit() {
            Save?.Save();
        }
    }
}
