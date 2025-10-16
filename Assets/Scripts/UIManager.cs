using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI管理器 - 负责监听游戏事件并更新UI显示
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("星星特效——对应不同奖项")]
    public List<GameObject> twinkleEffects; // 闪烁特效，对应不同奖项
    
    [Header("中将结果背景图片")]
    public List<Sprite> prizeResultBackgrounds; // 对应不同奖项的背景图片
    
    [Header("人物动画配置")]
    public VideoClip playerIdleClip; // 播放器空闲时的视频
    public VideoClip playerRunClip;
    public VideoPlayer videoPlayerForPlayer;
    
    [Header("中奖结果显示配置")]
    public GameObject prizeResultPanel; // 显示中奖结果的面板
    public TextMeshProUGUI prizeResultText; // 显示中奖结果的ID
    public Image prizeResultImage;
    
    void PrintTest(int prizeIndex)
    {
        Debug.Log($"Test Prize Draw Invoked for Prize Index: {prizeIndex}");
    }
    
    void Start()
    {
        // 订阅事件
        SubscribeToEvents();
        
        // 订阅视频播放器的结束事件
        if (videoPlayerForPlayer != null)
        {
            videoPlayerForPlayer.loopPointReached += OnVideoPlayerFinished;
        }
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        UnsubscribeFromEvents();
        
        // 取消订阅视频播放器事件
        if (videoPlayerForPlayer != null)
        {
            videoPlayerForPlayer.loopPointReached -= OnVideoPlayerFinished;
        }
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
        // 切换视频到抽奖状态
        if(videoPlayerForPlayer != null && playerRunClip != null)
        {
            videoPlayerForPlayer.clip = playerRunClip;
            videoPlayerForPlayer.Play();
        }
    }

    private void OnVideoPlayerFinished(VideoPlayer vp)
    {
        prizeResultPanel.SetActive(true);
        prizeResultText.text = GameLogicHandler.Instance?.LastWinnerID.ToString() ?? "未知";
        int prizeIndex = GameLogicHandler.Instance?.CurrentPrizeIndex ?? 1;
        if (prizeIndex > 0 && prizeIndex <= prizeResultBackgrounds.Count)
        {
            prizeResultImage.sprite = prizeResultBackgrounds[prizeIndex - 1];
        }
    }
}
