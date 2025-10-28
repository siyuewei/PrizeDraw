using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 配置系统 - 负责配置文件、黑名单、抽奖结果的读写管理
/// </summary>
public class MConfigSystem : MonoBehaviour, ISystem
{
    #region 配置数据类
    [System.Serializable]
    public class ConfigData
    {
        public int commonMinPeopleIndex = 1;
        public int commonMaxPeopleIndex = 100;
        public int specialPrizeIndex = 2;
        public int specialMinPeopleIndex = 1;
        public int specialMaxPeopleIndex = 100;
    }
    
    [System.Serializable]
    public class PrizeWinnerEntry
    {
        public int prizeIndex;
        public List<int> winnerIds;
    }

    [System.Serializable]
    public class DrawResultData
    {
        public List<PrizeWinnerEntry> prizeWinners = new List<PrizeWinnerEntry>();
    }
    #endregion
    
    #region 公开属性
    public ConfigData Config { get; private set; } = new ConfigData();
    public DrawResultData DrawResult { get; private set; } = new DrawResultData();
    public HashSet<int> BlackList { get; private set; } = new HashSet<int>();
    public HashSet<int> CommonWinnerIndices { get; private set; } = new HashSet<int>();
    public HashSet<int> SpecialWinnerIndices { get; private set; } = new HashSet<int>();
    #endregion
    
    #region 文件路径
    private string blackListFilePath;
    private string configFilePath;
    private string drawResultFilePath;
    private string mustWinListFilePath;
    #endregion
    
    #region 系统接口实现
    public void Initialize()
    {
        Debug.Log("[ConfigSystem] 初始化配置系统...");
        
        // 设置文件路径
        blackListFilePath = Path.Combine(Application.dataPath, "blacklist.txt");
        configFilePath = Path.Combine(Application.dataPath, "config.json");
        drawResultFilePath = Path.Combine(Application.dataPath, "drawResult.json");
        
        // 读取所有配置文件
        LoadAllData();
        
        Debug.Log($"[ConfigSystem] 配置加载完成。人员范围: {Config.commonMinPeopleIndex}-{Config.commonMaxPeopleIndex}, " +
                  $"黑名单: {BlackList.Count}人, 已中奖: {CommonWinnerIndices.Count}人");
    }
    
    public void Cleanup()
    {
        Debug.Log("[ConfigSystem] 清理配置系统");
    }
    #endregion
    
    #region 数据加载
    /// <summary>
    /// 加载所有数据
    /// </summary>
    public void LoadAllData()
    {
        ReadConfigFile();
        ReadBlackListFile();
        LoadDrawResult();
    }
    
