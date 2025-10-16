using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI管理器 - 基于状态机模式管理抽奖流程的UI状态
/// </summary>
public class UIManager : MonoBehaviour
{
    #region 状态枚举定义
    /// <summary>
    /// UI状态枚举
    /// </summary>
    private enum UIState
    {
        Idle,           // 待机状态：等待选择奖项和抽奖
        Drawing,        // 抽奖中：播放抽奖动画
        ShowingResult,  // 显示结果：显示中奖结果，等待重启
        Transitioning   // 过渡中：播放过渡动画，准备回到待机
    }
    #endregion
    
    #region Inspector配置
    [Header("星星特效——对应不同奖项")]
    public List<GameObject> twinkleEffects; // 闪烁特效，对应不同奖项
    
    [Header("中奖结果背景图片")]
    public List<Sprite> prizeResultBackgrounds; // 对应不同奖项的背景图片
    
    [Header("人物动画配置")]
    public VideoClip playerIdleClip; // 播放器空闲时的视频
    public VideoClip playerRunClip; // 抽奖时的视频
    public VideoPlayer videoPlayerForPlayer;
    
    [Header("中奖结果显示配置")]
    public GameObject prizeResultPanel; // 显示中奖结果的面板
    public TextMeshProUGUI prizeResultText; // 显示中奖结果的ID
    public Image prizeResultImage;
    
    [Header("两次抽奖之间的过渡幕布")]
    public GameObject curtainPanel; // 过渡幕布
    public VideoPlayer curtainVideoPlayer;
    #endregion
    
    #region 状态变量
    [Header("调试信息")]
    [ReadOnly, ShowInInspector]
    private UIState currentState = UIState.Idle;
    
    #endregion
    
    #region Unity生命周期
    void Start()
    {
        // 订阅事件
        SubscribeToEvents();
        
        // 初始化到待机状态
        EnterIdleState();
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        UnsubscribeFromEvents();
    }
    #endregion
    
    #region 事件订阅管理
    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeToEvents()
    {
        GameEvents.OnPrizeIndexChanged += HandlePrizeIndexChanged;
        GameEvents.OnPrizeDrawRequested += HandlePrizeDrawRequested;
        GameEvents.OnRestartRequested += HandleRestartRequested;
        
        // 订阅视频播放器的结束事件
        if (videoPlayerForPlayer != null)
        {
            videoPlayerForPlayer.loopPointReached += OnPlayerVideoFinished;
        }
        
        if (curtainVideoPlayer != null)
        {
            curtainVideoPlayer.loopPointReached += OnCurtainVideoFinished;
        }
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnPrizeIndexChanged -= HandlePrizeIndexChanged;
        GameEvents.OnPrizeDrawRequested -= HandlePrizeDrawRequested;
        GameEvents.OnRestartRequested -= HandleRestartRequested;
        
        // 取消订阅视频播放器事件
        if (videoPlayerForPlayer != null)
        {
            videoPlayerForPlayer.loopPointReached -= OnPlayerVideoFinished;
        }
        
        if (curtainVideoPlayer != null)
        {
            curtainVideoPlayer.loopPointReached -= OnCurtainVideoFinished;
        }
    }
    #endregion
    
    #region 事件处理方法
    /// <summary>
    /// 处理奖项索引变更事件
    /// </summary>
    /// <param name="prizeIndex">奖项索引（1-4）</param>
    private void HandlePrizeIndexChanged(int prizeIndex)
    {
        // 只在待机状态下才允许切换奖项
        if (currentState != UIState.Idle)
        {
            Debug.LogWarning($"当前状态为 {currentState}，无法切换奖项");
            return;
        }
        
        UpdateTwinkleEffects(prizeIndex);
    }
    
    /// <summary>
    /// 处理抽奖请求事件
    /// </summary>
    /// <param name="prizeIndex">当前奖项索引</param>
    private void HandlePrizeDrawRequested(int prizeIndex)
    {
        // 只在待机状态下才允许抽奖
        if (currentState != UIState.Idle)
        {
            Debug.LogWarning($"当前状态为 {currentState}，无法进行抽奖");
            return;
        }
        
        // 进入抽奖状态
        EnterDrawingState();
    }
    
