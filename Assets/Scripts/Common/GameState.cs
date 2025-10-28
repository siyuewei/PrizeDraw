/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    /// <summary>等待抽奖：准备状态，等待用户选择奖项并开始抽奖</summary>
    WaitingForDraw,
    
    /// <summary>抽奖进行中：正在播放抽奖动画</summary>
    DrawingInProgress,
    
    /// <summary>显示中奖者：显示中奖结果，等待用户重启</summary>
    ShowingWinner,
    
    /// <summary>过渡到下一轮：播放过渡动画，准备进入下一轮抽奖</summary>
    TransitionToNext
}

