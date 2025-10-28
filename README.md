# Unity 抽奖系统 (PrizeDraw)

一个基于Unity开发的可视化抽奖系统，采用模块化事件驱动架构，支持多等级奖项抽取、黑名单过滤和必中榜单等功能。

## 📺 项目效果展示

> 📽️ 观看完整演示视频：[DemoShow.mp4](DemoShow.mp4)

## 🏗️ 项目架构

### 系统概览

本项目采用**模块化事件驱动架构**（Event-Driven Architecture），所有系统通过 `SystemManager` 统一管理，系统间通过 `MEventSystem` 进行解耦通信。

```
SystemManager (系统管理器)
├── MConfigSystem (配置系统)
├── MEventSystem (事件系统)
├── MGameLogicSystem (游戏逻辑系统)
├── MInputSystem (输入系统)
└── MUISystem (UI系统)
```

### 核心系统说明

#### 1. **SystemManager** - 系统管理器
- **职责**：管理所有系统的生命周期（注册、初始化、清理）
- **位置**：`Assets/Scripts/Framework/Core/SystemManager.cs`
- **特点**：
  - 单例模式
  - 按顺序初始化系统
  - 提供全局访问点

#### 2. **MEventSystem** - 事件系统
- **职责**：提供类型安全的事件发布订阅机制
- **位置**：`Assets/Scripts/Framework/Events/MEventSystem.cs`
- **特点**：
  - 基于 `EventId` 枚举的事件标识
  - 支持泛型事件参数
  - 支持立即执行和队列执行
  - 防止重复订阅
  - 完善的错误处理

#### 3. **MConfigSystem** - 配置系统
- **职责**：加载和管理配置数据、黑名单、必中榜单等
- **位置**：`Assets/Scripts/Systems/MConfigSystem.cs`
- **数据文件**：
  - `config.json` - 抽奖配置（人员范围、特别奖配置等）
  - `blacklist.txt` - 黑名单
  - `mustwinlist1.txt` ~ `mustwinlist3.txt` - 必中榜单

#### 4. **MGameLogicSystem** - 游戏逻辑系统
- **职责**：管理游戏状态和抽奖逻辑
- **位置**：`Assets/Scripts/Systems/MGameLogicSystem.cs`
- **功能**：
  - 状态机管理（待抽奖、抽奖中、显示结果、过渡）
  - 普通奖项抽奖（支持黑名单和必中榜单）
  - 特别奖项抽奖（排除包含"4"的号码）
  - 中奖历史记录

#### 5. **MInputSystem** - 输入系统
- **职责**：处理键盘输入并转换为事件
- **位置**：`Assets/Scripts/Systems/MInputSystem.cs`
- **支持操作**：
  - 切换奖项等级（1/2/3/4键）
  - 抽奖（回车键）
  - 重新加载配置（C键）
  - 重启（R键）
  - 切换必中榜单（A/S/D/F键）

#### 6. **MUISystem** - UI系统
- **职责**：监听游戏状态变化并更新UI显示
- **位置**：`Assets/Scripts/Systems/MUISystem.cs`
- **功能**：
  - 视频播放控制（待机、抽奖、过场动画）
  - 奖项特效管理
  - 结果展示（支持不同奖项的自定义样式）
  - 动画事件回调

### 事件流转示意图

```
用户按键 → MInputSystem 发布事件
    ↓
MGameLogicSystem 接收处理 → 更新状态 → 发布状态事件
    ↓
MUISystem 接收状态事件 → 更新UI显示 → 播放动画
    ↓
动画完成 → MUISystem 发布完成事件
    ↓
MGameLogicSystem 接收 → 进入下一状态
```

### 数据流向

```
config.json ──┐
blacklist.txt ├──> MConfigSystem ──> MGameLogicSystem
mustwinlist*.txt ┘     (配置数据)        (抽奖逻辑)
```

### 设计模式

- **单例模式**：SystemManager
- **观察者模式**：事件系统（发布订阅）
- **状态模式**：游戏状态机
- **策略模式**：普通奖与特别奖抽奖逻辑

## 📖 使用说明

### 环境要求

- **Unity版本**：Unity 2022.3.62f1c1
- **依赖插件**：
  - TextMesh Pro
  - Odin Inspector（可选，用于Inspector增强）

### 快速开始

1. **打开项目**
   ```
   使用Unity Hub打开 PrizeDraw 项目文件夹
   ```