    /// <summary>
    /// 处理重启请求事件
    /// </summary>
    private void HandleRestartRequested()
    {
        // 只在显示结果状态下才允许重启
        if (currentState != UIState.ShowingResult)
        {
            Debug.LogWarning($"当前状态为 {currentState}，无法重启");
            return;
        }
        
        // 进入过渡状态
        EnterTransitioningState();
    }
    
    /// <summary>
    /// 当玩家动画视频播放完毕时调用
    /// </summary>
    private void OnPlayerVideoFinished(VideoPlayer vp)
    {
        // 只在抽奖状态下才处理视频结束
        if (currentState != UIState.Drawing)
        {
            return;
        }
        
        // 进入显示结果状态
        EnterShowingResultState();
    }
    
    /// <summary>
    /// 当过渡幕布视频播放完毕时调用
    /// </summary>
    private void OnCurtainVideoFinished(VideoPlayer vp)
    {
        // 只在过渡状态下才处理视频结束
        if (currentState != UIState.Transitioning)
        {
            return;
        }
        
        // 回到待机状态
        EnterIdleState();
    }
    #endregion
    
    #region 状态转换方法
    /// <summary>
    /// 进入待机状态
    /// </summary>
    private void EnterIdleState()
    {
        Debug.Log("进入待机状态");
        currentState = UIState.Idle;
        
        // 隐藏结果面板
        if (prizeResultPanel != null)
        {
            prizeResultPanel.SetActive(false);
        }
        
        // 隐藏过渡幕布
        if (curtainPanel != null)
        {
            curtainPanel.SetActive(false);
        }
        
        // 播放待机视频
        if (videoPlayerForPlayer != null && playerIdleClip != null)
        {
            videoPlayerForPlayer.clip = playerIdleClip;
            videoPlayerForPlayer.isLooping = true;
            videoPlayerForPlayer.Play();
        }
        
        // 更新星星特效
        int currentPrizeIndex = GameLogicHandler.Instance?.CurrentPrizeIndex ?? 1;
        UpdateTwinkleEffects(currentPrizeIndex);
    }
    
    /// <summary>
    /// 进入抽奖状态
    /// </summary>
    private void EnterDrawingState()
    {
        Debug.Log("进入抽奖状态");
        currentState = UIState.Drawing;
        
        // 播放抽奖动画
        if (videoPlayerForPlayer != null && playerRunClip != null)
        {
            videoPlayerForPlayer.clip = playerRunClip;
            videoPlayerForPlayer.isLooping = false;
            videoPlayerForPlayer.Play();
        }
    }
    
    /// <summary>
    /// 进入显示结果状态
    /// </summary>
    private void EnterShowingResultState()
    {
        Debug.Log("进入显示结果状态");
        currentState = UIState.ShowingResult;
        
        // 显示中奖结果面板
        if (prizeResultPanel != null)
        {
            prizeResultPanel.SetActive(true);
        }
        
        // 更新中奖ID
        if (prizeResultText != null)
        {
            prizeResultText.text = GameLogicHandler.Instance?.LastWinnerID.ToString() ?? "未知";
        }
        
        // 更新背景图片
        int prizeIndex = GameLogicHandler.Instance?.CurrentPrizeIndex ?? 1;
        if (prizeResultImage != null && prizeIndex > 0 && prizeIndex <= prizeResultBackgrounds.Count)
        {
            prizeResultImage.sprite = prizeResultBackgrounds[prizeIndex - 1];
        }
    }
    
    /// <summary>
    /// 进入过渡状态
    /// </summary>
    private void EnterTransitioningState()
    {
        Debug.Log("进入过渡状态");
        currentState = UIState.Transitioning;
        
        // 显示过渡幕布
        if (curtainPanel != null)
        {
            curtainPanel.SetActive(true);
        }
        
        // 播放过渡动画
        if (curtainVideoPlayer != null)
        {
            curtainVideoPlayer.isLooping = false;
            curtainVideoPlayer.Play();
        }
    }
    #endregion
    
    #region 辅助方法
    /// <summary>
    /// 更新星星特效显示
    /// </summary>
    /// <param name="prizeIndex">奖项索引（1-4）</param>
    private void UpdateTwinkleEffects(int prizeIndex)
    {
        if (prizeIndex > 0 && prizeIndex <= twinkleEffects.Count)
        {
            for (int i = 0; i < twinkleEffects.Count; i++)
            {
                twinkleEffects[i].SetActive(i == prizeIndex - 1);
            }
        }
    }
    #endregion
}
