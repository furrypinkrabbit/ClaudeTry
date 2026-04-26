# 古建修复者 · Unity 测试手册

> 回答「得在 Unity 项目里,什么场景下加什么、挂什么脚本」

项目有三档测试可以做,难度递增:

| 档 | 目的 | 需要几个场景 | 手工配置量 |
|---|---|---|---|
| **A - 冒烟测试** | 验证 Pawn-Controller 链 + 挥锤击碎结构 | 1(空) | 几乎零 |
| **B - 单场景玩法测试** | 正式预制体 + 房间 + 升级选项 | 1(InGame) | 中 |
| **C - 完整流程** | Boot → Lobby → InGame → 存档 | 3 | 高 |

**先做 A,确认打击-修复循环能跑,再往下走。**

---

## 档位 A:冒烟测试(5 分钟)

### 1. 打开项目
Unity Hub → Add project → 选 `GuJianRoguelike` 目录。
首次打开会自动拉包(InputSystem / URP / VectorGraphics / Cinemachine),等 ~2 min。

### 2. 激活新输入系统
`Edit → Project Settings → Player → Other Settings → Active Input Handling` 设为 **Input System Package (New)** 或 **Both**。
Unity 会重启一次。

### 3. 创建场景
`File → New Scene` → 选 **Basic (URP)** 模板 → 保存为 `Assets/Scenes/SmokeTest.unity`。
把场景里默认的 Main Camera、Directional Light **全删掉**(我们让脚本自己建,干净)。

### 4. 挂测试脚本
- 右键 Hierarchy → Create Empty → 命名 `Bootstrap`
- 在 Inspector 点 `Add Component` → 搜 **QuickTestBootstrap** → 添加
- 默认参数就够用,想改可以调:
  - `Enemy Count` = 敌人数(环形分布)
  - `Enemy Distance` = 半径
  - `Enemy Brokenness` = 靶子血量
  - `Enemy Material` = 木/石/铜/玉/漆(影响亲和伤害测试)
  - `Show Debug Hud` = 打开屏上 HUD

### 5. Play
按键:

| 键 | 动作 |
|---|---|
| WASD | 移动 |
| 鼠标 | 朝向(用移动方向也行) |
| **J / 鼠标左键** | 普攻(挥锤) |
| K / 鼠标右键 | 重击(击退强) |
| **L 按住/松开** | 蓄力(松开时根据蓄力时间放大伤害/范围) |
| Space | 闪避 |
| E | 交互 |
| Q | UseTool |
| `[` / `]` | 切工具槽 |
| R | 再刷一只靶子 |

### 6. 你应该看到
- 左上:HP / 体力 / 手艺等级 / 最近靶子残缺度
- 敌人(方块)**每挨一下就变亮一点**(修复过程视觉化)
- 残缺度归 0 → 方块消失 + 日志出现 `REPAIRED` + `+手艺`
- 手艺累积到阈值 → 日志出现 `LEVEL UP!` 事件
- 右下:事件日志(HIT/CRIT/REPAIRED/LEVEL UP)滚动刷新

### 7. 如果没看到预期
| 现象 | 原因 |
|---|---|
| 屏幕全黑 | 场景模板不是 URP,改用 Built-in 也行,但地面材质会 fallback 到 Standard(脚本已兼容) |
| 按 J 没反应 | 第 2 步没切 New Input System;或锤子没装配(查 Console 有无错) |
| 人走不动 | 玩家下面有个 `CharacterController`,被另外一个 Collider 阻挡了(删掉多余) |
| 敌人一直没死 | `Enemy Brokenness` 太高;或 SwingResolver 的 OverlapSphere 层没检测到——默认所有层都检测,应该 OK |

---

## 档位 B:单场景玩法测试(~30 分钟)

目的:把`StructureData`, `ToolData`, `RoomData` 等 ScriptableObject 资产、`StructureEnemyBase` 预制体、房间预制体接好,体验真正的「修房子 Roguelike」。

### 1. 场景骨架
新建 `Assets/Scenes/InGame.unity`(Basic URP),空场景。

### 2. 建玩家预制体

