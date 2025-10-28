using UnityEngine;

/// <summary>
/// 输入系统 - 负责处理键盘输入并发送相应事件
/// 
/// 发布的事件（Input → GameLogic）：
/// - Input_GameLogic_RequestDraw - 请求抽奖（按键1）
/// - Input_GameLogic_ChangePrizeIndex - 切换奖项（按键2/3/4/5）
/// - Input_GameLogic_ReloadConfig - 重新加载配置（按键C）
/// - Input_GameLogic_Restart - 重启（按键R）
/// - Input_GameLogic_ChangeMustListIndex - 切换必中榜单（按键A/S/D/F）
/// </summary>
public class MInputSystem : MonoBehaviour, ISystem
{
    #region 按键映射
    private readonly KeyCode keyCode_Prize1 = KeyCode.Alpha5; // 一等奖
    private readonly KeyCode keyCode_Prize2 = KeyCode.Alpha2; // 二等奖
    private readonly KeyCode keyCode_Prize3 = KeyCode.Alpha3; // 三等奖
    private readonly KeyCode keyCode_Prize4 = KeyCode.Alpha4; // 四等奖
    private readonly KeyCode keyCode_PrizeDraw = KeyCode.Alpha1; // 抽奖键
    private readonly KeyCode keyCode_Reload = KeyCode.C; // 重新加载配置
    private readonly KeyCode keyCode_Restart = KeyCode.R; // 重启
    private readonly KeyCode keyCode_Must_0 = KeyCode.A;
    private readonly KeyCode keyCode_Must_1 = KeyCode.S;
    private readonly KeyCode keyCode_Must_2 = KeyCode.D;
    private readonly KeyCode keyCode_Must_3 = KeyCode.F;
    #endregion
    
    #region 私有变量
    private MEventSystem eventSystem;
    private MGameLogicSystem gameLogicSystem;
    #endregion
    
    #region 系统接口实现
    public void Initialize()
    {
        Debug.Log("[MInputSystem] 初始化输入系统");
        
        eventSystem = SystemManager.Instance.Events;
        gameLogicSystem = SystemManager.Instance.MGameLogic;
    }
    
    public void Cleanup()
    {
        Debug.Log("[MInputSystem] 清理输入系统");
    }
    #endregion
    
    #region Unity生命周期
    void Update()
    {
        HandlePrizeSelection();
        HandlePrizeDraw();
        HandleReload();
        HandleRestart();
        HandleMustListIndexChange();
    }
    #endregion
    
    #region 输入处理方法
    private void HandlePrizeSelection()
    {
        if (Input.GetKeyDown(keyCode_Prize1))
        {
            // 发布事件：请求切换到1等奖
            eventSystem.Publish(EventId.Input_GameLogic_ChangePrizeIndex, new IntEventArg(1));
        }
        else if (Input.GetKeyDown(keyCode_Prize2))
        {
            // 发布事件：请求切换到2等奖
            eventSystem.Publish(EventId.Input_GameLogic_ChangePrizeIndex, new IntEventArg(2));
        }
        else if (Input.GetKeyDown(keyCode_Prize3))
        {
            // 发布事件：请求切换到3等奖
            eventSystem.Publish(EventId.Input_GameLogic_ChangePrizeIndex, new IntEventArg(3));
        }
        else if (Input.GetKeyDown(keyCode_Prize4))
        {
            // 发布事件：请求切换到4等奖
            eventSystem.Publish(EventId.Input_GameLogic_ChangePrizeIndex, new IntEventArg(4));
        }
    }
    
    private void HandlePrizeDraw()
    {
        if (Input.GetKeyDown(keyCode_PrizeDraw))
        {
            int currentPrizeIndex = gameLogicSystem?.CurrentPrizeIndex ?? 1;
            // 发布事件：请求抽奖
            eventSystem.Publish(EventId.Input_GameLogic_RequestDraw, new IntEventArg(currentPrizeIndex));
        }
    }
    
    private void HandleReload()
    {
        if (Input.GetKeyDown(keyCode_Reload))
        {
            // 发布事件：请求重新加载配置
            eventSystem.Publish(EventId.Input_GameLogic_ReloadConfig, EmptyEventArg.Instance);
        }
    }

    private void HandleRestart()
    {
        if (Input.GetKeyDown(keyCode_Restart))
        {
            // 发布事件：请求重启
            eventSystem.Publish(EventId.Input_GameLogic_Restart, EmptyEventArg.Instance);
        }
    }

    private void HandleMustListIndexChange()
    {
        if (Input.GetKeyDown(keyCode_Must_0))
        {
            // 发布事件：切换必中榜单到0
            eventSystem.Publish(EventId.Input_GameLogic_ChangeMustListIndex, new IntEventArg(0));
        }
        else if (Input.GetKeyDown(keyCode_Must_1))
        {
            // 发布事件：切换必中榜单到1
            eventSystem.Publish(EventId.Input_GameLogic_ChangeMustListIndex, new IntEventArg(1));
        }
        else if (Input.GetKeyDown(keyCode_Must_2))
        {
            // 发布事件：切换必中榜单到2
            eventSystem.Publish(EventId.Input_GameLogic_ChangeMustListIndex, new IntEventArg(2));
        }
        else if (Input.GetKeyDown(keyCode_Must_3))
        {
            // 发布事件：切换必中榜单到3
            eventSystem.Publish(EventId.Input_GameLogic_ChangeMustListIndex, new IntEventArg(3));
        }
    }
    #endregion
}
