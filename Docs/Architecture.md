# 古建修补匠 · Gameplay 架构设计

> 主题：古建文化 · 俯视 2.5D 视差 · 轻肉鸽
> 核心循环：挥锤 → 把"残缺结构"敲回正形（=击杀） → 获取「手艺」→ 升级 → 推进房间 → 修复最终结构（Boss） → 回到大厅 → 用沉淀的货币换/调锤子

---

## 0. 设计哲学

1. **Pawn-Controller 分离**：一切可被"驱动"的东西都是 `Pawn`；任何驱动来源（玩家输入 / AI / 回放）都是 `PawnController`。
2. **语义化输入**：玩家只声明"按键 → Action"映射，**绝不接触 `InputAction.CallbackContext`**。控制器把 Action 翻译成**语义意图** `PawnIntent`，Pawn 和工具只认意图，不认按键。
3. **工具即策略**：战斗动作不写死在角色里。角色只会"挥动当前工具"，工具自己决定形状 / 伤害 / 节奏 / 配件。换锤子/换工具=换策略对象。
4. **数据驱动**：所有可拓展内容（工具、结构敌人、房间、升级项、锤子配件）都是 `ScriptableObject`，新内容=新 `.asset`，不需改代码。

---

## 1. 分层总览

```
┌────────────────────────────────────────────────────────────────┐
│                          GameFlow 层                            │
│   GameBootstrap · LobbyGameMode · InGameGameMode · RunContext   │
└───────────▲─────────────────────────▲───────────────────────────┘
            │                         │
┌───────────┴────────────┐  ┌─────────┴─────────────────┐
│        Lobby 层         │  │         Run 层             │
│ MetaWallet · ToolShop   │  │ RoomManager · Waves · Boss │
│ UnlockManager           │  │ LevelUpSystem · Craftsmanship│
└───────────▲────────────┘  └─────────▲─────────────────┘
            │                         │
            └──────────┬──────────────┘
                       │
┌──────────────────────┴──────────────────────────────────────────┐
│                     Pawn · Controller 层                         │
│                                                                  │
│  [InputActionAsset]──┐                                           │
│                      ▼                                           │
│  IPawnInputSource ──►  IPawnController  ──possess──►  IPawn     │
│        ▲                   │translate                  │         │
│        │ 按键流              ▼ PawnIntent                ▼         │
│   (玩家/AI/回放)         发送意图              Pawn 部件:          │
│                                               Movement / Combat   │
│                                               Health / ToolSlot   │
└─────────────────────────────────────────────────────────────────┘
                       │
┌──────────────────────┴──────────────────────────────────────────┐
│                        Tool · Target 层                          │
│  ITool(Hammer/Chisel/Brush...) → Swing → IRepairable Hit        │
│  StructureEnemyPawn : IPawn + IRepairable                       │
│  StructureData(Type × Material × Rarity)                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Pawn-Controller 模型

### 2.1 意图 (`PawnIntent`)
一切上层驱动都最终归一成意图。意图是**结构体** + 枚举类型，0 GC。

```csharp
public enum PawnIntentKind {
    MoveAxis,       // Vector2 移动轴
    LookAxis,       // Vector2 朝向轴（瞄准/视角）
    AttackPrimary,  // 主攻击
    AttackHeavy,    // 重击
    Dodge,          // 翻滚/闪避
    Interact,       // 交互
    UseTool,        // 触发工具特殊
    SwitchTool,     // IntPayload = slot index
    ChargeStart,    // 蓄力开始
    ChargeEnd,      // 蓄力结束 (Scalar = 蓄力秒数)
}