1. Hierarchy 新建 GameObject `Player`,tag 设 `Player`
2. Add Component:
   - `CharacterController` (Height 1.8 / Radius 0.45 / Center Y=0.9)
   - `PawnMovement`(自动带 CharacterController)
   - `PawnToolSlot`
     - Tool Anchor:拖一个子 empty 进去,位置比如 (0.35, 1.1, 0.4)
   - `PawnCombat`
   - `PawnHealth`
   - `PlayerPawn`(GuJian.Pawns)
3. 子物体加视觉(随便 Capsule 即可,之后换 SVG 动画)
4. 拖进 `Assets/Prefabs/` → 成为 `Player.prefab`,从场景删除

### 3. 建 InputActionAsset

1. Project 窗口右键 `Assets/Input/` → Create → Input Actions → `GuJianInput.inputactions`
2. 双击打开,Action Map = `Gameplay`
3. Actions(全部用下面这些名字,否则要自己改绑定):

   | Action | Type | Binding |
   |---|---|---|
   | Move | Value / Vector2 | 2D Vector:WASD |
   | Look | Value / Vector2 | `<Mouse>/delta` |
   | Attack | Button | `<Mouse>/leftButton` + `<Keyboard>/j` |
   | Heavy | Button | `<Mouse>/rightButton` + `<Keyboard>/k` |
   | Charge | Button | `<Keyboard>/l` |
   | Dodge | Button | `<Keyboard>/space` |
   | Interact | Button | `<Keyboard>/e` |
   | UseTool | Button | `<Keyboard>/q` |
   | ToolNext | Button | `<Keyboard>/rightBracket` |
   | ToolPrev | Button | `<Keyboard>/leftBracket` |

   点 Save Asset。

### 4. 建 PlayerController GameObject

场景里加空物体 `PlayerController`:
- `PlayerPawnController`
  - Input Asset:拖 `GuJianInput.inputactions`
  - Bindings:**和上面 Action 名字一一对应**(Action Map = `Gameplay`)
  - Auto Possess:拖场景里的 `Player` 实例(如果场景里有摆好的话)

场景里拖入 `Player.prefab` 一份,Auto Possess 指向它。

### 5. 建锤子

1. Project 右键 → Create → GuJian → Tool → ToolData → `Hammer_Wood.asset`
2. Inspector 填:
   - Tool Id = `hammer_wood`
   - Display Name = 木槌
   - Base Damage = 18
   - Swing Cooldown = 0.5
   - Range = 2.0
   - Swing Arc Deg = 110
   - Supports Charge = ✓
3. 建 Hammer 预制体:空物体挂 `HammerTool` → `data` 拖 `Hammer_Wood.asset` → 子物体加视觉(Cube / SVG)→ 拖进 `Assets/Prefabs/Hammer_Wood.prefab`
4. `ToolData.runtimePrefab` 字段拖 `Hammer_Wood.prefab`
5. 运行时由 `PlayerLoadout` 根据存档 `equippedToolId` 实例化(下一步)

### 6. 建 ToolCatalog + 装备

1. Create → GuJian → Lobby → ToolCatalog → `ToolCatalog.asset`
2. Tools 列表加入 `Hammer_Wood.asset`
3. `Player.prefab` 上加 `PlayerLoadout` 组件,`catalog` 拖 ToolCatalog
4. 首次运行会读存档;如果存档没有 `equippedToolId`,改 `MetaProgressSave.Load()` 默认 equippedToolId 为 `"hammer_wood"`(代码里已经有 `equippedToolId` 字段,默认是空字符串——改成 `hammer_wood` 就能直接跑)

### 7. 建一个结构敌人

1. Create → GuJian → Structure → StructureData → `Structure_DouGong_Wood.asset`
2. 填:Type=DouGong, Material=Wood, MaxBrokenness=40
3. RepairStages 数组:0~3 张 Sprite(从最破到完好)——没有也没关系,`UpdateStageSprite` 会自己跳过
4. 建 `StructureEnemyBase.prefab`:空物体挂 `CharacterController` + `PawnMovement` + `StructureEnemyPawn`(`data` 拖 DouGong SO)
5. 放 `Assets/Resources/Prefabs/StructureEnemyBase.prefab`(注意:`WaveDirector` 用 `Resources.Load` 加载这个路径)

### 8. 建一个房间

