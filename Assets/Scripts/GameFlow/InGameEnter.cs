using UnityEngine;
using GuJian.Audio;
using GuJian.UI;

namespace GuJian.GameFlow
{
    /// InGame 场景入口:UI 切 HUD + 播战斗音乐。
    public class InGameEnter : MonoBehaviour
    {
        [SerializeField] string screenId = UIScreenIds.Ingame; // "ingame"
        [SerializeField] string sceneMusicId = "bgm_scene_1";

        void Start()
        {
            UIRouter.Instance?.Replace(screenId);
            if (!string.IsNullOrEmpty(sceneMusicId))
                AudioManager.Instance?.PlayScene(sceneMusicId);
        }
    }
}