    /// <summary>
    /// 读取配置文件
    /// </summary>
    private void ReadConfigFile()
    {
        try
        {
            if (File.Exists(configFilePath))
            {
                string jsonString = File.ReadAllText(configFilePath);
                ConfigData loadedConfig = JsonUtility.FromJson<ConfigData>(jsonString);

                if (loadedConfig != null)
                {
                    Config = loadedConfig;
                    Debug.Log($"[ConfigSystem] 配置文件读取成功，人员编号范围：{Config.commonMinPeopleIndex} - {Config.commonMaxPeopleIndex}");
                }
                else
                {
                    Debug.LogError("[ConfigSystem] 配置文件解析失败，将使用默认值。");
                }
            }
            else
            {
                // 创建默认配置文件
                string defaultJson = JsonUtility.ToJson(Config, true);
                File.WriteAllText(configFilePath, defaultJson);
                Debug.Log($"[ConfigSystem] 配置文件不存在，已创建默认文件：{configFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConfigSystem] 读取配置文件失败：{e.Message}。使用默认值。");
        }
    }
    
    /// <summary>
    /// 读取黑名单文件
    /// </summary>
    private void ReadBlackListFile()
    {
        BlackList.Clear();

        try
        {
            if (File.Exists(blackListFilePath))
            {
                string[] lines = File.ReadAllLines(blackListFilePath);

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;

                    if (int.TryParse(trimmedLine, out int blackListNumber))
                    {
                        if (blackListNumber >= Config.commonMinPeopleIndex && 
                            blackListNumber <= Config.commonMaxPeopleIndex)
                        {
                            BlackList.Add(blackListNumber);
                        }
                        else
                        {
                            Debug.LogWarning($"[ConfigSystem] 黑名单编号 {blackListNumber} 超出范围，已忽略。");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[ConfigSystem] 黑名单包含非数字行：'{line}'，已跳过。");
                    }
                }
            }
            else
            {
                File.WriteAllText(blackListFilePath, "");
                Debug.Log($"[ConfigSystem] 黑名单文件不存在，已创建：{blackListFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConfigSystem] 读取黑名单文件失败：{e.Message}");
        }
    }
    
    /// <summary>
    /// 加载抽奖结果
    /// </summary>
    private void LoadDrawResult()
    {
        if (File.Exists(drawResultFilePath))
        {
            try
            {
                string jsonString = File.ReadAllText(drawResultFilePath);
                DrawResultData loadedData = JsonUtility.FromJson<DrawResultData>(jsonString);
                
                if (loadedData != null && loadedData.prizeWinners != null)
                {
                    DrawResult = loadedData;

                    // 重建中奖人员集合
                    CommonWinnerIndices.Clear();
                    SpecialWinnerIndices.Clear();
                    
                    foreach (var entry in DrawResult.prizeWinners)
                    {
                        if (entry.winnerIds != null)
                        {
                            foreach (int winnerId in entry.winnerIds)
                            {
                                if (entry.prizeIndex == Config.specialPrizeIndex)
                                {
                                    SpecialWinnerIndices.Add(winnerId);
                                }
                                else
                                {
                                    CommonWinnerIndices.Add(winnerId);
                                }
                            }
                        }
                    }

                    Debug.Log($"[ConfigSystem] 抽奖结果加载成功。普通奖: {CommonWinnerIndices.Count}人，特别奖: {SpecialWinnerIndices.Count}人");
                }
                else
                {
                    Debug.LogWarning("[ConfigSystem] 抽奖结果文件为空或格式错误。");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ConfigSystem] 加载抽奖结果失败：{e.Message}");
            }
        }
        else
        {
            Debug.Log("[ConfigSystem] 未找到抽奖结果文件，将从初始状态开始。");
        }
    }
    #endregion
    
    #region 数据保存
    /// <summary>
    /// 保存抽奖结果到文件
    /// </summary>
    public void SaveDrawResult()
    {
        try
        {
            string json = JsonUtility.ToJson(DrawResult, true);
            File.WriteAllText(drawResultFilePath, json);
            Debug.Log("[ConfigSystem] 抽奖结果已保存");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConfigSystem] 保存抽奖结果失败：{e.Message}");
        }
    }
    #endregion
    
    #region 数据操作
    /// <summary>
    /// 添加中奖者
    /// </summary>
    public void AddWinner(int prizeIndex, int winnerId)
    {
        // 添加到对应的中奖集合
        if (prizeIndex == Config.specialPrizeIndex)
        {
            SpecialWinnerIndices.Add(winnerId);
        }
        else
        {
            CommonWinnerIndices.Add(winnerId);
        }
        
        // 添加到抽奖结果数据
        PrizeWinnerEntry existingEntry = DrawResult.prizeWinners.Find(entry => entry.prizeIndex == prizeIndex);
        if (existingEntry != null)
        {
            existingEntry.winnerIds.Add(winnerId);
        }
        else
        {
            DrawResult.prizeWinners.Add(new PrizeWinnerEntry 
            { 
                prizeIndex = prizeIndex, 
                winnerIds = new List<int> { winnerId } 
            });
        }
        
        // 自动保存
        SaveDrawResult();
    }
    
    /// <summary>
    /// 清除抽奖历史
    /// </summary>
    public void ClearDrawHistory()
    {
        CommonWinnerIndices.Clear();
        SpecialWinnerIndices.Clear();
        DrawResult.prizeWinners.Clear();

        try
        {
            if (File.Exists(drawResultFilePath))
            {
                File.Delete(drawResultFilePath);
            }
            Debug.Log("[ConfigSystem] 抽奖历史已清除");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConfigSystem] 清除抽奖历史失败：{e.Message}");
        }
    }
    
    /// <summary>
    /// 读取必中榜单文件
    /// </summary>
    public List<int> ReadMustWinList(int mustListIndex)
    {
        List<int> mustWinList = new List<int>();

        if (mustListIndex == 0) return mustWinList;

        string mustFileName = "mustwinlist" + mustListIndex + ".txt";
        mustWinListFilePath = Path.Combine(Application.dataPath, mustFileName);

        try
        {
            if (File.Exists(mustWinListFilePath))
            {
                string[] lines = File.ReadAllLines(mustWinListFilePath);

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;

                    if (int.TryParse(trimmedLine, out int mustWinNumber))
                    {
                        mustWinList.Add(mustWinNumber);
                    }
                    else
                    {
                        Debug.LogWarning($"[ConfigSystem] 必中榜单包含非数字行：'{line}'，已跳过。");
                    }
                }

                Debug.Log($"[ConfigSystem] 必中榜单读取成功，共 {mustWinList.Count} 人。");
            }
            else
            {
                Debug.Log($"[ConfigSystem] 必中榜单文件不存在：{mustWinListFilePath}");
                File.WriteAllText(mustWinListFilePath, "");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConfigSystem] 读取必中榜单失败：{e.Message}");
        }

        return mustWinList;
    }
    
    /// <summary>
    /// 计算可用人数
    /// </summary>
    public int GetCommonAvailablePeopleCount()
    {
        if (Config.commonMinPeopleIndex > Config.commonMaxPeopleIndex)
        {
            Debug.LogError("[ConfigSystem] 配置错误：最小编号大于最大编号。");
            return 0;
        }

        int availableCount = 0;
        for (int i = Config.commonMinPeopleIndex; i <= Config.commonMaxPeopleIndex; i++)
        {
            if (!BlackList.Contains(i))
            {
                availableCount++;
            }
        }

        return availableCount;
    }
    #endregion
}

