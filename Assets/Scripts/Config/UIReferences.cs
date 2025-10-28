using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// UI引用 - 存储场景中UI元素的引用
/// 这些是场景中的实际GameObject，需要在Inspector中设置
/// </summary>
public class UIReferences : MonoBehaviour
{
    [Header("配置资产")]
    [Tooltip("UI配置ScriptableObject，存储所有资产引用")]
    public UIConfig config;
    
    [Header("星星特效 - 场景对象")]
    [Tooltip("索引0对应一等奖，索引1对应二等奖，以此类推")]
    public List<GameObject> twinkleEffects = new List<GameObject>();
    
    [Header("视频播放器")]
    public VideoPlayer videoPlayerForPlayer;
    public VideoPlayer videoPlayerForPlayerRunBackground;
    public VideoPlayer curtainVideoPlayer;
    
    [Header("游戏对象")]
    public GameObject playerRunBackgroundObject;
    public GameObject curtainPanel;
    public GameObject prizeResultPanel;
    
    [Header("UI组件")]
    public TextMeshProUGUI prizeResultTextMeshPro;
    public TextMeshProUGUI mustListIndexTextMeshPro;
    public Image prizeResultImage;
    public Image prizeResultBackgroundImage;
    
    #region 验证
    private void OnValidate()
    {
        if (config == null)
        {
            Debug.LogWarning("[UIReferences] 未设置UI配置文件！请在Inspector中指定UIConfig资产。");
        }
        
        if (twinkleEffects.Count != 4)
        {
            Debug.LogWarning($"[UIReferences] 星星特效数量应该是4个（对应4个奖项），当前是{twinkleEffects.Count}个");
        }
    }
    #endregion
}

