using UnityEngine;

/// <summary>
/// 输入系统 - 负责处理键盘输入并发送相应事件
/// </summary>
public class MInputSystem : MonoBehaviour, ISystem
{
    #region 按键映射
    private readonly KeyCode keyCode_Prize1 = KeyCode.Alpha5; // 一等奖
    private readonly KeyCode keyCode_Prize2 = KeyCode.Alpha2; // 二等奖
    private readonly KeyCode keyCode_Prize3 = KeyCode.Alpha3; // 三等奖
    private readonly KeyCode keyCode_Prize4 = KeyCode.Alpha4; // 四等奖
    private readonly KeyCode keyCode_PrizeDraw = KeyCode.Alpha1; // 抽奖键
    private readonly KeyCode keyCode_Reload = KeyCode.C; // 重新加载配置和黑名单
    private readonly KeyCode keyCode_Restart = KeyCode.R; // 重新开启一次抽奖
    private readonly KeyCode keyCode_Must_0 = KeyCode.A;
    private readonly KeyCode keyCode_Must_1 = KeyCode.S;
    private readonly KeyCode keyCode_Must_2 = KeyCode.D;
    private readonly KeyCode keyCode_Must_3 = KeyCode.F;
    #endregion
    
    #region 私有变量
    private MEventSystem eventSystem;
    private MGameLogicSystem _mGameLogicSystem;
    #endregion
    
    #region 系统接口实现
    public void Initialize()
    {
        Debug.Log("[InputSystem] 初始化输入系统");
        
        // 获取系统引用
        eventSystem = SystemManager.Instance.Events;
        _mGameLogicSystem = SystemManager.Instance.MGameLogic;
    }
    
    public void Cleanup()
    {
        Debug.Log("[InputSystem] 清理输入系统");
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
    /// <summary>
    /// 处理奖项选择输入
    /// </summary>
    private void HandlePrizeSelection()
    {
        if (Input.GetKeyDown(keyCode_Prize1))
        {
            eventSystem.RequestPrizeIndexChange(1);
        }
        else if (Input.GetKeyDown(keyCode_Prize2))
        {
            eventSystem.RequestPrizeIndexChange(2);
        }
        else if (Input.GetKeyDown(keyCode_Prize3))
        {
            eventSystem.RequestPrizeIndexChange(3);
        }
        else if (Input.GetKeyDown(keyCode_Prize4))
        {
            eventSystem.RequestPrizeIndexChange(4);
        }
    }
    
    /// <summary>
    /// 处理抽奖输入
    /// </summary>
    private void HandlePrizeDraw()
    {
        if (Input.GetKeyDown(keyCode_PrizeDraw))
        {
            int currentPrizeIndex = _mGameLogicSystem?.CurrentPrizeIndex ?? 1;
            eventSystem.RequestPrizeDraw(currentPrizeIndex);
        }
    }
    
    /// <summary>
    /// 处理重新加载输入
    /// </summary>
    private void HandleReload()
    {
        if (Input.GetKeyDown(keyCode_Reload))
        {
            eventSystem.RequestReloadConfig();
        }
    }

    /// <summary>
    /// 处理重新开始抽奖输入
    /// </summary>
    private void HandleRestart()
    {
        if (Input.GetKeyDown(keyCode_Restart))
        {
            eventSystem.RequestRestart();
        }
    }

    /// <summary>
    /// 处理必中榜单索引切换输入
    /// </summary>
    private void HandleMustListIndexChange()
    {
        if (Input.GetKeyDown(keyCode_Must_0))
        {
            eventSystem.ChangeMustListIndex(0);
        }
        else if (Input.GetKeyDown(keyCode_Must_1))
        {
            eventSystem.ChangeMustListIndex(1);
        }
        else if (Input.GetKeyDown(keyCode_Must_2))
        {
            eventSystem.ChangeMustListIndex(2);
        }
        else if (Input.GetKeyDown(keyCode_Must_3))
        {
            eventSystem.ChangeMustListIndex(3);
        }
    }
    #endregion
}

