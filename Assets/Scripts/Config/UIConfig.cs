using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// UI配置 - 使用ScriptableObject管理UI资产
/// 在Project窗口右键 Create/PrizeDraw/UI Config 创建配置文件
/// </summary>
[CreateAssetMenu(fileName = "UIConfig", menuName = "PrizeDraw/UI Config", order = 1)]
public class UIConfig : ScriptableObject
{
    [Header("中奖结果背景图片")]
    [Tooltip("索引0对应一等奖，索引1对应二等奖，以此类推")]
    public List<Sprite> prizeResultBackgrounds = new List<Sprite>();

    [Header("螃蟹背景图片序列")]
    [Tooltip("用于三等奖的动画序列")]
    public List<Sprite> crabBackgrounds = new List<Sprite>();

    [Header("星星棒图片序列")]
    [Tooltip("用于二等奖的动画序列")]
    public List<Sprite> starBackgrounds = new List<Sprite>();

    [Header("中奖结果文字位置")]
    [Tooltip("每个奖项的文字位置偏移")]
    public List<Vector3> prizeResultRects = new List<Vector3>();
    
    [Header("中奖结果文字颜色")]
    [Tooltip("每个奖项的文字颜色")]
    public List<Color> prizeResultTextColors = new List<Color>();
    
    [Header("中奖结果背景图颜色")]
    [Tooltip("每个奖项的背景色")]
    public List<Color> prizeResultColors = new List<Color>();
    
    [Header("视频资源")]
    [Tooltip("人物待机视频")]
    public VideoClip playerIdleClip;
    
    [Tooltip("人物抽奖动画视频")]
    public VideoClip playerRunClip;
    
    [Tooltip("幕布关闭动画")]
    public VideoClip curtainCloseClip;
    
    [Tooltip("幕布打开动画")]
    public VideoClip curtainOpenClip;
    
    [Header("动画播放设置")]
    [Range(0, 1)]
    [Tooltip("抽奖动画播放到此百分比时显示结果")]
    public float percentOfPlayerRunClip = 0.5f;
    
    [Header("图片切换速度")]
    [Tooltip("螃蟹图片切换间隔（秒）")]
    public float crabSwitchInterval = 0.1f;
    
    [Tooltip("星星图片切换间隔（秒）")]
    public float starSwitchInterval = 0.5f;
    
    #region 验证
    private void OnValidate()
    {
        // 验证配置的合法性
        if (prizeResultBackgrounds.Count != 4)
        {
            Debug.LogWarning($"[UIConfig] 背景图片数量应该是4个，当前是{prizeResultBackgrounds.Count}个");
        }
        
        if (prizeResultRects.Count != 4)
        {
            Debug.LogWarning($"[UIConfig] 文字位置数量应该是4个，当前是{prizeResultRects.Count}个");
        }
        
        if (prizeResultTextColors.Count != 4)
        {
            Debug.LogWarning($"[UIConfig] 文字颜色数量应该是4个，当前是{prizeResultTextColors.Count}个");
        }
        
        if (prizeResultColors.Count != 4)
        {
            Debug.LogWarning($"[UIConfig] 背景颜色数量应该是4个，当前是{prizeResultColors.Count}个");
        }
    }
    #endregion
}

