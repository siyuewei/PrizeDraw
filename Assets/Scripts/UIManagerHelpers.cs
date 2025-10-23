using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// UI管理器 - 辅助方法部分
/// </summary>
public partial class UIManager
{
    #region 辅助方法
    /// <summary>
    /// 清除视频播放器的 RenderTexture，避免显示上一次播放的残留画面
    /// </summary>
    /// <param name="videoPlayer">要清除的视频播放器</param>
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
    
    /// <summary>
    /// 检测抽奖动画播放进度
    /// </summary>
    private void CheckDrawingAnimationProgress()
    {
        // 只在抽奖状态下检测
        if (GameLogicHandler.Instance?.CurrentState != GameState.Drawing)
        {
            return;
        }
        
        // 如果已经通知过，就不再检测
        if (hasNotifiedDrawingComplete)
        {
            return;
        }
        
        // 检查视频播放器和视频片段是否有效
        if (videoPlayerForPlayer == null || videoPlayerForPlayer.clip == null)
        {
            return;
        }
        
        // 检查是否正在播放
        if (!videoPlayerForPlayer.isPlaying)
        {
            return;
        }
        
        // 计算当前播放进度百分比
        double currentTime = videoPlayerForPlayer.time;
        double totalTime = videoPlayerForPlayer.clip.length;

        
        if (totalTime <= 0)
        {
            return;
        }
        
        float currentProgress = (float)(currentTime / totalTime);
        
        // 如果播放进度达到或超过设定的百分比，触发状态转换
        if (currentProgress >= percentOfPlayerRunClip)
        {
            Debug.Log($"[UI] 抽奖动画已播放到 {currentProgress * 100:F1}%（设定值：{percentOfPlayerRunClip * 100:F1}%），准备显示结果");
            hasNotifiedDrawingComplete = true;
            videoPlayerForPlayer.sendFrameReadyEvents = false;
            GameEvents.NotifyReadyToShowResult();
        }
    }
    
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
        if (prizeResultTextMeshPro != null)
        {
            prizeResultTextMeshPro.text = "";
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
        
        // 隐藏并停止抽奖背景
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
            
            // 清除 RenderTexture，避免下次播放时显示残留画面
            ClearVideoPlayerRenderTexture(videoPlayerForPlayerRunBackground);
        }
        
        // 更新星星特效到当前奖项
        int currentPrizeIndex = GameLogicHandler.Instance?.CurrentPrizeIndex ?? 1;
        UpdateTwinkleEffects(currentPrizeIndex);
    }
    #endregion
}