public readonly struct PawnIntent {
    public readonly PawnIntentKind Kind;
    public readonly Vector2 Vector;
    public readonly float    Scalar;
    public readonly int      IntPayload;
    // ctor + factory methods
}
```

### 2.2 输入源 (`IPawnInputSource`)
抽象"按键从哪儿来"。玩家实现包了 `InputActionAsset`，AI 实现包了 BT/决策。

```csharp
public interface IPawnInputSource {
    void Enable();
    void Disable();
    // 由 Controller 订阅
    event Action<PawnInputEvent> OnInput;
}
```

`PawnInputEvent` 只是**半原始**事件（比如 "Move 向量变了" / "Attack 按下了"），不带 `InputAction.CallbackContext`。

### 2.3 控制器 (`IPawnController`)
`IPawnInputSource.OnInput` → 翻译 → `PawnIntent` → `Pawn.ReceiveIntent()`。

```csharp
public interface IPawnController {
    IPawn Pawn { get; }
    void Possess(IPawn pawn);
    void Unpossess();
    void Tick(float dt);   // 允许控制器在 Update 里累积状态（比如按住蓄力）
}
```

- `PlayerPawnController`
  - 持有 `InputActionAsset` 引用（玩家自定义）
  - 内部构造 `PlayerInputSource`，订阅 Move / Look / Attack / Heavy / Dodge / Interact / Tool / Charge
  - **玩家只在 Inspector 指定这些 Action 名字**，不写回调
- `AIPawnController`
  - 持有 `IAIBrain`（可以是 FSM 或 BehaviorTree）
  - 把 AI 决定的"我要往这走"翻译成 `PawnIntent.MoveAxis`，把"我要攻击"翻译成 `PawnIntent.AttackPrimary`
  - 敌人因此与玩家 **100% 复用同一套 Pawn 执行逻辑**

### 2.4 Pawn (`IPawn`)
`IPawn` 本身很薄：接收意图 + 暴露部件查询。所有具体行为在部件里。

```csharp
public interface IPawn {
    Transform Transform { get; }
    bool IsAlive { get; }
    void ReceiveIntent(in PawnIntent intent);
    T GetPart<T>() where T : class;
}
```

Pawn 的**部件** (`PawnPartBase : MonoBehaviour`) 注册到 Pawn 上，在 `ReceiveIntent` 时按 Kind 路由：
- `PawnMovement` ← MoveAxis / LookAxis / Dodge
- `PawnCombat`   ← AttackPrimary / AttackHeavy / ChargeStart / ChargeEnd → 转发给 `PawnToolSlot.Current.Trigger(...)`
- `PawnToolSlot` ← SwitchTool / UseTool
- `PawnHealth`   ← 被动部件，不接受意图
- `PawnInteractor` ← Interact

这样做的好处：
1. 同一个 `StructureEnemyPawn` 既可被 AI 驱动巡逻，也可在 Debug 时被 `PlayerPawnController` 附身直接操控（开发效率高）。
2. 加新行为=加新部件，不改 Pawn 本体。

---

## 3. Tool 系统（锤子 & 可拓展工具）

### 3.1 数据 (`ToolData : ScriptableObject`)
```csharp
public enum ToolKind { Hammer, Chisel, Brush, Plane, Awl /* 锤/凿/刷/刨/锥 */ }

public class ToolData : ScriptableObject {
    public string       toolId;
    public string       displayName;
    public Sprite       icon;
    public ToolKind     kind;

    public float        baseDamage;
    public float        swingCooldown;
    public float        range;
    public float        swingArcDeg;
    public float        staminaCost;
    public float        chargeMaxSeconds;
    public float        chargeDamageMul;

    // 材质亲和：对某种结构材质 × 额外伤害
    public MaterialAffinity[] affinities;
    // 配件槽位（大厅里可以买配件装进去）
    public int          tuningSlots;
    public GameObject   runtimePrefab;  // 实际行为/模型
}
```

### 3.2 运行时 (`ITool` / `ToolBase`)
```csharp
public interface ITool {
    ToolData Data { get; }
    IPawn Owner { get; }
    void OnEquip(IPawn owner);
    void OnUnequip();
    void Trigger(ToolActionType type, in ToolContext ctx);
    void Tick(float dt);
}

