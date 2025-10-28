using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 系统管理器 - 负责管理所有系统的注册和生命周期
/// </summary>
public class SystemManager : MonoBehaviour
{
    public static SystemManager Instance { get; private set; }
    
    // 存储所有注册的系统
    private readonly List<ISystem> systems = new List<ISystem>();
    
    // 系统引用（方便访问）
    public MConfigSystem MConfig { get; private set; }
    public MEventSystem Events { get; private set; }
    public MGameLogicSystem MGameLogic { get; private set; }
    public MInputSystem MInput { get; private set; }
    public MUISystem Mui { get; private set; }
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        // 清理所有系统
        CleanupSystems();
    }
    
    /// <summary>
    /// 初始化所有系统
    /// </summary>
    private void InitializeSystems()
    {
        Debug.Log("[SystemManager] 开始初始化所有系统...");
        
        // 注意：系统初始化的顺序很重要
        // 1. 先初始化配置系统（其他系统可能需要配置）
        MConfig = RegisterSystem<MConfigSystem>();
        
        // 2. 初始化事件系统
        Events = RegisterSystem<MEventSystem>();
        
        // 3. 初始化游戏逻辑系统
        MGameLogic = RegisterSystem<MGameLogicSystem>();
        
        // 4. 初始化输入系统
        MInput = RegisterSystem<MInputSystem>();
        
        // 5. 初始化UI系统
        Mui = RegisterSystem<MUISystem>();
        
        Debug.Log($"[SystemManager] 系统初始化完成，共注册 {systems.Count} 个系统");
    }
    
    /// <summary>
    /// 注册并初始化一个系统
    /// </summary>
    private T RegisterSystem<T>() where T : MonoBehaviour, ISystem
    {
        // 检查是否已存在
        T existingSystem = GetComponent<T>();
        if (existingSystem != null)
        {
            Debug.LogWarning($"[SystemManager] 系统 {typeof(T).Name} 已存在，跳过注册");
            existingSystem.Initialize();
            return existingSystem;
        }
        
        // 添加组件并初始化
        T system = gameObject.AddComponent<T>();
        system.Initialize();
        systems.Add(system);
        
        Debug.Log($"[SystemManager] 注册系统: {typeof(T).Name}");
        return system;
    }
    
    /// <summary>
    /// 清理所有系统
    /// </summary>
    private void CleanupSystems()
    {
        Debug.Log("[SystemManager] 开始清理所有系统...");
        
        // 反向清理（后注册的先清理）
        for (int i = systems.Count - 1; i >= 0; i--)
        {
            systems[i].Cleanup();
        }
        
        systems.Clear();
        Debug.Log("[SystemManager] 系统清理完成");
    }
}


