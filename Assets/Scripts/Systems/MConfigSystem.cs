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
        Debug.Log("[MConfigSystem] 初始化配置系统...");
        
        // 设置文件路径
        blackListFilePath = Path.Combine(Application.dataPath, "blacklist.txt");
        configFilePath = Path.Combine(Application.dataPath, "config.json");
        drawResultFilePath = Path.Combine(Application.dataPath, "drawResult.json");
        
        // 读取所有配置文件
        LoadAllData();
        
        Debug.Log($"[MConfigSystem] 配置加载完成。人员范围: {Config.commonMinPeopleIndex}-{Config.commonMaxPeopleIndex}, " +
                  $"黑名单: {BlackList.Count}人, 已中奖: {CommonWinnerIndices.Count}人");
    }
    
    public void Cleanup()
    {
        Debug.Log("[MConfigSystem] 清理配置系统");
    }
    #endregion
    
    #region 数据加载
    public void LoadAllData()
    {
        ReadConfigFile();
        ReadBlackListFile();
        LoadDrawResult();
    }
    
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
                    Debug.Log($"[MConfigSystem] 配置文件读取成功");
                }
                else
                {
                    Debug.LogError("[MConfigSystem] 配置文件解析失败，使用默认值");
                }
            }
            else
            {
                string defaultJson = JsonUtility.ToJson(Config, true);
                File.WriteAllText(configFilePath, defaultJson);
                Debug.Log($"[MConfigSystem] 创建默认配置文件：{configFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MConfigSystem] 读取配置文件失败：{e.Message}");
        }
    }
    
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
                    }
                }
            }
            else
            {
                File.WriteAllText(blackListFilePath, "");
                Debug.Log($"[MConfigSystem] 创建黑名单文件：{blackListFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MConfigSystem] 读取黑名单失败：{e.Message}");
        }
    }
    
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

                    Debug.Log($"[MConfigSystem] 抽奖结果加载成功。普通奖: {CommonWinnerIndices.Count}人，特别奖: {SpecialWinnerIndices.Count}人");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MConfigSystem] 加载抽奖结果失败：{e.Message}");
            }
        }
    }
    #endregion
    
    #region 数据保存
    public void SaveDrawResult()
    {
        try
        {
            string json = JsonUtility.ToJson(DrawResult, true);
            File.WriteAllText(drawResultFilePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MConfigSystem] 保存抽奖结果失败：{e.Message}");
        }
    }
    #endregion
    
    #region 数据操作
    public void AddWinner(int prizeIndex, int winnerId)
    {
        if (prizeIndex == Config.specialPrizeIndex)
        {
            SpecialWinnerIndices.Add(winnerId);
        }
        else
        {
            CommonWinnerIndices.Add(winnerId);
        }
        
        var existingEntry = DrawResult.prizeWinners.Find(entry => entry.prizeIndex == prizeIndex);
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
        
        SaveDrawResult();
    }
    
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
            Debug.Log("[MConfigSystem] 抽奖历史已清除");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MConfigSystem] 清除抽奖历史失败：{e.Message}");
        }
    }
    
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
                }

                Debug.Log($"[MConfigSystem] 必中榜单读取成功，共 {mustWinList.Count} 人");
            }
            else
            {
                File.WriteAllText(mustWinListFilePath, "");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MConfigSystem] 读取必中榜单失败：{e.Message}");
        }

        return mustWinList;
    }
    
    public int GetCommonAvailablePeopleCount()
    {
        if (Config.commonMinPeopleIndex > Config.commonMaxPeopleIndex)
        {
            Debug.LogError("[MConfigSystem] 配置错误：最小编号大于最大编号");
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
