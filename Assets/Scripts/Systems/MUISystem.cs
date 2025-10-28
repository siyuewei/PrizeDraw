using System.Collections;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// UI系统 - 负责监听游戏状态变化并更新UI显示
/// 
/// 使用方式：
/// 1. 在场景中添加UIReferences组件
/// 2. 在UIReferences中设置UIConfig资产和场景UI引用
/// 3. MUISystem会自动从UIReferences获取配置
/// 
/// 订阅的事件（GameLogic → UI）：
/// - GameLogic_UI_StateChanged - 游戏状态改变
/// - GameLogic_UI_PrizeIndexUpdated - 奖项已更新
/// - GameLogic_UI_MustListIndexChanged - 必中榜单索引改变
/// 
/// 发布的事件（UI → GameLogic）：
/// - UI_GameLogic_ReadyToShowResult - 抽奖动画完成，准备显示结果
/// - UI_GameLogic_TransitionComplete - 过渡动画完成
/// </summary>
[RequireComponent(typeof(UIReferences))]
public class MUISystem : MonoBehaviour, ISystem
{
    #region 引用
    private UIReferences uiRefs;
    private UIConfig config;
    #endregion
    
    #region 私有变量
    private bool isPlayingCloseCurtain = false;
    private bool hasNotifiedDrawingComplete = false;
    private Coroutine crabSwitcher = null;
    private Coroutine starSwitcher = null;
    
    private MEventSystem eventSystem;
    private MGameLogicSystem gameLogicSystem;
    #endregion

    #region 系统接口实现
    public void Initialize()
    {
        Debug.Log("[MUISystem] 初始化UI系统");
        
        eventSystem = SystemManager.Instance.Events;
        gameLogicSystem = SystemManager.Instance.MGameLogic;
        
        // 获取UI引用组件
        uiRefs = FindObjectOfType<UIReferences>();
        if (uiRefs == null)
        {
            Debug.LogError("[MUISystem] 场景中未找到UIReferences组件！请在场景中添加该组件并配置UI引用。");
            return;
        }
        
        config = uiRefs.config;
        if (config == null)
        {
            Debug.LogError("[MUISystem] UIReferences中未设置UIConfig！请在Inspector中指定配置资产。");
            return;
        }
        
        SubscribeToEvents();
        
        Debug.Log("[MUISystem] UI系统初始化完成");
    }
    
    public void Cleanup()
    {
        Debug.Log("[MUISystem] 清理UI系统");
        UnsubscribeFromEvents();
    }
    #endregion
    
