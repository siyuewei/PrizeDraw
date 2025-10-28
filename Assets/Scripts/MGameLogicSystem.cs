using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;

/// <summary>
/// 游戏逻辑系统 - 负责游戏状态管理和抽奖逻辑
/// </summary>
public class MGameLogicSystem : MonoBehaviour, ISystem
{
    #region 公开属性
    [ShowInInspector, ReadOnly]
    public GameState CurrentState { get; private set; } = GameState.Idle;
    
    public int CurrentPrizeIndex { get; private set; } = 1;
    public int LastWinnerID { get; private set; } = 0;
    #endregion
    
    #region 私有变量
    private int mustListIndex = 0;
    private MConfigSystem _mConfigSystem;
    private MEventSystem eventSystem;
    #endregion
    
    #region Odin按钮
    [Button("Prize Draw")]
    void PrizeDrawButton()
    {
        ExecutePrizeDraw();
    }

    [Button("Clear Draw History")]
    void ClearDrawHistoryButton()
    {
        ClearDrawHistory();
    }
    #endregion
    
    #region 系统接口实现
    public void Initialize()
    {
        Debug.Log("[GameLogicSystem] 初始化游戏逻辑系统");
        
        // 获取系统引用
        _mConfigSystem = SystemManager.Instance.MConfig;
        eventSystem = SystemManager.Instance.Events;
        
        // 设置随机数种子
        Random.InitState(System.DateTime.Now.Millisecond);
        
        // 订阅事件
        SubscribeToEvents();
        
        // 初始化到待机状态
        ChangeState(GameState.Idle);
        
        Debug.Log("[GameLogicSystem] 游戏逻辑系统初始化完成");
    }
    
    public void Cleanup()
    {
        Debug.Log("[GameLogicSystem] 清理游戏逻辑系统");
        UnsubscribeFromEvents();
    }
    #endregion
    
    #region 事件订阅管理
    private void SubscribeToEvents()
    {
        eventSystem.OnPrizeDrawRequested += HandlePrizeDrawRequested;
        eventSystem.OnPrizeIndexChangeRequested += HandlePrizeIndexChangeRequested;
        eventSystem.OnReloadConfigRequested += HandleReloadConfigRequested;
        eventSystem.OnRestartRequested += HandleRestartRequested;
        eventSystem.OnReadyToShowResult += HandleReadyToShowResult;
        eventSystem.OnTransitionComplete += HandleTransitionComplete;
        eventSystem.OnChangeMustListIndex += HandleChangeMustListIndex;
    }
    
    private void UnsubscribeFromEvents()
    {
        eventSystem.OnPrizeDrawRequested -= HandlePrizeDrawRequested;
        eventSystem.OnPrizeIndexChangeRequested -= HandlePrizeIndexChangeRequested;
        eventSystem.OnReloadConfigRequested -= HandleReloadConfigRequested;
        eventSystem.OnRestartRequested -= HandleRestartRequested;
        eventSystem.OnReadyToShowResult -= HandleReadyToShowResult;
        eventSystem.OnTransitionComplete -= HandleTransitionComplete;
        eventSystem.OnChangeMustListIndex -= HandleChangeMustListIndex;
    }
    #endregion
    
    #region 事件处理方法
    private void HandlePrizeDrawRequested(int prizeIndex)
    {
        if (CurrentState != GameState.Idle)
        {
            Debug.LogWarning($"[GameLogicSystem] 当前状态为 {CurrentState}，无法进行抽奖");
            return;
        }
        
        ExecutePrizeDraw();
        ChangeState(GameState.Drawing);
    }
    
    private void HandlePrizeIndexChangeRequested(int prizeIndex)
    {
        if (CurrentState != GameState.Idle)
        {
            Debug.LogWarning($"[GameLogicSystem] 当前状态为 {CurrentState}，无法切换奖项");
            return;
        }
        
        if (prizeIndex < 1 || prizeIndex > 4)
        {
            Debug.LogWarning($"[GameLogicSystem] 无效的奖项索引: {prizeIndex}");
            return;
        }
        
        CurrentPrizeIndex = prizeIndex;
        Debug.Log($"[GameLogicSystem] 奖项已切换到: {prizeIndex}等奖");
        
        eventSystem.NotifyPrizeIndexUpdated(prizeIndex);
    }
    
    private void HandleReloadConfigRequested()
    {
        if (CurrentState != GameState.Idle)
        {
            Debug.LogWarning($"[GameLogicSystem] 当前状态为 {CurrentState}，无法重新加载配置");
            return;
        }
        
        _mConfigSystem.LoadAllData();
        Debug.Log("[GameLogicSystem] 重新加载配置完成");
    }
    
    private void HandleRestartRequested()
    {
        if (CurrentState != GameState.ShowingResult)
        {
            Debug.LogWarning($"[GameLogicSystem] 当前状态为 {CurrentState}，无法重启");
            return;
        }
        
        ChangeState(GameState.Transitioning);
    }
    
    private void HandleReadyToShowResult()
    {
        if (CurrentState != GameState.Drawing)
        {
            return;
        }
        
        ChangeState(GameState.ShowingResult);
    }
    
    private void HandleTransitionComplete()
    {
        if (CurrentState != GameState.Transitioning)
        {
            return;
        }
        
        ChangeState(GameState.Idle);
    }
    
    private void HandleChangeMustListIndex(int mustListIndex)
    {
        this.mustListIndex = mustListIndex;
        Debug.Log($"[GameLogicSystem] 必中榜单索引已切换到: {this.mustListIndex}");
        eventSystem.NotifyMustListIndexChanged(mustListIndex);
    }
    #endregion
    
