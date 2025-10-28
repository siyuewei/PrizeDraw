/// 
/// 事件ID枚举 - 定义所有游戏事件
/// 使用事件ID可以直接在代码中搜索事件的发布和订阅位置
/// 
public enum EventId
{
    // ========== 游戏状态事件 ==========
    // 游戏状态改变
    GameStateChanged,
    
    // ========== 抽奖相关事件 ==========
    // 请求执行抽奖
    PrizeDrawRequested,
    
    // 请求切换奖项
    PrizeIndexChangeRequested,
    
    // 奖项索引已更新（验证通过）
    PrizeIndexUpdated,
    
    // ========== 配置相关事件 ==========
    // 请求重新加载配置
    ReloadConfigRequested,
    
    // ========== UI相关事件 ==========
    // 抽奖动画已就绪，准备显示结果
    ReadyToShowResult,
    
    // 过渡动画完成
    TransitionComplete,
    
    // 请求重启（进入下一轮抽奖）
    RestartRequested,
    
    // ========== 必中榜单相关事件 ==========
    // 请求切换必中榜单索引
    ChangeMustListIndex,
    
    /// 必中榜单索引已改变
    MustListIndexChanged,
}

