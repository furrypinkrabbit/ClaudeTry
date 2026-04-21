using UnityEngine.UIElements;
using GuJian.GameFlow;

namespace GuJian.UI.Screens {
    /// <summary>
    /// 大厅(外层主菜单)。定位:初次进入游戏所见的面孔。
    /// 按钮:开始游戏 / 进入商店 / 背包 / 角色 / 退出
    /// </summary>
    public class LobbyScreen : UIScreen {
        Button _startBtn, _shopBtn, _invBtn, _charBtn, _quitBtn;
        Label  _titleLabel;
        Label  _subTitle;

        protected override void OnBind(VisualElement root) {
            _startBtn   = root.Q<Button>("btn-start");
            _shopBtn    = root.Q<Button>("btn-shop");
            _invBtn     = root.Q<Button>("btn-inventory");
            _charBtn    = root.Q<Button>("btn-character");
            _quitBtn    = root.Q<Button>("btn-quit");
            _titleLabel = root.Q<Label>("title");
            _subTitle   = root.Q<Label>("subtitle");

            if (_startBtn != null) _startBtn.clicked += OnStart;
            if (_shopBtn  != null) _shopBtn.clicked  += () => UIRouter.Instance.Push(UIScreenIds.Shop);
            if (_invBtn   != null) _invBtn.clicked   += () => UIRouter.Instance.Push(UIScreenIds.Inventory);
            if (_charBtn  != null) _charBtn.clicked  += () => UIRouter.Instance.Push(UIScreenIds.Character);
            if (_quitBtn  != null) _quitBtn.clicked  += OnQuit;
        }

        void OnStart() {
            if (GameBootstrap.Instance != null) {
                UIRouter.Instance.Replace(UIScreenIds.Ingame);
                GameBootstrap.Instance.StartRun();
            } else {
                UIRouter.Instance.Replace(UIScreenIds.Ingame);
            }
        }

        void OnQuit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
