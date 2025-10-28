using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;

/// <summary>
/// 游戏逻辑系统 - 负责游戏状态管理和抽奖逻辑
/// 
/// 订阅的事件：
/// - EventId.PrizeDrawRequested - 请求抽奖
/// - EventId.PrizeIndexChangeRequested - 请求切换奖项
/// - EventId.ReloadConfigRequested - 请求重新加载配置
/// - EventId.RestartRequested - 请求重启
/// - EventId.ReadyToShowResult - 准备显示结果
/// - EventId.TransitionComplete - 过渡完成
/// - EventId.ChangeMustListIndex - 切换必中榜单
/// 
/// 发布的事件：
/// - EventId.GameStateChanged - 游戏状态改变
/// - EventId.PrizeIndexUpdated - 奖项已更新
/// - EventId.MustListIndexChanged - 必中榜单索引已改变
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
    private MConfigSystem configSystem;
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
        Debug.Log("[MGameLogicSystem] 初始化游戏逻辑系统");
        
        configSystem = SystemManager.Instance.MConfig;
        eventSystem = SystemManager.Instance.Events;
        
        Random.InitState(System.DateTime.Now.Millisecond);
        
        SubscribeToEvents();
        ChangeState(GameState.Idle);
        
        Debug.Log("[MGameLogicSystem] 游戏逻辑系统初始化完成");
    }
    
    public void Cleanup()
    {
        Debug.Log("[MGameLogicSystem] 清理游戏逻辑系统");
        UnsubscribeFromEvents();
    }
    #endregion
    
    #region 事件订阅管理
    private void SubscribeToEvents()
    {
        // 搜索 "EventId.PrizeDrawRequested" 可以找到所有发布此事件的位置
        eventSystem.Subscribe<IntEventArg>(EventId.PrizeDrawRequested, HandlePrizeDrawRequested);
        eventSystem.Subscribe<IntEventArg>(EventId.PrizeIndexChangeRequested, HandlePrizeIndexChangeRequested);
        eventSystem.Subscribe<EmptyEventArg>(EventId.ReloadConfigRequested, HandleReloadConfigRequested);
        eventSystem.Subscribe<EmptyEventArg>(EventId.RestartRequested, HandleRestartRequested);
        eventSystem.Subscribe<EmptyEventArg>(EventId.ReadyToShowResult, HandleReadyToShowResult);
        eventSystem.Subscribe<EmptyEventArg>(EventId.TransitionComplete, HandleTransitionComplete);
        eventSystem.Subscribe<IntEventArg>(EventId.ChangeMustListIndex, HandleChangeMustListIndex);
    }
    
    private void UnsubscribeFromEvents()
    {
        eventSystem.Unsubscribe<IntEventArg>(EventId.PrizeDrawRequested, HandlePrizeDrawRequested);
        eventSystem.Unsubscribe<IntEventArg>(EventId.PrizeIndexChangeRequested, HandlePrizeIndexChangeRequested);
        eventSystem.Unsubscribe<EmptyEventArg>(EventId.ReloadConfigRequested, HandleReloadConfigRequested);
        eventSystem.Unsubscribe<EmptyEventArg>(EventId.RestartRequested, HandleRestartRequested);
        eventSystem.Unsubscribe<EmptyEventArg>(EventId.ReadyToShowResult, HandleReadyToShowResult);
        eventSystem.Unsubscribe<EmptyEventArg>(EventId.TransitionComplete, HandleTransitionComplete);
        eventSystem.Unsubscribe<IntEventArg>(EventId.ChangeMustListIndex, HandleChangeMustListIndex);
    }
    #endregion
    
    #region 事件处理方法
    private void HandlePrizeDrawRequested(IntEventArg arg)
    {
        if (CurrentState != GameState.Idle)
        {
            Debug.LogWarning($"[MGameLogicSystem] 当前状态为 {CurrentState}，无法进行抽奖");
            return;
        }
        
        ExecutePrizeDraw();
        ChangeState(GameState.Drawing);
    }
    
    private void HandlePrizeIndexChangeRequested(IntEventArg arg)
    {
        int prizeIndex = arg.value;
        
        if (CurrentState != GameState.Idle)
        {
            Debug.LogWarning($"[MGameLogicSystem] 当前状态为 {CurrentState}，无法切换奖项");
            return;
        }
        
        if (prizeIndex < 1 || prizeIndex > 4)
        {
            Debug.LogWarning($"[MGameLogicSystem] 无效的奖项索引: {prizeIndex}");
            return;
        }
        
        CurrentPrizeIndex = prizeIndex;
        Debug.Log($"[MGameLogicSystem] 奖项已切换到: {prizeIndex}等奖");
        
        // 发布事件：奖项已更新
        eventSystem.Publish(EventId.PrizeIndexUpdated, new IntEventArg(prizeIndex));
    }
    
    private void HandleReloadConfigRequested(EmptyEventArg arg)
    {
        if (CurrentState != GameState.Idle)
        {
            Debug.LogWarning($"[MGameLogicSystem] 当前状态为 {CurrentState}，无法重新加载配置");
            return;
        }
        
        configSystem.LoadAllData();
        Debug.Log("[MGameLogicSystem] 重新加载配置完成");
    }
    
    private void HandleRestartRequested(EmptyEventArg arg)
    {
        if (CurrentState != GameState.ShowingResult)
        {
            Debug.LogWarning($"[MGameLogicSystem] 当前状态为 {CurrentState}，无法重启");
            return;
        }
        
        ChangeState(GameState.Transitioning);
    }
    
    private void HandleReadyToShowResult(EmptyEventArg arg)
    {
        if (CurrentState != GameState.Drawing)
        {
            return;
        }
        
        ChangeState(GameState.ShowingResult);
    }
    
    private void HandleTransitionComplete(EmptyEventArg arg)
    {
        if (CurrentState != GameState.Transitioning)
        {
            return;
        }
        
        ChangeState(GameState.Idle);
    }
    
    private void HandleChangeMustListIndex(IntEventArg arg)
    {
        this.mustListIndex = arg.value;
        Debug.Log($"[MGameLogicSystem] 必中榜单索引已切换到: {this.mustListIndex}");
        
        // 发布事件：必中榜单索引已改变
        eventSystem.Publish(EventId.MustListIndexChanged, new IntEventArg(this.mustListIndex));
    }
    #endregion
    
    #region 状态管理
    private void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }
        
        Debug.Log($"[MGameLogicSystem] 状态切换: {CurrentState} -> {newState}");
        CurrentState = newState;
        
        // 发布事件：游戏状态改变
        eventSystem.Publish(EventId.GameStateChanged, new GameStateEventArg(newState));
    }
    #endregion
    
    #region 抽奖逻辑
    private void ExecutePrizeDraw()
    {
        bool isSpecialPrize = (CurrentPrizeIndex == configSystem.Config.specialPrizeIndex);

        if (isSpecialPrize)
        {
            DrawSpecialPrize();
        }
        else
        {
            DrawCommonPrize();
        }
    }
    
    private void DrawSpecialPrize()
    {
        var config = configSystem.Config;
        var specialWinners = configSystem.SpecialWinnerIndices;
        
        int specialTotalPeople = config.specialMaxPeopleIndex - config.specialMinPeopleIndex + 1;
        int specialAvailableCount = specialTotalPeople - specialWinners.Count;

        if (specialAvailableCount <= 0)
        {
            Debug.LogWarning("[MGameLogicSystem] 所有特别奖人员都已被抽中");
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
                Debug.LogError("[MGameLogicSystem] 特别奖抽奖尝试次数过多");
                return;
            }

            drawnIndex = Random.Range(config.specialMinPeopleIndex, config.specialMaxPeopleIndex + 1);
        } while (specialWinners.Contains(drawnIndex) || drawnIndex.ToString().Contains("4"));
        
        LastWinnerID = drawnIndex;
        configSystem.AddWinner(CurrentPrizeIndex, LastWinnerID);
        
        Debug.Log($"[MGameLogicSystem] 【特别奖】第{CurrentPrizeIndex}等奖，中奖号码：{LastWinnerID}");
    }
    
    private void DrawCommonPrize()
    {
        var config = configSystem.Config;
        var commonWinners = configSystem.CommonWinnerIndices;
        var blackList = configSystem.BlackList;
        
        int commonAvailableCount = configSystem.GetCommonAvailablePeopleCount();
        
        if (commonAvailableCount <= 0)
        {
            Debug.LogWarning("[MGameLogicSystem] 没有可用人数");
            return;
        }

        int totalPossibleDraws = commonAvailableCount - commonWinners.Count;

        if (totalPossibleDraws <= 0)
        {
            Debug.LogWarning("[MGameLogicSystem] 所有人员都已被抽中");
            return;
        }

        List<int> mustWinList = configSystem.ReadMustWinList(mustListIndex);
        
        int drawnIndex = -1;
        bool drawnFromMustWinList = false;
        
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
                Debug.Log($"[MGameLogicSystem] 从必中榜单中抽取，可选人数：{validMustWinList.Count}");
            }
        }
        
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
                    Debug.LogError("[MGameLogicSystem] 抽奖尝试次数过多");
                    return;
                }
                
            } while (commonWinners.Contains(drawnIndex) || blackList.Contains(drawnIndex));
        }

        LastWinnerID = drawnIndex;
        configSystem.AddWinner(CurrentPrizeIndex, LastWinnerID);
        
        string drawSource = drawnFromMustWinList ? "【必中榜单】" : "【正常抽奖】";
        Debug.Log($"[MGameLogicSystem] {drawSource}第{CurrentPrizeIndex}等奖，中奖号码：{LastWinnerID}");
    }
    
    public void ClearDrawHistory()
    {
        if (CurrentState != GameState.Idle)
        {
            Debug.LogWarning($"[MGameLogicSystem] 当前状态为 {CurrentState}，无法清除历史");
            return;
        }

        configSystem.ClearDrawHistory();
    }
    #endregion
}
