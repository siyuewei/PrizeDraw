using System;

/// <summary>
/// 游戏事件系统定义
/// </summary>
public static class GameEvents
{
    // 抽奖相关事件
    public static event Action<int> OnPrizeDrawRequested;
    public static event Action<int> OnPrizeIndexChanged;
    public static event Action OnReloadConfigRequested;
    
    // 事件发送方法
    public static void RequestPrizeDraw(int prizeIndex)
    {
        OnPrizeDrawRequested?.Invoke(prizeIndex);
    }
    
    public static void ChangePrizeIndex(int prizeIndex)
    {
        OnPrizeIndexChanged?.Invoke(prizeIndex);
    }
    
    public static void RequestReloadConfig()
    {
        OnReloadConfigRequested?.Invoke();
    }
}
