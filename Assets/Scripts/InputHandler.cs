using UnityEngine;

/// <summary>
/// 输入处理器 - 负责处理键盘输入并发送相应事件
/// </summary>
public class InputHandler : MonoBehaviour
{
    // 按键映射
    private readonly KeyCode _keyCode_Prize1 = KeyCode.Alpha1; // 一等奖
    private readonly KeyCode _keyCode_Prize2 = KeyCode.Alpha2; // 二等奖
    private readonly KeyCode _keyCode_Prize3 = KeyCode.Alpha3; // 三等奖
    private readonly KeyCode _keyCode_Prize4 = KeyCode.Alpha4; // 四等奖
    private readonly KeyCode _keyCode_PrizeDraw = KeyCode.Return; // 抽奖键
    private readonly KeyCode _keyCode_reload = KeyCode.C; // 重新加载配置和黑名单
    private readonly KeyCode _keyCode_restart = KeyCode.R; //重新开启一次抽奖

    void Update()
    {
        HandlePrizeSelection();
        HandlePrizeDraw();
        HandleReload();
        HandleRestart();
    }
    
    /// <summary>
    /// 处理奖项选择输入
    /// </summary>
    private void HandlePrizeSelection()
    {
        if (Input.GetKeyDown(_keyCode_Prize1))
        {
            GameEvents.RequestPrizeIndexChange(1);
            Debug.Log("请求切换到一等奖");
        }
        else if (Input.GetKeyDown(_keyCode_Prize2))
        {
            GameEvents.RequestPrizeIndexChange(2);
            Debug.Log("请求切换到二等奖");
        }
        else if (Input.GetKeyDown(_keyCode_Prize3))
        {
            GameEvents.RequestPrizeIndexChange(3);
            Debug.Log("请求切换到三等奖");
        }
        else if (Input.GetKeyDown(_keyCode_Prize4))
        {
            GameEvents.RequestPrizeIndexChange(4);
            Debug.Log("请求切换到四等奖");
        }
    }
    
    /// <summary>
    /// 处理抽奖输入
    /// </summary>
    private void HandlePrizeDraw()
    {
        if (Input.GetKeyDown(_keyCode_PrizeDraw))
        {
            // 获取当前奖项索引（这里需要从游戏逻辑处理器获取）
            int currentPrizeIndex = GameLogicHandler.Instance?.CurrentPrizeIndex ?? 1;
            GameEvents.RequestPrizeDraw(currentPrizeIndex);
        }
    }
    
    /// <summary>
    /// 处理重新加载输入
    /// </summary>
    private void HandleReload()
    {
        if (Input.GetKeyDown(_keyCode_reload))
        {
            GameEvents.RequestReloadConfig();
        }
    }

    /// <summary>
    /// 处理重新开始抽奖输入
    /// </summary>
    private void HandleRestart()
    {
        if (Input.GetKeyDown(_keyCode_restart))
        {
            GameEvents.RequestRestart();
        }
    }
}
