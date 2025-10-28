using System;
using UnityEngine;

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
/// 事件系统 - 负责管理所有游戏事件
/// </summary>
public class MEventSystem : MonoBehaviour, ISystem
{
    #region 状态相关事件
    public event Action<GameState> OnGameStateChanged;
    #endregion
    
    #region 输入请求事件
    public event Action<int> OnPrizeDrawRequested;
    public event Action<int> OnPrizeIndexChangeRequested;
    public event Action OnReloadConfigRequested;
    public event Action OnRestartRequested;
    public event Action<int> OnChangeMustListIndex;
    #endregion
    
    #region 逻辑层验证后的通知事件
    public event Action<int> OnPrizeIndexUpdated;
    public event Action<int> OnMustListIndexChanged;
    #endregion
    
    #region UI完成事件
    public event Action OnReadyToShowResult;
    public event Action OnTransitionComplete;
    #endregion
    
    #region 系统接口实现
    public void Initialize()
    {
        Debug.Log("[EventSystem] 初始化事件系统");
    }
    
    public void Cleanup()
    {
        Debug.Log("[EventSystem] 清理事件系统");
        
        // 清空所有事件订阅
        OnGameStateChanged = null;
        OnPrizeDrawRequested = null;
        OnPrizeIndexChangeRequested = null;
        OnReloadConfigRequested = null;
        OnRestartRequested = null;
        OnChangeMustListIndex = null;
        OnPrizeIndexUpdated = null;
        OnMustListIndexChanged = null;
        OnReadyToShowResult = null;
        OnTransitionComplete = null;
    }
    #endregion
    
    #region 状态变化通知
    public void NotifyStateChanged(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }
    #endregion
    
    #region 输入请求方法
    public void RequestPrizeDraw(int prizeIndex)
    {
        OnPrizeDrawRequested?.Invoke(prizeIndex);
    }
    
    public void RequestPrizeIndexChange(int prizeIndex)
    {
        OnPrizeIndexChangeRequested?.Invoke(prizeIndex);
    }
    
    public void RequestReloadConfig()
    {
        OnReloadConfigRequested?.Invoke();
    }
    
    public void RequestRestart()
    {
        OnRestartRequested?.Invoke();
    }
    
    public void ChangeMustListIndex(int mustListIndex)
    {
        OnChangeMustListIndex?.Invoke(mustListIndex);
    }
    #endregion
    
    #region 逻辑层通知方法
    public void NotifyPrizeIndexUpdated(int prizeIndex)
    {
        OnPrizeIndexUpdated?.Invoke(prizeIndex);
    }
    
    public void NotifyMustListIndexChanged(int mustListIndex)
    {
        OnMustListIndexChanged?.Invoke(mustListIndex);
    }
    #endregion
    
    #region UI完成通知方法
    public void NotifyReadyToShowResult()
    {
        OnReadyToShowResult?.Invoke();
    }
    
    public void NotifyTransitionComplete()
    {
        OnTransitionComplete?.Invoke();
    }
    #endregion
}

