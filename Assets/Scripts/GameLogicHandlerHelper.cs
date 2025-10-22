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
        commonAvailablePeopleCount = CheckCommonAvailablePeopleCount();
    }

    // 检查可用人数
    private int CheckCommonAvailablePeopleCount()
    {
        // 检查配置逻辑
        if (configData.commonMinPeopleIndex > configData.commonMaxPeopleIndex)
        {
            Debug.LogError("配置错误：最小编号大于最大编号。请检查 config.json。");
            return 0;
        }
        
        // 计算总人数
        int totalPeople = configData.commonMaxPeopleIndex - configData.commonMinPeopleIndex + 1;

        // 计算可用人数
        int availableCount = 0;
        for (int i = configData.commonMinPeopleIndex; i <= configData.commonMaxPeopleIndex; i++)
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
                        if (blackListNumber >= configData.commonMinPeopleIndex && blackListNumber <= configData.commonMaxPeopleIndex)
                        {
                            blackList.Add(blackListNumber);
                        }
                        else
                        {
                            Debug.LogWarning($"黑名单编号 {blackListNumber} 超出当前配置的人员范围 ({configData.commonMinPeopleIndex} - {configData.commonMaxPeopleIndex})，已忽略。");
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
    
    /// <summary>
    /// 读取必中榜单文件
    /// </summary>
    List<int> ReadMustWinListFile()
    {
        List<int> mustWinList = new List<int>();
        
        try
        {
            if (File.Exists(mustWinListFilePath))
            {
                // 使用 File.ReadAllLines 读取所有行
                string[] lines = File.ReadAllLines(mustWinListFilePath);
                
                foreach (string line in lines)
                {
                    // 忽略空行或只包含空格的行
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;
                    
                    if (int.TryParse(trimmedLine, out int mustWinNumber))
                    {
                        mustWinList.Add(mustWinNumber);
                    }
                    else
                    {
                        Debug.LogWarning($"必中榜单文件包含非数字行：'{line}'，已跳过。");
                    }
                }
                
                Debug.Log($"必中榜单读取成功，共 {mustWinList.Count} 人。");
            }
            else
            {
                Debug.Log($"必中榜单文件不存在：{mustWinListFilePath}，将进行正常抽奖。");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"读取必中榜单文件失败：{e.Message}");
        }
        
        return mustWinList;
    }
    
    void ReadConfigFile()
    {
        try
        {
            if (File.Exists(configFilePath))
            {
                string jsonString = File.ReadAllText(configFilePath);
                ConfigData loadedConfig = JsonUtility.FromJson<ConfigData>(jsonString);
                
                if (loadedConfig != null)
                {
                    configData = loadedConfig;
                    Debug.Log($"配置文件读取成功，人员编号范围：{configData.commonMinPeopleIndex} - {configData.commonMaxPeopleIndex}");
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
                    commonWinnerIndices.Clear();
                    specialWinnerIndices.Clear();
                    foreach (var entry in drawResultData.prizeWinners)
                    {
                        if (entry.winnerIds != null)
                        {
                            foreach (int winnerId in entry.winnerIds)
                            {
                                if (entry.prizeIndex == configData.specialPrizeIndex)
                                {
                                    specialWinnerIndices.Add(winnerId);
                                }
                                else
                                {
                                    commonWinnerIndices.Add(winnerId);
                                }
                            }
                        }
                    }
                    
                    Debug.Log($"抽奖结果加载成功。普通奖已中奖人数: {commonWinnerIndices.Count}，特别奖已中奖人数: {specialWinnerIndices.Count}");
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
        commonWinnerIndices.Clear();
        specialWinnerIndices.Clear();
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
        // 判断当前是否是特别奖
        bool isSpecialPrize = (CurrentPrizeIndex == configData.specialPrizeIndex);
        
        if (isSpecialPrize)
        {
            // 特别奖抽奖逻辑
            DrawSpecialPrize();
        }
        else
        {
            // 普通奖抽奖逻辑
            DrawCommonPrize();
        }
    }
    
    /// <summary>
    /// 特别奖抽奖逻辑
    /// </summary>
    void DrawSpecialPrize()
    {
        // 计算特别奖可用人数
        int specialTotalPeople = configData.specialMaxPeopleIndex - configData.specialMinPeopleIndex + 1;
        int specialAvailableCount = specialTotalPeople - specialWinnerIndices.Count;
        
        if (specialAvailableCount <= 0)
        {
            Debug.LogWarning("所有特别奖人员都已被抽中，无法继续抽奖。");
            return;
        }
        
        int drawnIndex;
        int attemptCount = 0;
        const int maxAttempts = 10000;
        
        do
        {
            // 在特别奖范围内随机抽取
            drawnIndex = Random.Range(configData.specialMinPeopleIndex, configData.specialMaxPeopleIndex + 1);
            attemptCount++;
            
            if (attemptCount > maxAttempts)
            {
                Debug.LogError("特别奖抽奖尝试次数过多，可能存在逻辑错误或配置问题。已停止抽奖。");
                return;
            }
            
        } while (specialWinnerIndices.Contains(drawnIndex)); // 只检查特别奖的中奖记录，不考虑黑名单
        
        LastWinnerID = drawnIndex;
        // 将中奖人添加到特别奖中奖集合
        specialWinnerIndices.Add(LastWinnerID);
        
        // 将中奖人添加到对应奖项的列表中
        PrizeWinnerEntry existingEntry = drawResultData.prizeWinners.Find(entry => entry.prizeIndex == CurrentPrizeIndex);
        if (existingEntry != null)
        {
            existingEntry.winnerIds.Add(LastWinnerID);
        }
        else
        {
            drawResultData.prizeWinners.Add(new PrizeWinnerEntry 
            { 
                prizeIndex = CurrentPrizeIndex, 
                winnerIds = new List<int> { LastWinnerID } 
            });
        }
        
        string logString = $"抽奖键按下，当前奖项为特别奖（第 {CurrentPrizeIndex} 等奖），中奖号码为：{LastWinnerID}。 " +
                     $"特别奖已中奖人数：{specialWinnerIndices.Count}";
        Debug.Log(logString);
        
        // 保存抽奖结果
        SaveDrawResult();
    }
    
    /// <summary>
    /// 普通奖抽奖逻辑
    /// </summary>
    void DrawCommonPrize()
    {
        if (commonAvailablePeopleCount <= 0)
        {
            Debug.LogWarning("没有可用人数，无法进行抽奖。");
            return;
        }

        int totalPossibleDraws = commonAvailablePeopleCount - commonWinnerIndices.Count;

        if (totalPossibleDraws <= 0)
        {
            Debug.LogWarning("所有符合条件的人员都已被抽中，无法继续抽奖。");
            return;
        }

        // 每次抽奖都重新读取必中榜单
        List<int> mustWinList = ReadMustWinListFile();
        
        int drawnIndex = -1;
        bool drawnFromMustWinList = false;
        
        // 先尝试从必中榜单中抽取
        if (mustWinList.Count > 0)
        {
            // 筛选出符合条件的必中人员：在index范围内、未中奖、不在黑名单中
            List<int> validMustWinList = new List<int>();
            foreach (int mustWinNumber in mustWinList)
            {
                if (mustWinNumber >= configData.commonMinPeopleIndex && 
                    mustWinNumber <= configData.commonMaxPeopleIndex &&
                    !commonWinnerIndices.Contains(mustWinNumber) &&
                    !blackList.Contains(mustWinNumber))
                {
                    validMustWinList.Add(mustWinNumber);
                }
            }
            
            // 如果有符合条件的必中人员，从中随机抽取一个
            if (validMustWinList.Count > 0)
            {
                int randomIndex = Random.Range(0, validMustWinList.Count);
                drawnIndex = validMustWinList[randomIndex];
                drawnFromMustWinList = true;
                Debug.Log($"从必中榜单中抽取，可选人数：{validMustWinList.Count}");
            }
            else
            {
                Debug.Log("必中榜单中没有符合条件的人员，将进行正常抽奖。");
            }
        }
        
        // 如果必中榜单没有符合条件的人员，则进行正常抽奖
        if (drawnIndex == -1)
        {
            int attemptCount = 0; 
            const int maxAttempts = 10000;

            do
            {
                // 在普通奖范围内随机抽取
                drawnIndex = Random.Range(configData.commonMinPeopleIndex, configData.commonMaxPeopleIndex + 1);
                attemptCount++;

                if (attemptCount > maxAttempts)
                {
                    Debug.LogError("抽奖尝试次数过多，可能存在逻辑错误或配置问题。已停止抽奖。");
                    return;
                }
                
            } while (commonWinnerIndices.Contains(drawnIndex) || blackList.Contains(drawnIndex)); // 检查普通奖中奖记录和黑名单
        }

        LastWinnerID = drawnIndex;
        // 将中奖人添加到普通奖中奖集合
        commonWinnerIndices.Add(LastWinnerID);
        
        // 将中奖人添加到对应奖项的列表中
        PrizeWinnerEntry existingEntry = drawResultData.prizeWinners.Find(entry => entry.prizeIndex == CurrentPrizeIndex);
        if (existingEntry != null)
        {
            existingEntry.winnerIds.Add(LastWinnerID);
        }
        else
        {
            drawResultData.prizeWinners.Add(new PrizeWinnerEntry 
            { 
                prizeIndex = CurrentPrizeIndex, 
                winnerIds = new List<int> { LastWinnerID } 
            });
        }
        
        string drawSource = drawnFromMustWinList ? "【必中榜单】" : "【正常抽奖】";
        string logString = $"{drawSource}抽奖键按下，当前奖项为第 {CurrentPrizeIndex} 等奖，中奖号码为：{LastWinnerID}。 " +
                     $"普通奖已中奖人数：{commonWinnerIndices.Count}";
        Debug.Log(logString);
        
        // 保存抽奖结果
        SaveDrawResult();
    }
}