    #region 事件订阅管理
    private void SubscribeToEvents()
    {
        // 订阅来自GameLogicSystem的事件
        eventSystem.Subscribe<GameStateEventArg>(EventId.GameLogic_UI_StateChanged, HandleGameStateChanged);
        eventSystem.Subscribe<IntEventArg>(EventId.GameLogic_UI_PrizeIndexUpdated, HandlePrizeIndexUpdated);
        eventSystem.Subscribe<IntEventArg>(EventId.GameLogic_UI_MustListIndexChanged, HandleMustListIndexChanged);
        
        if (uiRefs.videoPlayerForPlayer != null)
        {
            uiRefs.videoPlayerForPlayer.frameReady += OnPlayerFrameReady;
        }
        
        if (uiRefs.curtainVideoPlayer != null)
        {
            uiRefs.curtainVideoPlayer.loopPointReached += OnCurtainVideoFinished;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        eventSystem.Unsubscribe<GameStateEventArg>(EventId.GameLogic_UI_StateChanged, HandleGameStateChanged);
        eventSystem.Unsubscribe<IntEventArg>(EventId.GameLogic_UI_PrizeIndexUpdated, HandlePrizeIndexUpdated);
        eventSystem.Unsubscribe<IntEventArg>(EventId.GameLogic_UI_MustListIndexChanged, HandleMustListIndexChanged);
        
        if (uiRefs != null && uiRefs.videoPlayerForPlayer != null)
        {
            uiRefs.videoPlayerForPlayer.frameReady -= OnPlayerFrameReady;
        }
        
        if (uiRefs != null && uiRefs.curtainVideoPlayer != null)
        {
            uiRefs.curtainVideoPlayer.loopPointReached -= OnCurtainVideoFinished;
        }
    }
    #endregion
    
    #region 事件处理方法
    private void HandleGameStateChanged(GameStateEventArg arg)
    {
        switch (arg.state)
        {
            case GameState.WaitingForDraw:
                ShowIdleUI();
                break;
            case GameState.DrawingInProgress:
                ShowDrawingUI();
                break;
            case GameState.ShowingWinner:
                ShowResultUI();
                break;
            case GameState.TransitionToNext:
                ShowTransitionUI();
                break;
        }
    }
    
    private void HandlePrizeIndexUpdated(IntEventArg arg)
    {
        Debug.Log($"[MUISystem] 更新奖项显示: {arg.value}等奖");
        UpdateTwinkleEffects(arg.value);
    }
    
    private void OnPlayerFrameReady(VideoPlayer vp, long frameIdx)
    {
        CheckDrawingAnimationProgress();
    }
    
    private void OnCurtainVideoFinished(VideoPlayer vp)
    {
        if (isPlayingCloseCurtain)
        {
            Debug.Log("[MUISystem] 黑屏动画完成，重置UI");
            ResetAllUI();
            
            if (uiRefs.curtainVideoPlayer != null && config.curtainOpenClip != null)
            {
                Debug.Log("[MUISystem] 播放开屏动画");
                uiRefs.curtainVideoPlayer.clip = config.curtainOpenClip;
                uiRefs.curtainVideoPlayer.Play();
                isPlayingCloseCurtain = false;
            }
            else
            {
                // 发布事件：过渡完成（UI → GameLogic）
                eventSystem.Publish(EventId.UI_GameLogic_TransitionComplete, EmptyEventArg.Instance);
            }
        }
        else
        {
            Debug.Log("[MUISystem] 开屏动画完成");
            // 发布事件：过渡完成（UI → GameLogic）
            eventSystem.Publish(EventId.UI_GameLogic_TransitionComplete, EmptyEventArg.Instance);
        }
    }
    
    private void HandleMustListIndexChanged(IntEventArg arg)
    {
        if (uiRefs.mustListIndexTextMeshPro != null)
        {
            uiRefs.mustListIndexTextMeshPro.text = arg.value.ToString();
        }
    }
    #endregion
    
    #region UI显示方法
    private void ShowIdleUI()
    {
        Debug.Log("[MUISystem] 显示待机状态");
        
        ResetAllUI();
        
        if (uiRefs.curtainPanel != null)
        {
            uiRefs.curtainPanel.SetActive(false);
        }
    }
    
    private void ShowDrawingUI()
    {
        Debug.Log("[MUISystem] 显示抽奖状态");
        
        hasNotifiedDrawingComplete = false;
        
        if (uiRefs.videoPlayerForPlayer != null && config.playerRunClip != null)
        {
            uiRefs.videoPlayerForPlayer.clip = config.playerRunClip;
            uiRefs.videoPlayerForPlayer.isLooping = false;
            uiRefs.videoPlayerForPlayer.sendFrameReadyEvents = true;
            uiRefs.videoPlayerForPlayer.Play();
        }
        
        if (uiRefs.playerRunBackgroundObject != null)
        {
            uiRefs.playerRunBackgroundObject.SetActive(true);
            if (uiRefs.videoPlayerForPlayerRunBackground != null)
            {
                ClearVideoPlayerRenderTexture(uiRefs.videoPlayerForPlayerRunBackground);
                
                uiRefs.videoPlayerForPlayerRunBackground.isLooping = true;
                uiRefs.videoPlayerForPlayerRunBackground.Play();
            }
        }
    }
    
    private void ShowResultUI()
    {
        Debug.Log("[MUISystem] 显示结果状态");
        
        if (uiRefs.prizeResultPanel != null)
        {
            uiRefs.prizeResultPanel.SetActive(true);
        }
        
        if (uiRefs.prizeResultTextMeshPro != null)
        {
            uiRefs.prizeResultTextMeshPro.text = gameLogicSystem?.LastWinnerID.ToString() ?? "未知";
        }
        
        int prizeIndex = gameLogicSystem?.CurrentPrizeIndex ?? 1;
        if (uiRefs.prizeResultImage != null && prizeIndex > 0 && prizeIndex <= config.prizeResultBackgrounds.Count)
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
                uiRefs.prizeResultImage.sprite = config.prizeResultBackgrounds[prizeIndex - 1];
            }
        }

        if (uiRefs.prizeResultBackgroundImage != null && prizeIndex > 0 && prizeIndex <= config.prizeResultColors.Count)
        {
            uiRefs.prizeResultBackgroundImage.color = config.prizeResultColors[prizeIndex - 1];
        }
        
        if (uiRefs.prizeResultTextMeshPro != null && prizeIndex > 0 && prizeIndex <= config.prizeResultRects.Count)
        {
            uiRefs.prizeResultTextMeshPro.rectTransform.localPosition = config.prizeResultRects[prizeIndex - 1];
        }
        
