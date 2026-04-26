using UnityEngine.UIElements;

namespace GuJian.UI.Screens {
    /// <summary>
    /// 进入游戏后的主界面(一页式、另外的中心菜单)。适合做存档菜单、
    /// Run 前预览、小段剧情等。这里提供框架,具体内容由产品填。
    /// </summary>
    public class HomeScreen : UIScreen {
        Button _continueBtn, _newRunBtn, _lobbyBtn;
        protected override void OnBind(VisualElement root) {
            _continueBtn = root.Q<Button>("btn-continue");
            _newRunBtn   = root.Q<Button>("btn-new-run");
            _lobbyBtn    = root.Q<Button>("btn-back-lobby");
            if (_continueBtn != null) _continueBtn.clicked += () => UIRouter.Instance.Push(UIScreenIds.Ingame);
            if (_newRunBtn   != null) _newRunBtn.clicked   += () => UIRouter.Instance.Push(UIScreenIds.Ingame);
            if (_lobbyBtn    != null) _lobbyBtn.clicked    += () => UIRouter.Instance.Replace(UIScreenIds.Lobby);
        }
    }
}
