using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using GuJian.Pawns;
using GuJian.Pawns.Parts;
using GuJian.Controllers;
using GuJian.Tools;
using GuJian.Structures;
using GuJian.Input;
using GuJian.Progression;

namespace GuJian.DevTools {
    /// <summary>
    /// 一键冒烟测试：在空场景里挂这一个脚本，按 Play 即可:
    ///   - 生成一个 CharacterController 玩家(带 Pawn + 锤子 + Controller + Input)
    ///   - 生成一个残缺结构敌人做靶子
    ///   - 生成摄像机 / 光 / 地面
    ///   - 生成 DebugHUD 显示状态
    ///
    /// 按键映射(内建,不依赖任何 .inputactions 资源):
    ///   WASD          - 移动
    ///   鼠标移动      - 朝向
    ///   J / 鼠标左键  - 普攻
    ///   K / 鼠标右键  - 重击
    ///   L            - 蓄力(按住释放)
    ///   Space        - 闪避
    ///   E            - 交互
    ///   Q            - UseTool
    ///   [ / ]        - 切换工具
    ///   R            - 重新生成一个新敌人(便于连打)
    /// </summary>
    public class QuickTestBootstrap : MonoBehaviour {
        [Header("生成配置")]
        [SerializeField] int     enemyCount    = 3;
        [SerializeField] float   enemyDistance = 5f;
        [SerializeField] int     enemyBrokenness = 60;
        [SerializeField] StructureMaterial enemyMaterial = StructureMaterial.Wood;
        [SerializeField] bool    showDebugHud  = true;

        // 运行时资产(内存创建,不进Asset)
        InputActionAsset _inputAsset;
        ToolData         _hammerData;
        StructureData    _enemyData;

        // 主引用
        PlayerPawn            _player;
        PlayerPawnController  _controller;
        CraftsmanshipWallet   _wallet;

        void Awake() {
            BuildGround();
            BuildLight();
            BuildCamera();

            _inputAsset = BuildInputAsset();
            _hammerData = BuildHammerData();
            _enemyData  = BuildEnemyData(enemyBrokenness, enemyMaterial);

            _player     = BuildPlayer(_inputAsset, _hammerData, out _controller);
            BuildProgression(out _wallet);

            for (int i = 0; i < enemyCount; i++) {
                float a = (i / (float)enemyCount) * Mathf.PI * 2f;
                var pos = new Vector3(Mathf.Sin(a) * enemyDistance, 0f, Mathf.Cos(a) * enemyDistance);
                SpawnEnemy(_enemyData, pos);
            }

            if (showDebugHud) {
                var hud = new GameObject("DebugHUD").AddComponent<DebugHUD>();
                hud.Bind(_player, _wallet);
            }
        }

        void Update() {
            // 快捷:R 生成一只新敌人
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) {
                var p = _player != null ? _player.transform.position : Vector3.zero;
                var dir = Random.insideUnitCircle.normalized * enemyDistance;
                SpawnEnemy(_enemyData, p + new Vector3(dir.x, 0, dir.y));
            }
        }

        // ========== 场景搭建 ==========
        void BuildGround() {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(6, 1, 6); // 60x60
            var mr = ground.GetComponent<MeshRenderer>();
            if (mr != null) Tint(mr, new Color(0.95f, 0.89f, 0.78f)); // 宣纸米
        }

