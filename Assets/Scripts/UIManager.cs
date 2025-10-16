using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI管理器 - 负责监听游戏事件并更新UI显示
/// </summary>
public class UIManager : MonoBehaviour
{
    public List<GameObject> twinkleEffects; // 闪烁特效，对应不同奖项
    
    void Start()
    {
        // 订阅事件
        SubscribeToEvents();
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeToEvents()
    {
        GameEvents.OnPrizeIndexChanged += HandlePrizeIndexChanged;
        GameEvents.OnPrizeDrawRequested += HandlePrizeDrawRequested;
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnPrizeIndexChanged -= HandlePrizeIndexChanged;
        GameEvents.OnPrizeDrawRequested -= HandlePrizeDrawRequested;
    }
    
    /// <summary>
    /// 处理奖项索引变更事件
    /// </summary>
    /// <param name="prizeIndex">奖项索引（1-4）</param>
    private void HandlePrizeIndexChanged(int prizeIndex)
    {
       if(prizeIndex > 0 && prizeIndex <= twinkleEffects.Count)
       {
           for(int i = 0; i < twinkleEffects.Count; i++)
           {
               twinkleEffects[i].SetActive(i == prizeIndex - 1);
           }
       }
    }
    
    /// <summary>
    /// 处理抽奖请求事件
    /// </summary>
    /// <param name="prizeIndex">当前奖项索引</param>
    private void HandlePrizeDrawRequested(int prizeIndex)
    {
    }
}
