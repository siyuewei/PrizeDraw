using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.IO;

/// <summary>
/// 游戏逻辑处理器 - 数据处理和抽奖逻辑部分
/// </summary>
public partial class GameLogicHandler
{
    void ReadFiles()
    {
        ReadConfigFile();
        ReadBlackListFile();
        LoadDrawResult();
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
    
    /// <summary>
    /// 保存抽奖结果到文件
    /// </summary>
    void SaveDrawResult()
    {
        string json = JsonUtility.ToJson(drawResultData, true);
        File.WriteAllText(drawResultFilePath, json);
    }
    
    /// <summary>
    /// 从文件加载抽奖结果
    /// </summary>
    void LoadDrawResult()
    {
        if (File.Exists(drawResultFilePath))
        {
            try
            {
                string jsonString = File.ReadAllText(drawResultFilePath);
                DrawResultData loadedData = JsonUtility.FromJson<DrawResultData>(jsonString);
                if (loadedData != null && loadedData.prizeWinners != null)
                {
                    drawResultData = loadedData;
                    
                    // 重建drawnPeopleIndices集合
                    drawnPeopleIndices.Clear();
                    foreach (var entry in drawResultData.prizeWinners)
                    {
                        if (entry.winnerIds != null)
                        {
                            foreach (int winnerId in entry.winnerIds)
                            {
                                drawnPeopleIndices.Add(winnerId);
                            }
                        }
                    }
                    
                    Debug.Log($"抽奖结果加载成功。已中奖人数: {drawnPeopleIndices.Count}");
                }
                else
                {
                    Debug.LogWarning("抽奖结果文件为空或格式错误，将从初始状态开始。");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载抽奖结果失败：{e.Message}。将从初始状态开始。");
            }
        }
        else
        {
           Debug.Log("未找到抽奖结果文件，将从初始状态开始。");
        }
    }
    
    /// <summary>
    /// 清除抽奖历史记录
    /// </summary>
    public void ClearDrawHistory()
    {
        // 只在待机状态下才允许清除历史
        if (CurrentState != GameState.Idle)
        {
            Debug.LogWarning($"当前状态为 {CurrentState}，无法清除抽奖历史");
            return;
        }
        
        // 清除已中奖人员列表和中奖记录
        drawnPeopleIndices.Clear();
        drawResultData.prizeWinners.Clear();
        
        // 删除抽奖结果文件
        try
        {
            if (File.Exists(drawResultFilePath))
            {
                File.Delete(drawResultFilePath);
            }

            Debug.Log("抽奖历史已清除");
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"清除抽奖历史失败：{e.Message}");
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
        
        // 将中奖人添加到对应奖项的列表中
        // 查找是否已有该奖项的entry
        PrizeWinnerEntry existingEntry = drawResultData.prizeWinners.Find(entry => entry.prizeIndex == CurrentPrizeIndex);
        if (existingEntry != null)
        {
            // 如果已存在，就添加到现有的winnerIds列表中
            existingEntry.winnerIds.Add(LastWinnerID);
        }
        else
        {
            // 如果不存在，创建新的entry
            drawResultData.prizeWinners.Add(new PrizeWinnerEntry 
            { 
                prizeIndex = CurrentPrizeIndex, 
                winnerIds = new List<int> { LastWinnerID } 
            });
        }
        
        string logString = $"抽奖键按下，当前奖项为第 {CurrentPrizeIndex} 等奖，中奖号码为：{LastWinnerID}。 " +
                     $"已中奖人数：{drawnPeopleIndices.Count}";
        Debug.Log(logString);
        
        // 保存抽奖结果
        SaveDrawResult();
    }
}

