using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.IO;
using Sirenix.OdinInspector;

/// <summary>
/// 游戏逻辑处理器 - 负责订阅事件和处理抽奖逻辑
/// </summary>
public class GameLogicHandler : MonoBehaviour
{
    public static GameLogicHandler Instance { get; private set; }
    
    // 使用 [System.Serializable] 标记，确保 JsonUtility 可以正确序列化/反序列化
    [System.Serializable]
    private class ConfigData
    {
        public int minPeopleIndex = 1; // 提供默认值
        public int maxPeopleIndex = 100; // 提供默认值
    }
    
    // 状态变量
    public int CurrentPrizeIndex { get; private set; } = 1;
    public int LastWinnerID = 0;
    
    // 配置变量
    private int minPeopleIndex = 1;
    private int maxPeopleIndex = 100;
    
    // 存储已中奖的人的编号，使用HashSet提高查找效率
    private readonly HashSet<int> drawnPeopleIndices = new HashSet<int>();
    
    // 将黑名单改为 HashSet，提高查找效率
    private readonly HashSet<int> blackList = new HashSet<int>();

    // 文件路径
    private string blackListFilePath;
    private string configFilePath;
    
    private int availablePeopleCount = 0; // 可用人数
    
    [Button("Prize Draw")]
    void PrizeDrawButton()
    {
        PrizeDraw();
    }

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 统一设置文件路径
        blackListFilePath = Path.Combine(Application.dataPath, "blacklist.txt");
        configFilePath = Path.Combine(Application.dataPath, "config.json");
        
        ReadFiles();
        
        Debug.Log($"抽奖系统启动。人员范围: {minPeopleIndex} - {maxPeopleIndex}。黑名单人数: {blackList.Count}。");
        