public enum ToolActionType { Primary, Heavy, Special, ChargeStart, ChargeRelease }
```

锤子的具体实现 `HammerTool : ToolBase`：在 `Primary` 时做一次扇形击打（`SwingResolver` 负责 OverlapBox/扇形判定），命中 `IRepairable` → 派发 `HitInfo`。

### 3.3 配件 (`ToolTuning : ScriptableObject`)
锤头/锤柄/包铁等。装到 `ToolData.tunings` 槽 → 运行时构成"属性修饰器链" (`IToolModifier`) → 计算最终属性。这样**同一把锤子也能有多种配置**，满足你说的"大厅买锤子的配置"。

### 3.4 拓展规范
新工具只需：
1. 新 `ToolKind` 枚举值；
2. 新 `ToolData` asset；
3. 新 `XxxTool : ToolBase` 实现 `Trigger`；
4. 塞进 `MetaProgression.UnlockedTools` 并在 `ToolShop` 挂价。

**完全不需要改 Pawn/Controller/战斗解算层。**

---

## 4. Structure Enemy（残缺结构敌人）

### 4.1 概念
敌人 = "一个原本应该长成某个样子的古建构件，现在缺了 / 歪了 / 裂了"。
玩家每一次命中，让它**朝正形恢复一步**；恢复到 0 残缺度 = 修复 = 击杀。

### 4.2 数据
```csharp
public enum StructureType     { DouGong, SunMao, WaDang, ChiWen, QueTi, LattieWin /* 斗拱/榫卯/瓦当/鸱吻/雀替/棂花 */ }
public enum StructureMaterial { Wood, Stone, Bronze, Jade, Lacquer /* 木/石/铜/玉/漆 —— 玉/漆 为精英 */ }

public class StructureData : ScriptableObject {
    public StructureType     type;
    public StructureMaterial material;    // 决定硬度/亲和
    public int               brokenness;  // 初始残缺度（=HP）
    public float             moveSpeed;
    public AttackPattern     attackPattern;
    public Sprite[]          repairStages; // 从最破到完整的分帧
    public CraftsmanshipReward reward;
    public DropTable         drops;
}
```

### 4.3 运行时
```csharp
public class StructureEnemyPawn : MonoBehaviour, IPawn, IRepairable {
    [SerializeField] StructureData data;
    int currentBrokenness;

