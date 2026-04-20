# 古建修补匠 · GuJian Roguelike

**Unity 2022.3 LTS** · 俯视 2.5D 视差 · 古建筑修复主题轻肉鸽

基于 **Pawn-Controller** 架构，数据驱动，工具系统可拓展。

## 打开项目
1. 安装 Unity Hub，下载 Unity 2022.3.40f1（或同大版本 LTS）
2. Add Project → 选择本 `GuJianRoguelike/` 根目录
3. 首次打开会自动解析 `Packages/manifest.json`（含 Input System / Vector Graphics / URP）
4. 打开场景 `Assets/Scenes/Boot.unity`（若不存在，先在 Hierarchy 创建一个空 GO 挂 `GameBootstrap`）

## 目录对应
见 `Docs/Architecture.md`。

## 新增内容的自助手册
| 想做什么 | 需要创建 |
|---------|---------|
| 新锤子 | `ToolData.asset` + 一把 `HammerTool` 变体；放进 `MetaProgression.UnlockedTools` |
| 锤子配件 | `ToolTuning.asset`（继承 `ToolTuningBase`），在锤子数据里挂槽 |
| 新残缺结构 | `StructureData.asset`，挂到 `WaveDirector.SpawnTable` |
| 新升级项 | `UpgradeOption.asset`（实现 `IUpgradeEffect`），加入 `UpgradePool` |
| 新房间 | Room 预制体 + `RoomData.asset` → `RoomSet.asset` |
