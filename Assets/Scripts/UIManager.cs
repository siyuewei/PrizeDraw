using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI管理器 - 负责监听游戏状态变化并更新UI显示
/// </summary>
public class UIManager : MonoBehaviour
{
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
    public VideoClip curtainCloseClip; // 黑屏关闭动画
    public VideoClip curtainOpenClip; // 开屏打开动画
    #endregion
    
    #region 私有变量
    private bool isPlayingCloseCurtain = false; // 标记当前是否正在播放关闭动画
    #endregion
    
    #region Unity生命周期
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
    #endregion
    
    #region 事件订阅管理
    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeToEvents()
    {
        // 订阅游戏状态变化事件
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        
        // 订阅奖项更新事件（由GameLogicHandler验证后发送）
        GameEvents.OnPrizeIndexUpdated += HandlePrizeIndexUpdated;
        
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
        // 取消订阅游戏状态变化事件
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        
        // 取消订阅奖项更新事件
        GameEvents.OnPrizeIndexUpdated -= HandlePrizeIndexUpdated;
        
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
    /// 处理游戏状态变化事件
    /// </summary>
    /// <param name="newState">新的游戏状态</param>
    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Idle:
                ShowIdleUI();
                break;
            case GameState.Drawing:
                ShowDrawingUI();
                break;
            case GameState.ShowingResult:
                ShowResultUI();
                break;
            case GameState.Transitioning:
                ShowTransitionUI();
                break;
        }
    }
    
    /// <summary>
    /// 处理奖项索引更新事件（已通过GameLogicHandler验证）
    /// </summary>
    /// <param name="prizeIndex">奖项索引（1-4）</param>
    private void HandlePrizeIndexUpdated(int prizeIndex)
    {
        Debug.Log($"[UI] 更新奖项显示: {prizeIndex}等奖");
        UpdateTwinkleEffects(prizeIndex);
    }
    
    /// <summary>
    /// 当玩家动画视频播放完毕时调用
    /// </summary>
    private void OnPlayerVideoFinished(VideoPlayer vp)
    {
        // 通知游戏逻辑：抽奖动画已完成
        GameEvents.NotifyDrawingComplete();
    }
    
    /// <summary>
    /// 当过渡幕布视频播放完毕时调用
    /// </summary>
    private void OnCurtainVideoFinished(VideoPlayer vp)
    {
        if (isPlayingCloseCurtain)
        {
            // 第一段黑屏动画结束，重置UI
            Debug.Log("[UI] 黑屏动画完成，重置UI");
            ResetAllUI();
            
            // 播放第二段开屏动画
            if (curtainVideoPlayer != null && curtainOpenClip != null)
            {
                Debug.Log("[UI] 播放开屏动画");
                curtainVideoPlayer.clip = curtainOpenClip;
                curtainVideoPlayer.Play();
                isPlayingCloseCurtain = false;
            }
            else
            {
                // 如果没有开屏动画，直接结束过渡
                GameEvents.NotifyTransitionComplete();
            }
        }
        else
        {
            // 第二段开屏动画结束，通知游戏逻辑：过渡动画已完成
            Debug.Log("[UI] 开屏动画完成，过渡结束");
            GameEvents.NotifyTransitionComplete();
        }
    }
    #endregion
    
    #region UI显示方法
    /// <summary>
    /// 显示待机状态UI
    /// </summary>
    private void ShowIdleUI()
    {
        Debug.Log("[UI] 显示待机状态");
        
        // 重置所有UI到初始状态
        ResetAllUI();
        
        // 隐藏过渡幕布
        if (curtainPanel != null)
        {
            curtainPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 显示抽奖状态UI
    /// </summary>
    private void ShowDrawingUI()
    {
        Debug.Log("[UI] 显示抽奖状态");
        
        // 播放抽奖动画
        if (videoPlayerForPlayer != null && playerRunClip != null)
        {
            videoPlayerForPlayer.clip = playerRunClip;
            videoPlayerForPlayer.isLooping = false;
            videoPlayerForPlayer.Play();
        }
    }
    
    /// <summary>
    /// 显示结果状态UI
    /// </summary>
    private void ShowResultUI()
    {
        Debug.Log("[UI] 显示结果状态");
        
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
    /// 显示过渡状态UI
    /// </summary>
    private void ShowTransitionUI()
    {
        Debug.Log("[UI] 显示过渡状态");
        
        // 显示过渡幕布
        if (curtainPanel != null)
        {
            curtainPanel.SetActive(true);
        }
        
        // 播放第一段黑屏关闭动画
        if (curtainVideoPlayer != null && curtainCloseClip != null)
        {
            Debug.Log("[UI] 播放黑屏关闭动画");
            curtainVideoPlayer.clip = curtainCloseClip;
            curtainVideoPlayer.isLooping = false;
            curtainVideoPlayer.Play();
            isPlayingCloseCurtain = true;
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
    
    /// <summary>
    /// 重置所有UI元素到初始状态
    /// </summary>
    private void ResetAllUI()
    {
        Debug.Log("[UI] 重置所有UI");
        
        // 隐藏中奖结果面板
        if (prizeResultPanel != null)
        {
            prizeResultPanel.SetActive(false);
        }
        
        // 清空中奖结果文本
        if (prizeResultText != null)
        {
            prizeResultText.text = "";
        }
        
        // 重置背景图片
        if (prizeResultImage != null)
        {
            prizeResultImage.sprite = null;
        }
        
        // 重置人物视频到待机状态
        if (videoPlayerForPlayer != null && playerIdleClip != null)
        {
            videoPlayerForPlayer.clip = playerIdleClip;
            videoPlayerForPlayer.isLooping = true;
            videoPlayerForPlayer.Play();
        }
        
        // 更新星星特效到当前奖项
        int currentPrizeIndex = GameLogicHandler.Instance?.CurrentPrizeIndex ?? 1;
        UpdateTwinkleEffects(currentPrizeIndex);
    }
    #endregion
}