        // 首次检查可用人数
        CheckAvailablePeopleCount();
        
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
        GameEvents.OnPrizeDrawRequested += HandlePrizeDrawRequested;
        GameEvents.OnPrizeIndexChanged += HandlePrizeIndexChanged;
        GameEvents.OnReloadConfigRequested += HandleReloadConfigRequested;
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnPrizeDrawRequested -= HandlePrizeDrawRequested;
        GameEvents.OnPrizeIndexChanged -= HandlePrizeIndexChanged;
        GameEvents.OnReloadConfigRequested -= HandleReloadConfigRequested;
    }
    
    /// <summary>
    /// 处理抽奖请求
    /// </summary>
    private void HandlePrizeDrawRequested(int prizeIndex)
    {
        PrizeDraw();
    }
    
    /// <summary>
    /// 处理奖项索引变更
    /// </summary>
    private void HandlePrizeIndexChanged(int prizeIndex)
    {
        CurrentPrizeIndex = prizeIndex;
    }
    
    /// <summary>
    /// 处理重新加载配置请求
    /// </summary>
    private void HandleReloadConfigRequested()
    {
        ReadFiles();
    }
    
    void ReadFiles()
    {
        ReadConfigFile();
        ReadBlackListFile();
        availablePeopleCount = CheckAvailablePeopleCount();
    }
    
    // 检查可用人数
    private int CheckAvailablePeopleCount()
    {
        // 检查配置逻辑
        if (minPeopleIndex > maxPeopleIndex)
        {
            Debug.LogError("配置错误：最小编号大于最大编号。请检查 config.json。");
            return 0;
        }
        
        // 计算总人数
        int totalPeople = maxPeopleIndex - minPeopleIndex + 1;
        
        // 计算可用人数
        int availableCount = 0;
        for (int i = minPeopleIndex; i <= maxPeopleIndex; i++)
        {
            if (!blackList.Contains(i))
            {
                availableCount++;
            }
        }
        
        if (availableCount <= 0)
        {
            Debug.LogWarning("注意：可参与抽奖的人数（非黑名单且在范围内）为 0！无法进行抽奖。");
        }
        
        return availableCount;
    }

    void ReadBlackListFile()
    {
        blackList.Clear(); // 清空，确保每次读取都是最新的
        
        try
        {
            if (File.Exists(blackListFilePath))
            {
                // 使用 File.ReadAllLines 读取所有行
                string[] lines = File.ReadAllLines(blackListFilePath);
                
                foreach (string line in lines)
                {
                    // 忽略空行或只包含空格的行
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;
                    
                    if (int.TryParse(trimmedLine, out int blackListNumber))
                    {
                        // 仅添加在合法范围内的黑名单编号
                        if (blackListNumber >= minPeopleIndex && blackListNumber <= maxPeopleIndex)
                        {
                            blackList.Add(blackListNumber);
                        }
                        else
                        {
                            Debug.LogWarning($"黑名单编号 {blackListNumber} 超出当前配置的人员范围 ({minPeopleIndex} - {maxPeopleIndex})，已忽略。");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"黑名单文件包含非数字行：'{line}'，已跳过。");
                    }
                }
            }
            else
            {
                // 创建黑名单文件，使用 File.WriteAllText(..., "") 替代 File.Create()
                File.WriteAllText(blackListFilePath, "");
                Debug.Log($"黑名单文件不存在，已创建：{blackListFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"读取黑名单文件失败：{e.Message}");
        }
    }
    
    void ReadConfigFile()
    {
        ConfigData configData = new ConfigData(); // 初始化一个带有默认值的对象
        
        try
        {
            if (File.Exists(configFilePath))
            {
                string jsonString = File.ReadAllText(configFilePath);
                ConfigData loadedConfig = JsonUtility.FromJson<ConfigData>(jsonString);
                
                if (loadedConfig != null)
                {
                    configData = loadedConfig;
                    minPeopleIndex = configData.minPeopleIndex;
                    maxPeopleIndex = configData.maxPeopleIndex;
                    Debug.Log($"配置文件读取成功，人员编号范围：{minPeopleIndex} - {maxPeopleIndex}");
                }
                else
                {
                    Debug.LogError("配置文件解析失败，将使用默认值。");
                }
            }
            else
            {
                // 创建默认配置文件
                string defaultJson = JsonUtility.ToJson(configData, true);
                File.WriteAllText(configFilePath, defaultJson);
                Debug.Log($"配置文件不存在，已创建默认文件：{configFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"读取配置文件失败：{e.Message}。使用默认值。");
        }
    }
    
    void PrizeDraw()
    {
        if (availablePeopleCount <= 0) return; // 没有可用人数，直接退出

        int totalPossibleDraws = availablePeopleCount - drawnPeopleIndices.Count;

        if (totalPossibleDraws <= 0)
        {
            Debug.LogWarning("所有符合条件的人员都已被抽中，无法继续抽奖。");
            return;
        }

        int drawnIndex;
        // 添加计数器，防止在极端情况下进入死循环
        int attemptCount = 0; 
        const int maxAttempts = 10000; // 设置最大尝试次数

        do
        {
            // 在 [minPeopleIndex, maxPeopleIndex] 范围内随机抽取一个人
            // Random.Range(int min, int max) 是包含 min 但不包含 max 的，因此需要 +1
            drawnIndex = Random.Range(minPeopleIndex, maxPeopleIndex + 1);
            attemptCount++;

            // 检查是否超出尝试次数（尽管逻辑上不应发生，但这是防御性编程）
            if (attemptCount > maxAttempts)
            {
                Debug.LogError("抽奖尝试次数过多，可能存在逻辑错误或配置问题。已停止抽奖。");
                return;
            }
            
        } while (drawnPeopleIndices.Contains(drawnIndex) || blackList.Contains(drawnIndex));

        LastWinnerID = drawnIndex;
        // 将中奖人添加到已中奖集合
        drawnPeopleIndices.Add(LastWinnerID);
        
        string logString = $"抽奖键按下，当前奖项为第 {CurrentPrizeIndex} 等奖，中奖号码为：{LastWinnerID}。 " +
                     $"已中奖人数：{drawnPeopleIndices.Count}";
        Debug.Log(logString);
    }
}