1. Create → GuJian → Rooms → RoomData → `Room_OuterYard_1.asset`
2. Waves 数组:加一波,Entries 里放 `structure = Structure_DouGong_Wood` / count = 2
3. Room 预制体:Plane 做地面 + 若干 Empty 做 `spawnPoints` → 挂 `WaveDirector`(拖 RoomData + spawnPoints)
4. 拖成 prefab,`RoomData.prefab` 字段回填
5. Create → GuJian → Rooms → RoomSet → 在 `outerYardRooms` 列表加刚才的 RoomData

### 9. 把房间接起来

场景里加空 `World` → 挂 `RoomManager`:
- Set:拖 RoomSet
- Spawn Root:把 World 自己拖进去
- Sequence:默认 `OuterYard, Corridor, GreatHall, SideWing, Event, GreatHall, Boss` 够用

再空物体 `GameMode` → 挂 `InGameGameMode`:
- Room Manager → World
- 还可以挂 `CraftsmanshipWallet` + `LevelUpSystem`(同一物体或单独物体)

### 10. Play
- 进场景 → WaveDirector Start → 房间里刷怪
- 清完 → 发 `RoomClearedEvent` → InGameGameMode.Next() → RoomManager 换下一间
- 跑到 Boss 关时 `RoomClearedEvent` 推出 → RunFinishedEvent → 写存档

---

## 档位 C:完整三场景流程

三个场景都要加进 Build Settings (`File → Build Settings`) 顺序:
```
0  Boot
1  Lobby
2  InGame
```

### Boot.unity
- 1 个空物体 `GameBootstrap`,挂 `GameBootstrap`(Lobby Scene / Ingame Scene 名字和上面对应)
- 无 UI,只是跳转
- 进入游戏时永远从这个场景启动

### Lobby.unity
- 空物体 `LobbyRoot`
- `GameBootstrap` 会自动 `AddComponent<LobbyGameMode>` 到这个场景
- 建 UI(Canvas):显示 `MetaProgressSave.matterSilver`, `bestRunStructures`, `totalRuns`
- `StartRun` 按钮 → `GameBootstrap.Instance.StartRun()`
- `ToolShop` GO:挂 `ToolShop` 脚本 + 引用 `ToolCatalog`

### InGame.unity
- 同档位 B 的场景
- `GameBootstrap` 自动 `AddComponent<InGameGameMode>`,由它驱动 `RoomManager`

---

## 扩展提示(你当前想加新玩法时)

| 想加 | 新建 | 无需改 |
|---|---|---|
| 新工具类型(凿/刨) | `ChiselTool : ToolBase` + `ToolData` SO | PawnCombat / PawnToolSlot / SwingResolver |
| 新敌人形态 | `StructureData` SO + 资源 | StructureEnemyPawn / SwingResolver |
| 新配件 | `ToolTuning` 子类 + SO | ToolBase / HammerTool |
| 新升级 | `UpgradeOption` SO | LevelUpSystem / IUpgradeEffect |
| 新房间 | `RoomData` SO + 预制体 + 加入 `RoomSet` | RoomManager / WaveDirector |
| 新 AI 行为 | `IAIBrain` 实现 | AIPawnController / 全部 Pawn 代码 |
| 新输入设备 | 新 `IPawnInputSource` 实现 | PlayerPawnController 只换 Input Source 类型 |

---

## 常见坑

1. **挥锤打空**:SwingResolver 用物理 OverlapSphere,敌人必须有 Collider。CharacterController 是 Collider,但如果你把敌人视觉换成无 Collider 的 Sprite,要记得挂个 Sphere/CapsuleCollider。
2. **锤子一直打不到**:`toolAnchor` 没设 → ToolBase `ctx.Origin` 会用玩家脚底 → 挥锤范围偏了。务必在 `PawnToolSlot.toolAnchor` 指定一个挂点。
3. **Input 失效**:`Active Input Handling` 没设成 New。
4. **结构"死"了也不算击杀**:`StructureEnemyPawn.ApplyHit` 会自动发 `StructureRepairedEvent`——确保 `InGameGameMode.OnRepaired` 有订阅(它 Awake 就订阅了)。
5. **运行后再把 SerializeField 拖空**:Controller 的 inputAsset 拖空了 Awake 会 LogError;只需要在 Inspector 拖回去。

---

## 如果只想最快看到"敲结构"
走档位 A。一个脚本搞定,不需要任何预制体,不需要 .inputactions,不需要 SO。
