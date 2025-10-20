using System;

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    Idle,           // 待机状态：等待选择奖项和抽奖
    Drawing,        // 抽奖中：播放抽奖动画
    ShowingResult,  // 显示结果：显示中奖结果，等待重启
    Transitioning   // 过渡中：播放过渡动画，准备回到待机
}

/// <summary>
/// 游戏事件系统定义
/// </summary>
public static class GameEvents
{
    // 状态相关事件
    public static event Action<GameState> OnGameStateChanged;
    
    // 输入请求事件（由InputHandler发送，GameLogicHandler处理）
    public static event Action<int> OnPrizeDrawRequested;
    public static event Action<int> OnPrizeIndexChangeRequested;  // 请求切换奖项
    public static event Action OnReloadConfigRequested;
    public static event Action OnRestartRequested;
    
    // 逻辑层验证后的通知事件（由GameLogicHandler发送，UI层监听）
    public static event Action<int> OnPrizeIndexUpdated;  // 奖项已更新（验证通过）
    
    // UI完成事件（由UIManager发送，GameLogicHandler处理）
    public static event Action OnReadyToShowResult; // 抽奖动画播放到设定百分比，准备显示结果
    public static event Action OnTransitionComplete;
    
    // 状态变化通知
    public static void NotifyStateChanged(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }
    
    // InputHandler调用的请求方法
    public static void RequestPrizeDraw(int prizeIndex)
    {
        OnPrizeDrawRequested?.Invoke(prizeIndex);
    }
    
    public static void RequestPrizeIndexChange(int prizeIndex)
    {
        OnPrizeIndexChangeRequested?.Invoke(prizeIndex);
    }
    
    public static void RequestReloadConfig()
    {
        OnReloadConfigRequested?.Invoke();
    }
    
    public static void RequestRestart()
    {
        OnRestartRequested?.Invoke();
    }
    
    // GameLogicHandler调用的通知方法
    public static void NotifyPrizeIndexUpdated(int prizeIndex)
    {
        OnPrizeIndexUpdated?.Invoke(prizeIndex);
    }
    
    // UIManager调用的完成通知方法
    public static void NotifyReadyToShowResult()
    {
        OnReadyToShowResult?.Invoke();
    }
    
    public static void NotifyTransitionComplete()
    {
        OnTransitionComplete?.Invoke();
    }
}