        void BuildLight() {
            var lgo = new GameObject("Sun");
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.1f;
            lgo.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        void BuildCamera() {
            var cgo = new GameObject("MainCamera");
            cgo.tag = "MainCamera";
            var cam = cgo.AddComponent<Camera>();
            cam.backgroundColor = new Color(0.11f, 0.10f, 0.09f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.fieldOfView = 55f;
            cgo.transform.position = new Vector3(0, 12f, -8f);
            cgo.transform.rotation = Quaternion.Euler(55f, 0, 0);
            cgo.AddComponent<AudioListener>();
            cgo.AddComponent<TopDownFollow>();
        }

        // ========== 运行时 InputActionAsset ==========
        InputActionAsset BuildInputAsset() {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "GuJianTestInput";
            var map = asset.AddActionMap("Gameplay");

            var move = map.AddAction("Move", InputActionType.Value);
            move.expectedControlType = "Vector2";
            move.AddCompositeBinding("2DVector")
                .With("Up",    "<Keyboard>/w")
                .With("Down",  "<Keyboard>/s")
                .With("Left",  "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            var look = map.AddAction("Look", InputActionType.Value);
            look.expectedControlType = "Vector2";
            look.AddBinding("<Mouse>/delta");

            map.AddAction("Attack",   InputActionType.Button).AddBinding("<Mouse>/leftButton");
            map["Attack"].AddBinding("<Keyboard>/j");
            map.AddAction("Heavy",    InputActionType.Button).AddBinding("<Mouse>/rightButton");
            map["Heavy"].AddBinding("<Keyboard>/k");
            map.AddAction("Charge",   InputActionType.Button).AddBinding("<Keyboard>/l");
            map.AddAction("Dodge",    InputActionType.Button).AddBinding("<Keyboard>/space");
            map.AddAction("Interact", InputActionType.Button).AddBinding("<Keyboard>/e");
            map.AddAction("UseTool",  InputActionType.Button).AddBinding("<Keyboard>/q");
            map.AddAction("ToolNext", InputActionType.Button).AddBinding("<Keyboard>/rightBracket");
            map.AddAction("ToolPrev", InputActionType.Button).AddBinding("<Keyboard>/leftBracket");
            return asset;
        }

        // ========== 运行时 ToolData ==========
        ToolData BuildHammerData() {
            var d = ScriptableObject.CreateInstance<ToolData>();
            d.toolId          = "test_hammer";
            d.displayName     = "测试锤";
            d.kind            = ToolKind.Hammer;
            d.baseDamage      = 22f;
            d.swingCooldown   = 0.45f;
            d.range           = 2.3f;
            d.swingArcDeg     = 120f;
            d.staminaCost     = 10f;
            d.supportsCharge  = true;
            d.chargeMaxSeconds= 1.2f;
            d.chargeDamageMul = 2.4f;
            d.chargeRangeMul  = 1.2f;
            d.tuningSlots     = 2;
            return d;
        }

        // ========== 运行时 StructureData ==========
        StructureData BuildEnemyData(int brokenness, StructureMaterial mat) {
            var d = ScriptableObject.CreateInstance<StructureData>();
            d.structureId    = "test_dougong";
            d.displayName    = "残缺·斗拱(测试)";
            d.type           = StructureType.DouGong;
            d.material       = mat;
            d.maxBrokenness  = brokenness;
            d.moveSpeed      = 0f; // 原地靶子
            d.contactDamage  = 0f;
            d.craftsmanshipReward = 10;
            return d;
        }

        // ========== 玩家 ==========
        PlayerPawn BuildPlayer(InputActionAsset input, ToolData hammerData, out PlayerPawnController controller) {
            var go = new GameObject("Player");
            go.tag = "Player";
            go.transform.position = new Vector3(0, 0.1f, 0);
            go.SetActive(false); // 先禁,等 SerializeField 反射注入后再启用

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.45f; cc.center = new Vector3(0, 0.9f, 0);

            // 视觉(子物体,避免重复碰撞)
            var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vis.name = "Visual";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localPosition = new Vector3(0, 0.9f, 0);
            DestroyImmediate(vis.GetComponent<CapsuleCollider>());
            Paint(vis, new Color(0.78f, 0.21f, 0.18f)); // 朱砂红

            // 工具挂点
            var anchorGo = new GameObject("ToolAnchor");
            anchorGo.transform.SetParent(go.transform, false);
            anchorGo.transform.localPosition = new Vector3(0.35f, 1.1f, 0.4f);

            // 部件
            go.AddComponent<PawnMovement>();
            var slot   = go.AddComponent<PawnToolSlot>();
            SetField(slot, "toolAnchor", anchorGo.transform);
            go.AddComponent<PawnCombat>();
            go.AddComponent<PawnHealth>();
            var pawn = go.AddComponent<PlayerPawn>();

            // 控制器(独立 GO)
            var ctrlGo = new GameObject("PlayerController");
            ctrlGo.SetActive(false);
            controller = ctrlGo.AddComponent<PlayerPawnController>();
            SetField(controller, "inputAsset", input);
            SetField(controller, "autoPossess", pawn);

            go.SetActive(true);
            ctrlGo.SetActive(true);

            // 锤子实例化 + 装配
            var hammerGo = new GameObject("Hammer");
            hammerGo.transform.SetParent(anchorGo.transform, false);
            var hammerVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hammerVis.transform.SetParent(hammerGo.transform, false);
            hammerVis.transform.localScale = new Vector3(0.15f, 0.15f, 0.45f);
            DestroyImmediate(hammerVis.GetComponent<BoxCollider>());
            Paint(hammerVis, new Color(0.79f, 0.63f, 0.30f)); // 鎏金
            var hammer = hammerGo.AddComponent<HammerTool>();
            SetField(hammer, "data", hammerData);

            slot.EquipToSlot(0, hammer);
            return pawn;
        }

        // ========== 敌人 ==========
        GameObject SpawnEnemy(StructureData data, Vector3 pos) {
            var go = new GameObject($"Enemy_{data.structureId}");
            go.transform.position = pos;
            go.SetActive(false);

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.6f; cc.radius = 0.5f; cc.center = new Vector3(0, 0.8f, 0);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "Visual";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localPosition = new Vector3(0, 0.8f, 0);
            vis.transform.localScale    = new Vector3(1.0f, 1.6f, 1.0f);
            DestroyImmediate(vis.GetComponent<BoxCollider>());
            var col = MaterialColor(data.material);
            Paint(vis, col);

            go.AddComponent<PawnMovement>();
            var enemy = go.AddComponent<StructureEnemyPawn>();
            SetField(enemy, "data", data);

            // 视觉反馈:按残缺度变色(DevTools 专用)
            go.AddComponent<EnemyBrokennessVisual>().Bind(enemy, vis.GetComponent<MeshRenderer>());
            go.SetActive(true);
            return go;
        }

        // ========== 进度/钱包 ==========
        void BuildProgression(out CraftsmanshipWallet wallet) {
            var go = new GameObject("Progression");
            wallet = go.AddComponent<CraftsmanshipWallet>();
            go.AddComponent<LevelUpSystem>(); // 有则有,无则算了
        }

        // ========== 工具函数 ==========
        static void SetField(object target, string name, object value) {
            var f = target.GetType().GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null) { Debug.LogError($"[QuickTest] field not found: {target.GetType().Name}.{name}"); return; }
            f.SetValue(target, value);
        }

        static void Paint(GameObject go, Color c) {
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) Tint(mr, c);
        }

        /// <summary>
        /// 给 MeshRenderer 上色,避免紫色 shader-missing 问题。
        /// 关键点:
        /// 1) 不 new Material + Shader.Find("...Lit") —— 运行时那个 shader 经常返回 null。
        ///    直接用 Primitive 自带默认材质的 shader(URP 项目就是 URP/Lit,Built-in 就是 Standard,永远对)。
        /// 2) 用 renderer.material (不是 sharedMaterial) 拿到实例化副本,每个 Renderer 独立着色。
        /// 3) 同时写 _Color (Built-in) 和 _BaseColor (URP/HDRP),保证两条管线都生效。
        /// </summary>
        static void Tint(MeshRenderer mr, Color c) {
            var mat = mr.material; // 实例化,shader 是管线正确的那个
            mat.color = c;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     c);
        }

