/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    Idle,           // 待机状态：等待选择奖项和抽奖
    Drawing,        // 抽奖中：播放抽奖动画
    ShowingResult,  // 显示结果：显示中奖结果，等待重启
    Transitioning   // 过渡中：播放过渡动画，准备回到待机
}

