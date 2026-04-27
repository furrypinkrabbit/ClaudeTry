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
        [SerializeField] bool skipIntro = false;

        GameModeBase _current;

        void Awake() {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        void Start() { if (skipIntro) GoLobby(); }

        public void GoLobby()  => SwitchTo(lobbyScene,  isLobby: true);
        public void StartRun() => SwitchTo(ingameScene, isLobby: false);

        void SwitchTo(string sceneName, bool isLobby) {
            _current?.Exit();
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null) { Debug.LogError($"Scene '{sceneName}' 未加入 Build Settings"); return; }
            op.completed += _ => {
                var scene = SceneManager.GetSceneByName(sceneName);
                GameModeBase mode;
                if (isLobby)
                {
                    var go = new GameObject("LobbyGameMode");
                    SceneManager.MoveGameObjectToScene(go, scene);
                    mode = go.AddComponent<LobbyGameMode>();
                }
                else
                {
                    // 出路 A:优先用场景里手摆的(带 Inspector 引用),找不到再 AddComponent 兜底
                    mode = FindAnyObjectByType<InGameGameMode>();
                    if (mode == null)
                    {
                        var go = new GameObject("InGameGameMode");
                        SceneManager.MoveGameObjectToScene(go, scene);
                        mode = go.AddComponent<InGameGameMode>();
                    }
                }
                _current = mode;
                mode.Enter();
            };

        }
    }
}