        static Color MaterialColor(StructureMaterial m) => m switch {
            StructureMaterial.Wood    => new Color(0.55f, 0.36f, 0.22f),
            StructureMaterial.Stone   => new Color(0.68f, 0.66f, 0.62f),
            StructureMaterial.Bronze  => new Color(0.49f, 0.38f, 0.20f),
            StructureMaterial.Jade    => new Color(0.25f, 0.62f, 0.51f),
            StructureMaterial.Lacquer => new Color(0.48f, 0.12f, 0.08f),
            _                         => Color.gray,
        };
    }

    /// <summary>敌人残缺度变色:从深(破)到亮(近好)。DevTools 可视化用。</summary>
    public class EnemyBrokennessVisual : MonoBehaviour {
        StructureEnemyPawn _enemy;
        MeshRenderer       _mr;
        Color              _base;
        public void Bind(StructureEnemyPawn e, MeshRenderer mr) {
            _enemy = e; _mr = mr;
            if (mr != null) {
                var m = mr.material; // 已实例化
                // 优先从 _BaseColor 取(URP),否则取 color(Built-in)
                _base = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
            }
        }
        void Update() {
            if (_enemy == null || _mr == null) return;
            float t = 1f - (float)_enemy.CurrentBrokenness / Mathf.Max(1, _enemy.MaxBrokenness);
            // 越接近“修好”越亮
            var c = Color.Lerp(_base * 0.35f, _base * 1.25f, t);
            var mat = _mr.material;
            mat.color = c;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     c);
        }
    }

    /// <summary>让摄像机平滑跟随玩家。</summary>
    public class TopDownFollow : MonoBehaviour {
        Transform _target;
        Vector3   _offset;
        void LateUpdate() {
            if (_target == null) {
                var p = GameObject.FindWithTag("Player");
                if (p != null) {
                    _target = p.transform;
                    _offset = transform.position - _target.position;
                }
                return;
            }
            var want = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, want, Time.deltaTime * 6f);
        }
    }
}