2. **场景加载**
   ```
   打开场景：Assets/Scenes/MainScene.unity
   ```

3. **配置设置**
   
   编辑 `Assets/config.json` 配置文件：
   ```json
   {
       "commonMinPeopleIndex": 52101,    // 普通奖最小号码
       "commonMaxPeopleIndex": 52201,    // 普通奖最大号码
       "specialPrizeIndex": 2,            // 特别奖等级（1-4）
       "specialMinPeopleIndex": 1,        // 特别奖最小号码
       "specialMaxPeopleIndex": 100       // 特别奖最大号码
   }
   ```

4. **黑名单设置**
   
   编辑 `Assets/blacklist.txt`（每行一个号码）：
   ```
   52101
   52102
   ```

5. **必中榜单设置**
   
   编辑 `Assets/mustwinlist1.txt` ~ `mustwinlist3.txt`（每行一个号码）

### 操作指南

#### 键盘控制

| 按键 | 功能 | 说明 |
|------|------|------|
| **1/2/3/4** | 切换奖项 | 切换到对应等级的奖项 |
| **回车** | 开始抽奖 | 在待抽奖状态下触发抽奖 |
| **R** | 重启 | 在显示结果状态下重启到待抽奖状态 |
| **C** | 重载配置 | 热重载配置文件（无需重启Unity） |
| **A/S/D/F** | 切换必中榜单 | 切换到对应索引的必中榜单（1/2/3），0表示不使用必中榜单 |

#### 操作流程

1. **启动游戏**：点击Unity编辑器的播放按钮
2. **选择奖项**：按 1/2/3/4 选择要抽取的奖项等级
3. **开始抽奖**：按回车键开始抽奖
4. **查看结果**：等待抽奖动画完成，显示中奖号码
5. **继续抽奖**：按 R 键返回待抽奖状态，重复步骤2-4

### UI配置

在Unity编辑器中配置UI：

1. 找到场景中的 `UIReferences` 组件
2. 在Inspector中设置以下内容：
   - **UI Config**：链接到 `Assets/Data/UIConfig.asset`
   - **场景UI引用**：拖拽场景中的UI元素到对应字段
   - **视频剪辑**：设置待机、抽奖、过场动画视频
   - **奖项效果**：配置不同奖项的背景、颜色、特效等

### 高级功能

#### 必中榜单

系统支持多个必中榜单（默认3个）：
- 榜单文件：`mustwinlist1.txt` ~ `mustwinlist3.txt`（默认不使用）
- 切换榜单：按 A/S/D/F 键切换
- 抽奖逻辑：优先从必中榜单中抽取（如果榜单中有有效号码）

#### 黑名单过滤

- 在 `blacklist.txt` 中添加的号码将不会被抽中
- 支持热重载（按C键）

#### 特别奖配置

- 特别奖可以配置独立的号码范围
- 自动排除包含"4"的号码
- 在 `config.json` 中通过 `specialPrizeIndex` 指定哪个等级是特别奖

#### 中奖历史管理

- 系统自动记录已中奖号码，防止重复中奖
- 使用Odin Inspector在运行时清除历史（点击 "Clear Draw History" 按钮）

## 🔧 开发说明

### 添加新功能

1. **添加新事件**：在 `EventId.cs` 中添加事件ID
2. **创建事件参数**：在 `EventArgs.cs` 中定义事件参数类
3. **发布事件**：在需要的系统中调用 `eventSystem.Publish()`
4. **订阅事件**：在目标系统中调用 `eventSystem.Subscribe()`

### 代码规范

- 系统类以 `M` 开头（Module）
- 事件ID遵循 `发送者_接收者_动作` 命名规范
- 所有系统实现 `ISystem` 接口
- 使用详细的XML注释

## 📦 项目结构

```
Assets/
├── Scripts/
│   ├── Framework/          # 框架核心
│   │   ├── Core/          # 核心接口和系统管理器
│   │   └── Events/        # 事件系统
│   ├── Systems/           # 各功能系统
│   ├── Config/            # 配置相关
│   └── Common/            # 通用类型定义
├── Scenes/                # 场景文件
├── Resources/             # 资源文件（UI素材、视频等）
├── Data/                  # 配置数据资产
├── config.json           # 抽奖配置
├── blacklist.txt         # 黑名单
└── mustwinlist*.txt      # 必中榜单
```

