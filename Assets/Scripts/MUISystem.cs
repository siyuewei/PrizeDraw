using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// UI系统 - 负责监听游戏状态变化并更新UI显示
/// </summary>
public class MUISystem : MonoBehaviour, ISystem
{
    #region Inspector配置
    [Header("星星特效——对应不同奖项")]
    public List<GameObject> twinkleEffects;
    
    [Header("中奖结果背景图片")]
    public List<Sprite> prizeResultBackgrounds;

    [Header("螃蟹背景图片")]
    public List<Sprite> crabBackgrounds;

    [Header("星星棒")]
    public List<Sprite> starBackgrounds;

    [Header("中奖结果文字位置")]
    public List<Vector3> prizeResultRects;
    
    [Header("中奖结果文字颜色")]
    public List<Color> prizeResultTextColors;
    
    [Header("中奖结果背景图Color")]
    public List<Color> prizeResultColors;
    
    [Header("人物动画配置")]
    public VideoClip playerIdleClip;
    public VideoClip playerRunClip;
    public VideoPlayer videoPlayerForPlayer;
    [Range(0, 1)]
    [Tooltip("抽奖动画播放的百分比")]
    public float percentOfPlayerRunClip = 0.5f;
    public GameObject playerRunBackgroundObject;
    public VideoPlayer videoPlayerForPlayerRunBackground;
    
    [Header("中奖结果显示配置")]
    public GameObject prizeResultPanel;
    [FormerlySerializedAs("prizeResultText")] 
    public TextMeshProUGUI prizeResultTextMeshPro;
    public Image prizeResultImage;
    public Image prizeResultBackgroundImage;
    
    [Header("两次抽奖之间的过渡幕布")]
    public GameObject curtainPanel;
    public VideoPlayer curtainVideoPlayer;
    public VideoClip curtainCloseClip;
    public VideoClip curtainOpenClip;

    [Header("必中榜单序号")]
    public TextMeshProUGUI mustListIndexTextMeshPro;
    #endregion
    
    #region 私有变量
    private bool isPlayingCloseCurtain = false;
    private bool hasNotifiedDrawingComplete = false;
    private Coroutine crabSwitcher = null;
    private Coroutine starSwitcher = null;
    
    private MEventSystem eventSystem;
    private MGameLogicSystem _mGameLogicSystem;
    #endregion

    #region 系统接口实现
    public void Initialize()
    {
        Debug.Log("[UISystem] 初始化UI系统");
        
        // 获取系统引用
        eventSystem = SystemManager.Instance.Events;
        _mGameLogicSystem = SystemManager.Instance.MGameLogic;
        
        // 订阅事件
        SubscribeToEvents();
    }
    
    public void Cleanup()
    {
        Debug.Log("[UISystem] 清理UI系统");
        UnsubscribeFromEvents();
    }
    #endregion
    