        if (uiRefs.prizeResultTextMeshPro != null && prizeIndex > 0 && prizeIndex <= config.prizeResultTextColors.Count)
        {
            uiRefs.prizeResultTextMeshPro.color = config.prizeResultTextColors[prizeIndex - 1];
        }
    }
    
    private void ShowTransitionUI()
    {
        Debug.Log("[MUISystem] 显示过渡状态");

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

        if (uiRefs.curtainPanel != null)
        {
            uiRefs.curtainPanel.SetActive(true);
        }
        
        if (uiRefs.curtainVideoPlayer != null && config.curtainCloseClip != null)
        {
            Debug.Log("[MUISystem] 播放黑屏关闭动画");
            uiRefs.curtainVideoPlayer.clip = config.curtainCloseClip;
            uiRefs.curtainVideoPlayer.isLooping = false;
            uiRefs.curtainVideoPlayer.Play();
            isPlayingCloseCurtain = true;
        }
    }
    #endregion
    
    #region 辅助方法
    private void CheckDrawingAnimationProgress()
    {
        if (gameLogicSystem?.CurrentState != GameState.DrawingInProgress)
        {
            return;
        }
        
        if (hasNotifiedDrawingComplete)
        {
            return;
        }
        
        if (uiRefs.videoPlayerForPlayer == null || uiRefs.videoPlayerForPlayer.clip == null)
        {
            return;
        }
        
        if (!uiRefs.videoPlayerForPlayer.isPlaying)
        {
            return;
        }
        
        double currentTime = uiRefs.videoPlayerForPlayer.time;
        double totalTime = uiRefs.videoPlayerForPlayer.clip.length;

        if (totalTime <= 0)
        {
            return;
        }
        
        float currentProgress = (float)(currentTime / totalTime);
        
        if (currentProgress >= config.percentOfPlayerRunClip)
        {
            Debug.Log($"[MUISystem] 抽奖动画已播放到 {currentProgress * 100:F1}%，准备显示结果");
            hasNotifiedDrawingComplete = true;
            uiRefs.videoPlayerForPlayer.sendFrameReadyEvents = false;
            
            // 发布事件：准备显示结果（UI → GameLogic）
            eventSystem.Publish(EventId.UI_GameLogic_ReadyToShowResult, EmptyEventArg.Instance);
        }
    }
    
    private void UpdateTwinkleEffects(int prizeIndex)
    {
        if (prizeIndex > 0 && prizeIndex <= uiRefs.twinkleEffects.Count)
        {
            for (int i = 0; i < uiRefs.twinkleEffects.Count; i++)
            {
                uiRefs.twinkleEffects[i].SetActive(i == prizeIndex - 1);
            }
        }
    }
    
    private void ResetAllUI()
    {
        Debug.Log("[MUISystem] 重置所有UI");
        
        if (uiRefs.prizeResultPanel != null)
        {
            uiRefs.prizeResultPanel.SetActive(false);
        }
        
        if (uiRefs.prizeResultTextMeshPro != null)
        {
            uiRefs.prizeResultTextMeshPro.text = "";
        }
        
        if (uiRefs.prizeResultImage != null)
        {
            uiRefs.prizeResultImage.sprite = null;
        }
        
        if (uiRefs.videoPlayerForPlayer != null && config.playerIdleClip != null)
        {
            uiRefs.videoPlayerForPlayer.clip = config.playerIdleClip;
            uiRefs.videoPlayerForPlayer.isLooping = true;
            uiRefs.videoPlayerForPlayer.Play();
        }
        
        if (uiRefs.playerRunBackgroundObject != null)
        {
            uiRefs.playerRunBackgroundObject.SetActive(false);
        }
        
        if (uiRefs.videoPlayerForPlayerRunBackground != null)
        {
            if (uiRefs.videoPlayerForPlayerRunBackground.isPlaying)
            {
                uiRefs.videoPlayerForPlayerRunBackground.Stop();
            }
            
            ClearVideoPlayerRenderTexture(uiRefs.videoPlayerForPlayerRunBackground);
        }
        
        int currentPrizeIndex = gameLogicSystem?.CurrentPrizeIndex ?? 1;
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
            uiRefs.prizeResultImage.sprite = config.crabBackgrounds[index];
            index = (index + 1) % config.crabBackgrounds.Count;
            yield return new WaitForSeconds(config.crabSwitchInterval);
        }
    }

    private IEnumerator StarSwitchImages()
    {
        int index = 0;
        while (true)
        {
            uiRefs.prizeResultImage.sprite = config.starBackgrounds[index];
            index = (index + 1) % config.starBackgrounds.Count;
            yield return new WaitForSeconds(config.starSwitchInterval);
        }
    }
    #endregion
}
