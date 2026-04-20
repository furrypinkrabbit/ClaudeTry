using UnityEngine;
using UnityEngine.SceneManagement;
using GuJian.Core;

namespace GuJian.GameFlow {
    /// <summary>
    /// 启动器：管理 GameMode 切换、场景加载。挂在 Boot.unity 唯一的一个 GO 上。
    /// </summary>
    public class GameBootstrap : MonoBehaviour {
        public static GameBootstrap Instance { get; private set; }

        [SerializeField] string lobbyScene  = "Lobby";
        [SerializeField] string ingameScene = "InGame";

        GameModeBase _current;

        void Awake() {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        void Start() { GoLobby(); }

        public void GoLobby()  => SwitchTo(lobbyScene,  isLobby: true);
        public void StartRun() => SwitchTo(ingameScene, isLobby: false);

        void SwitchTo(string sceneName, bool isLobby) {
            _current?.Exit();
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null) { Debug.LogError($"Scene '{sceneName}' 未加入 Build Settings"); return; }
            op.completed += _ => {
                var scene = SceneManager.GetSceneByName(sceneName);
                var go = new GameObject(isLobby ? "LobbyGameMode" : "InGameGameMode");
                SceneManager.MoveGameObjectToScene(go, scene);
                GameModeBase mode = isLobby
                    ? (GameModeBase)go.AddComponent<LobbyGameMode>()
                    : go.AddComponent<InGameGameMode>();
                _current = mode;
                mode.Enter();
            };
        }
    }
}
