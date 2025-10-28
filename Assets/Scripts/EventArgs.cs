using System;
using UnityEngine;

/// <summary>
/// 事件参数基类 - 所有事件参数必须继承此类
/// </summary>
[Serializable]
public abstract class EventArgBase { }

/// <summary>
/// 空事件参数 - 用于无参数的事件
/// </summary>
[Serializable]
public sealed class EmptyEventArg : EventArgBase 
{
    public static readonly EmptyEventArg Instance = new EmptyEventArg();
}

/// <summary>
/// 整数事件参数
/// </summary>
[Serializable]
public class IntEventArg : EventArgBase
{
    public int value;
    
    public IntEventArg() { }
    public IntEventArg(int value) { this.value = value; }
}

/// <summary>
/// 游戏状态事件参数
/// </summary>
[Serializable]
public class GameStateEventArg : EventArgBase
{
    public GameState state;
    
    public GameStateEventArg() { }
    public GameStateEventArg(GameState state) { this.state = state; }
}

/// <summary>
/// 字符串事件参数
/// </summary>
[Serializable]
public class StringEventArg : EventArgBase
{
    public string value;
    
    public StringEventArg() { }
    public StringEventArg(string value) { this.value = value; }
}

/// <summary>
/// 浮点数事件参数
/// </summary>
[Serializable]
public class FloatEventArg : EventArgBase
{
    public float value;
    
    public FloatEventArg() { }
    public FloatEventArg(float value) { this.value = value; }
}

/// <summary>
/// 布尔事件参数
/// </summary>
[Serializable]
public class BoolEventArg : EventArgBase
{
    public bool value;
    
    public BoolEventArg() { }
    public BoolEventArg(bool value) { this.value = value; }
}