    #region 事件订阅管理
    private void SubscribeToEvents()
    {
        eventSystem.OnGameStateChanged += HandleGameStateChanged;
        eventSystem.OnPrizeIndexUpdated += HandlePrizeIndexUpdated;
        eventSystem.OnMustListIndexChanged += HandleMustListIndexChanged;
        
        if (videoPlayerForPlayer != null)
        {
            videoPlayerForPlayer.frameReady += OnPlayerFrameReady;
        }
        
        if (curtainVideoPlayer != null)
        {
            curtainVideoPlayer.loopPointReached += OnCurtainVideoFinished;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        eventSystem.OnGameStateChanged -= HandleGameStateChanged;
        eventSystem.OnPrizeIndexUpdated -= HandlePrizeIndexUpdated;
        eventSystem.OnMustListIndexChanged -= HandleMustListIndexChanged;
        
        if (videoPlayerForPlayer != null)
        {
            videoPlayerForPlayer.frameReady -= OnPlayerFrameReady;
        }
        
        if (curtainVideoPlayer != null)
        {
            curtainVideoPlayer.loopPointReached -= OnCurtainVideoFinished;
        }
    }
    #endregion
    
    #region 事件处理方法
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
    
    private void HandlePrizeIndexUpdated(int prizeIndex)
    {
        Debug.Log($"[UISystem] 更新奖项显示: {prizeIndex}等奖");
        UpdateTwinkleEffects(prizeIndex);
    }
    
    private void OnPlayerFrameReady(VideoPlayer vp, long frameIdx)
    {
        CheckDrawingAnimationProgress();
    }
    
    private void OnCurtainVideoFinished(VideoPlayer vp)
    {
        if (isPlayingCloseCurtain)
        {
            Debug.Log("[UISystem] 黑屏动画完成，重置UI");
            ResetAllUI();
            
            if (curtainVideoPlayer != null && curtainOpenClip != null)
            {
                Debug.Log("[UISystem] 播放开屏动画");
                curtainVideoPlayer.clip = curtainOpenClip;
                curtainVideoPlayer.Play();
                isPlayingCloseCurtain = false;
            }
            else
            {
                eventSystem.NotifyTransitionComplete();
            }
        }
        else
        {
            Debug.Log("[UISystem] 开屏动画完成，过渡结束");
            eventSystem.NotifyTransitionComplete();
        }
    }
    
    private void HandleMustListIndexChanged(int mustListIndex)
    {
        if (mustListIndexTextMeshPro != null)
        {
            mustListIndexTextMeshPro.text = mustListIndex.ToString();
        }
    }
    #endregion
    
    #region UI显示方法
    private void ShowIdleUI()
    {
        Debug.Log("[UISystem] 显示待机状态");
        
        ResetAllUI();
        
        if (curtainPanel != null)
        {
            curtainPanel.SetActive(false);
        }
    }
    
    private void ShowDrawingUI()
    {
        Debug.Log("[UISystem] 显示抽奖状态");
        
        hasNotifiedDrawingComplete = false;
        
        if (videoPlayerForPlayer != null && playerRunClip != null)
        {
            videoPlayerForPlayer.clip = playerRunClip;
            videoPlayerForPlayer.isLooping = false;
            videoPlayerForPlayer.sendFrameReadyEvents = true;
            videoPlayerForPlayer.Play();
        }
        
        if (playerRunBackgroundObject != null)
        {
            playerRunBackgroundObject.SetActive(true);
            if (videoPlayerForPlayerRunBackground != null)
            {
                ClearVideoPlayerRenderTexture(videoPlayerForPlayerRunBackground);
                
                videoPlayerForPlayerRunBackground.isLooping = true;
                videoPlayerForPlayerRunBackground.Play();
            }
        }
    }
    
    private void ShowResultUI()
    {
        Debug.Log("[UISystem] 显示结果状态");
        
        if (prizeResultPanel != null)
        {
            prizeResultPanel.SetActive(true);
        }
        
        if (prizeResultTextMeshPro != null)
        {
            prizeResultTextMeshPro.text = _mGameLogicSystem?.LastWinnerID.ToString() ?? "未知";
        }
        
        int prizeIndex = _mGameLogicSystem?.CurrentPrizeIndex ?? 1;
        if (prizeResultImage != null && prizeIndex > 0 && prizeIndex <= prizeResultBackgrounds.Count)
        {
            if (prizeIndex == 3)
            {
                crabSwitcher = StartCoroutine(SwitchImages());
            }
            else if(prizeIndex == 2)
            {
                starSwitcher = StartCoroutine(StarSwitchImages());
            }
            else
            {
                prizeResultImage.sprite = prizeResultBackgrounds[prizeIndex - 1];
            }
        }

        if (prizeResultBackgroundImage != null && prizeIndex > 0 && prizeIndex <= prizeResultColors.Count)
        {
            prizeResultBackgroundImage.color = prizeResultColors[prizeIndex - 1];
        }
        
        if (prizeResultTextMeshPro != null && prizeIndex > 0 && prizeIndex <= prizeResultRects.Count)
        {
            prizeResultTextMeshPro.rectTransform.localPosition = prizeResultRects[prizeIndex - 1];
        }
        
        if (prizeResultTextMeshPro != null && prizeIndex > 0 && prizeIndex <= prizeResultTextColors.Count)
        {
            prizeResultTextMeshPro.color = prizeResultTextColors[prizeIndex - 1];
        }
    }
    
    private void ShowTransitionUI()
    {
        Debug.Log("[UISystem] 显示过渡状态");

        if (crabSwitcher != null)
        {
            StopCoroutine(crabSwitcher);
            crabSwitcher = null;
        }

        if (starSwitcher != null)
        {
            StopCoroutine(starSwitcher);
            starSwitcher = null;
        }

        if (curtainPanel != null)
        {
            curtainPanel.SetActive(true);
        }
        
        if (curtainVideoPlayer != null && curtainCloseClip != null)
        {
            Debug.Log("[UISystem] 播放黑屏关闭动画");
            curtainVideoPlayer.clip = curtainCloseClip;
            curtainVideoPlayer.isLooping = false;
            curtainVideoPlayer.Play();
            isPlayingCloseCurtain = true;
        }
    }
    #endregion
    
    #region 辅助方法
    private void CheckDrawingAnimationProgress()
    {
        if (_mGameLogicSystem?.CurrentState != GameState.Drawing)
        {
            return;
        }
        
        if (hasNotifiedDrawingComplete)
        {
            return;
        }
        
        if (videoPlayerForPlayer == null || videoPlayerForPlayer.clip == null)
        {
            return;
        }
        
        if (!videoPlayerForPlayer.isPlaying)
        {
            return;
        }
        
        double currentTime = videoPlayerForPlayer.time;
        double totalTime = videoPlayerForPlayer.clip.length;

        if (totalTime <= 0)
        {
            return;
        }
        
        float currentProgress = (float)(currentTime / totalTime);
        
        if (currentProgress >= percentOfPlayerRunClip)
        {
            Debug.Log($"[UISystem] 抽奖动画已播放到 {currentProgress * 100:F1}%（设定值：{percentOfPlayerRunClip * 100:F1}%），准备显示结果");
            hasNotifiedDrawingComplete = true;
            videoPlayerForPlayer.sendFrameReadyEvents = false;
            eventSystem.NotifyReadyToShowResult();
        }
    }
    
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
    
    private void ResetAllUI()
    {
        Debug.Log("[UISystem] 重置所有UI");
        
        if (prizeResultPanel != null)
        {
            prizeResultPanel.SetActive(false);
        }
        
        if (prizeResultTextMeshPro != null)
        {
            prizeResultTextMeshPro.text = "";
        }
        
        if (prizeResultImage != null)
        {
            prizeResultImage.sprite = null;
        }
        
        if (videoPlayerForPlayer != null && playerIdleClip != null)
        {
            videoPlayerForPlayer.clip = playerIdleClip;
            videoPlayerForPlayer.isLooping = true;
            videoPlayerForPlayer.Play();
        }
        
        if (playerRunBackgroundObject != null)
        {
            playerRunBackgroundObject.SetActive(false);
        }
        
        if (videoPlayerForPlayerRunBackground != null)
        {
            if (videoPlayerForPlayerRunBackground.isPlaying)
            {
                videoPlayerForPlayerRunBackground.Stop();
            }
            
            ClearVideoPlayerRenderTexture(videoPlayerForPlayerRunBackground);
        }
        
        int currentPrizeIndex = _mGameLogicSystem?.CurrentPrizeIndex ?? 1;
        UpdateTwinkleEffects(currentPrizeIndex);
    }
    
    private void ClearVideoPlayerRenderTexture(VideoPlayer videoPlayer)
    {
        if (videoPlayer == null || videoPlayer.targetTexture == null)
        {
            return;
        }
        
        RenderTexture rt = videoPlayer.targetTexture;
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = currentRT;
    }
    
    private IEnumerator SwitchImages()
    {
        int index = 0;
        while (true)
        {
            prizeResultImage.sprite = crabBackgrounds[index];
            index = (index + 1) % crabBackgrounds.Count;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator StarSwitchImages()
    {
        int index = 0;
        while (true)
        {
            prizeResultImage.sprite = starBackgrounds[index];
            index = (index + 1) % starBackgrounds.Count;
            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion
}

