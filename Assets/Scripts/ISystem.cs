/// <summary>
/// 系统接口 - 所有系统都需要实现此接口
/// </summary>
public interface ISystem
{
    /// <summary>
    /// 系统初始化
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// 系统清理
    /// </summary>
    void Cleanup();
}

