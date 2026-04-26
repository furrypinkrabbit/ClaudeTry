using UnityEngine;
using GuJian.Audio;
using GuJian.UI;

namespace GuJian.GameFlow
{
    /// Lobby 场景入口:UI 切到大厅屏 + 播大厅音乐。
    public class LobbyEnter : MonoBehaviour
    {
        [SerializeField] string screenId = UIScreenIds.Lobby; // "lobby"
        [SerializeField] string sceneMusicId = "bgm_lobby";

        void Start()
        {
            UIRouter.Instance?.Replace(screenId);
            if (!string.IsNullOrEmpty(sceneMusicId))
                AudioManager.Instance?.PlayScene(sceneMusicId);
        }
    }
}