    public void ApplyHit(in HitInfo hit) {
        // 1. 材质亲和 → 实际修复量
        int repair = ResolveRepairAmount(hit);
        currentBrokenness = Mathf.Max(0, currentBrokenness - repair);
        // 2. 换下一阶段 Sprite（视觉"被敲回正形"）
        UpdateStageSprite();
        // 3. 为0 → 修复完成
        if (currentBrokenness == 0) OnRepaired();
    }
}
```

- **精英怪** = 更硬的材质（Jade / Lacquer），对特定工具亲和负值 → 需要换工具或蓄力。
- **小怪种类** = 不同 `StructureType`：斗拱会挥肘顶人、瓦当会滚、鸱吻会喷……攻击模式挂 `AttackPattern` SO。

---

## 5. 进度系统

| 层级 | 容器 | 货币 | 生命周期 | 用途 |
|------|------|------|----------|------|
| Run（一次跑图） | `RunContext` | **手艺 (Craftsmanship)** | 本 Run 结束归零 | 升级 5 大成长项 |
| Meta（大厅） | `MetaProgressSave` | **匠银 (MatterSilver)** | 持久化 | 买工具/配件/永久解锁 |

- `CraftsmanshipWallet`：击杀给手艺，攒满升级 → 弹 `LevelUpPanel` 三选一（`UpgradeOption : SO`）。5 大 tree 对应 Day4 plan：榫卯加固 / 台基筑基 / 曲径通幽 / 瓦当护佑 / 斗拱承力。
- `RunSummary`：Run 结束按"修复结构数 × 材质系数 + Boss 加成 + 文化事件加成"换算成匠银，回大厅解锁。

---

## 6. 游戏流程 (GameMode)

```csharp
public abstract class GameModeBase : MonoBehaviour {
    public abstract void Enter();
    public abstract void Exit();
}
```

- **LobbyGameMode**：
  大厅场景，显示角色、工具架、数据碑（上次 Run 统计）、商店 NPC、出征门（点击→`StartRun()`）。
- **InGameGameMode**：
  - `RunContext` 负责本次 Run 的所有状态
  - `RoomManager` 按 plan Day2 的 4 类房间线性随机连接：院落前厅 → 回廊 → 殿内大厅 → 偏院厢房…… 最终 Boss 房
  - `WaveDirector` 用 `StructureSpawnTable` 在房间里刷残缺结构
  - 事件房（祈福/寻韵/休憩）用 `RoomType.Event` + `EventHouseData` 驱动
  - Boss：`BossEncounter` = 一整个巨型残缺结构，`StructurePart[]` 分阶段修复 + 技能（檐角冲击、斗拱镇压）

切换：`GameBootstrap.SwitchMode(new InGameGameMode())` / `new LobbyGameMode()`，通过 `SceneLoader` 异步加载。

---

## 7. 拓展规范清单（给未来加内容的"契约"）

| 想加什么 | 要动什么 |
|---------|---------|
| 新工具 | 新 `ToolKind` + 新 `ToolData.asset` + 新 `XxxTool : ToolBase` |
| 工具配件 | 新 `ToolTuning.asset`（实现 `IToolModifier`）|
| 新敌人 | 新 `StructureData.asset`（够用），或 + 新 `AttackPattern` |
| 新升级项 | 新 `UpgradeOption.asset`（实现 `IUpgradeEffect`）|
| 新房间 | 新 `RoomPrefab` + 新 `RoomData.asset` |
| 新事件房 | 新 `EventHouseData.asset` |
| 新材质 | 新 `StructureMaterial` 枚举值 + `MaterialAffinity` 条目 |
| 新 Boss | 新 `BossEncounterData.asset` + 若干 `StructurePart` |

---

## 8. 目录结构

```
Assets/
├─ Scripts/
│  ├─ Core/            # 基础接口、服务定位器、事件总线
│  ├─ Input/           # InputActionAsset、InputSource
│  ├─ Controllers/     # PlayerPawnController, AIPawnController
│  ├─ Pawns/           # IPawn, PawnBase, 各部件
│  ├─ Tools/           # ITool, ToolBase, HammerTool, Tuning
│  ├─ Structures/      # StructureEnemyPawn, StructureData
│  ├─ Combat/          # HitInfo, SwingResolver, DamagePipeline
│  ├─ Progression/     # Craftsmanship, LevelUpSystem, MetaSave
│  ├─ Rooms/           # RoomManager, RoomData, WaveDirector
│  ├─ GameFlow/        # GameBootstrap, GameMode, SceneLoader
│  ├─ Lobby/           # ToolShop, UnlockManager
│  ├─ UI/              # 所有 UI 控件
│  └─ Data/            # ScriptableObject 基类
├─ Art/SVG/            # 统一古风 SVG 资源
├─ Prefabs/
├─ Scenes/
└─ Resources/
```

---

## 9. 样式统一 · 古风色板

| 用途 | 中文 | Hex |
|------|------|-----|
| 主色 | 朱砂红 | `#B8352F` |
| 辅色 | 墨黑 | `#1C1A17` |
| 底色 | 宣纸米 | `#F2E5C8` |
| 点缀 | 青黛 | `#3E5C76` |
| 高光 | 鎏金 | `#C9A24C` |
| 血色 | 残缺朱 | `#7A1E12` |

所有 SVG 都从这 6 色里取，保证视觉统一。