    #region 状态管理
    private void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }
        
        Debug.Log($"[GameLogicSystem] 状态切换: {CurrentState} -> {newState}");
        CurrentState = newState;
        
        eventSystem.NotifyStateChanged(newState);
    }
    #endregion
    
    #region 抽奖逻辑
    private void ExecutePrizeDraw()
    {
        bool isSpecialPrize = (CurrentPrizeIndex == _mConfigSystem.Config.specialPrizeIndex);

        if (isSpecialPrize)
        {
            DrawSpecialPrize();
        }
        else
        {
            DrawCommonPrize();
        }
    }
    
    /// <summary>
    /// 特别奖抽奖逻辑
    /// </summary>
    private void DrawSpecialPrize()
    {
        var config = _mConfigSystem.Config;
        var specialWinners = _mConfigSystem.SpecialWinnerIndices;
        
        int specialTotalPeople = config.specialMaxPeopleIndex - config.specialMinPeopleIndex + 1;
        int specialAvailableCount = specialTotalPeople - specialWinners.Count;

        if (specialAvailableCount <= 0)
        {
            Debug.LogWarning("[GameLogicSystem] 所有特别奖人员都已被抽中，无法继续抽奖。");
            return;
        }

        int drawnIndex;
        int attemptCount = 0;
        const int maxAttempts = 1000;

        do
        {
            attemptCount++;
            if (attemptCount > maxAttempts)
            {
                Debug.LogError("[GameLogicSystem] 特别奖抽奖尝试次数过多，已停止抽奖。");
                return;
            }

            drawnIndex = Random.Range(config.specialMinPeopleIndex, config.specialMaxPeopleIndex + 1);
        } while (specialWinners.Contains(drawnIndex) || drawnIndex.ToString().Contains("4"));
        
        LastWinnerID = drawnIndex;
        _mConfigSystem.AddWinner(CurrentPrizeIndex, LastWinnerID);
        
        Debug.Log($"[GameLogicSystem] 【特别奖】第 {CurrentPrizeIndex} 等奖，中奖号码：{LastWinnerID}。特别奖已中奖人数：{specialWinners.Count}");
    }
    
    /// <summary>
    /// 普通奖抽奖逻辑
    /// </summary>
    private void DrawCommonPrize()
    {
        var config = _mConfigSystem.Config;
        var commonWinners = _mConfigSystem.CommonWinnerIndices;
        var blackList = _mConfigSystem.BlackList;
        
        int commonAvailableCount = _mConfigSystem.GetCommonAvailablePeopleCount();
        
        if (commonAvailableCount <= 0)
        {
            Debug.LogWarning("[GameLogicSystem] 没有可用人数，无法进行抽奖。");
            return;
        }

        int totalPossibleDraws = commonAvailableCount - commonWinners.Count;

        if (totalPossibleDraws <= 0)
        {
            Debug.LogWarning("[GameLogicSystem] 所有符合条件的人员都已被抽中，无法继续抽奖。");
            return;
        }

        // 读取必中榜单
        List<int> mustWinList = _mConfigSystem.ReadMustWinList(mustListIndex);
        
        int drawnIndex = -1;
        bool drawnFromMustWinList = false;
        
        // 先尝试从必中榜单中抽取
        if (mustWinList.Count > 0)
        {
            List<int> validMustWinList = new List<int>();
            foreach (int mustWinNumber in mustWinList)
            {
                if (mustWinNumber >= config.commonMinPeopleIndex && 
                    mustWinNumber <= config.commonMaxPeopleIndex &&
                    !commonWinners.Contains(mustWinNumber) &&
                    !blackList.Contains(mustWinNumber))
                {
                    validMustWinList.Add(mustWinNumber);
                }
            }
            
            if (validMustWinList.Count > 0)
            {
                int randomIndex = Random.Range(0, validMustWinList.Count);
                drawnIndex = validMustWinList[randomIndex];
                drawnFromMustWinList = true;
                Debug.Log($"[GameLogicSystem] 从必中榜单中抽取，可选人数：{validMustWinList.Count}");
            }
            else
            {
                Debug.Log("[GameLogicSystem] 必中榜单中没有符合条件的人员，将进行正常抽奖。");
            }
        }
        
        // 正常抽奖
        if (drawnIndex == -1)
        {
            int attemptCount = 0; 
            const int maxAttempts = 10000;

            do
            {
                drawnIndex = Random.Range(config.commonMinPeopleIndex, config.commonMaxPeopleIndex + 1);
                attemptCount++;

                if (attemptCount > maxAttempts)
                {
                    Debug.LogError("[GameLogicSystem] 抽奖尝试次数过多，已停止抽奖。");
                    return;
                }
                
            } while (commonWinners.Contains(drawnIndex) || blackList.Contains(drawnIndex));
        }

        LastWinnerID = drawnIndex;
        _mConfigSystem.AddWinner(CurrentPrizeIndex, LastWinnerID);
        
        string drawSource = drawnFromMustWinList ? "【必中榜单】" : "【正常抽奖】";
        Debug.Log($"[GameLogicSystem] {drawSource}第 {CurrentPrizeIndex} 等奖，中奖号码：{LastWinnerID}。普通奖已中奖人数：{commonWinners.Count}");
    }
    
    /// <summary>
    /// 清除抽奖历史
    /// </summary>
    public void ClearDrawHistory()
    {
        if (CurrentState != GameState.Idle)
        {
            Debug.LogWarning($"[GameLogicSystem] 当前状态为 {CurrentState}，无法清除抽奖历史");
            return;
        }

        _mConfigSystem.ClearDrawHistory();
    }
    #endregion
}

