/// <summary>
/// 事件ID枚举 - 定义所有游戏事件
/// 命名规范: 发送系统_接收系统_动作
/// 例如: Input_GameLogic_RequestDraw 表示 InputSystem 向 GameLogicSystem 发送抽奖请求
/// </summary>
public enum EventId
{
    // ========== InputSystem → GameLogicSystem ==========
    /// <summary>输入系统请求游戏逻辑系统执行抽奖</summary>
    Input_GameLogic_RequestDraw,
    
    /// <summary>输入系统请求游戏逻辑系统切换奖项</summary>
    Input_GameLogic_ChangePrizeIndex,
    
    /// <summary>输入系统请求游戏逻辑系统重新加载配置</summary>
    Input_GameLogic_ReloadConfig,
    
    /// <summary>输入系统请求游戏逻辑系统重启（进入下一轮）</summary>
    Input_GameLogic_Restart,
    
    /// <summary>输入系统请求游戏逻辑系统切换必中榜单索引</summary>
    Input_GameLogic_ChangeMustListIndex,
    
    // ========== GameLogicSystem → UISystem ==========
    /// <summary>游戏逻辑系统通知UI系统：游戏状态已改变</summary>
    GameLogic_UI_StateChanged,
    
    /// <summary>游戏逻辑系统通知UI系统：奖项索引已更新</summary>
    GameLogic_UI_PrizeIndexUpdated,
    
    /// <summary>游戏逻辑系统通知UI系统：必中榜单索引已改变</summary>
    GameLogic_UI_MustListIndexChanged,
    
    // ========== UISystem → GameLogicSystem ==========
    /// <summary>UI系统通知游戏逻辑系统：抽奖动画完成，准备显示结果</summary>
    UI_GameLogic_ReadyToShowResult,
    
    /// <summary>UI系统通知游戏逻辑系统：过渡动画完成</summary>
    UI_GameLogic_TransitionComplete,
}

