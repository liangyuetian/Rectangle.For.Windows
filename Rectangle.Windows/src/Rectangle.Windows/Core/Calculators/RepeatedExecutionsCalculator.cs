using System.Collections.Generic;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 重复执行计算器：处理连续按同一快捷键时的循环行为
/// </summary>
public static class RepeatedExecutionsCalculator
{
    /// <summary>
    /// 三分之一循环序列：FirstThird → CenterThird → LastThird → FirstThird
    /// </summary>
    private static readonly WindowAction[] ThirdsCycle = 
    {
        WindowAction.FirstThird,
        WindowAction.CenterThird,
        WindowAction.LastThird
    };

    /// <summary>
    /// 三分之二循环序列：FirstTwoThirds → CenterTwoThirds → LastTwoThirds → FirstTwoThirds
    /// </summary>
    private static readonly WindowAction[] TwoThirdsCycle = 
    {
        WindowAction.FirstTwoThirds,
        WindowAction.CenterTwoThirds,
        WindowAction.LastTwoThirds
    };

    /// <summary>
    /// 左右半屏循环序列：LeftHalf → RightHalf → LeftHalf
    /// </summary>
    private static readonly WindowAction[] LeftRightCycle = 
    {
        WindowAction.LeftHalf,
        WindowAction.RightHalf
    };

    /// <summary>
    /// 上下半屏循环序列：TopHalf → BottomHalf → TopHalf
    /// </summary>
    private static readonly WindowAction[] TopBottomCycle = 
    {
        WindowAction.TopHalf,
        WindowAction.BottomHalf
    };

    /// <summary>
    /// 四等分循环序列：FirstFourth → SecondFourth → ThirdFourth → LastFourth → FirstFourth
    /// </summary>
    private static readonly WindowAction[] FourthsCycle = 
    {
        WindowAction.FirstFourth,
        WindowAction.SecondFourth,
        WindowAction.ThirdFourth,
        WindowAction.LastFourth
    };

    /// <summary>
    /// 四分之三循环序列：FirstThreeFourths → CenterThreeFourths → LastThreeFourths → FirstThreeFourths
    /// </summary>
    private static readonly WindowAction[] ThreeFourthsCycle = 
    {
        WindowAction.FirstThreeFourths,
        WindowAction.CenterThreeFourths,
        WindowAction.LastThreeFourths
    };

    /// <summary>
    /// 所有循环组的映射
    /// </summary>
    private static readonly Dictionary<WindowAction, WindowAction[]> CycleGroups = new()
    {
        // 三分之一循环组
        { WindowAction.FirstThird, ThirdsCycle },
        { WindowAction.CenterThird, ThirdsCycle },
        { WindowAction.LastThird, ThirdsCycle },
        
        // 三分之二循环组
        { WindowAction.FirstTwoThirds, TwoThirdsCycle },
        { WindowAction.CenterTwoThirds, TwoThirdsCycle },
        { WindowAction.LastTwoThirds, TwoThirdsCycle },
        
        // 左右半屏循环组
        { WindowAction.LeftHalf, LeftRightCycle },
        { WindowAction.RightHalf, LeftRightCycle },
        
        // 上下半屏循环组
        { WindowAction.TopHalf, TopBottomCycle },
        { WindowAction.BottomHalf, TopBottomCycle },
        
        // 四等分循环组
        { WindowAction.FirstFourth, FourthsCycle },
        { WindowAction.SecondFourth, FourthsCycle },
        { WindowAction.ThirdFourth, FourthsCycle },
        { WindowAction.LastFourth, FourthsCycle },
        
        // 四分之三循环组
        { WindowAction.FirstThreeFourths, ThreeFourthsCycle },
        { WindowAction.CenterThreeFourths, ThreeFourthsCycle },
        { WindowAction.LastThreeFourths, ThreeFourthsCycle },
    };

    /// <summary>
    /// 检查操作是否支持循环
    /// </summary>
    public static bool SupportsCycle(WindowAction action)
    {
        return CycleGroups.ContainsKey(action);
    }

    /// <summary>
    /// 获取下一个循环操作
    /// </summary>
    /// <param name="currentAction">当前操作</param>
    /// <param name="executionCount">当前操作已执行次数</param>
    /// <returns>下一个应该执行的操作，如果不支持循环则返回原操作</returns>
    public static WindowAction GetNextCycleAction(WindowAction currentAction, int executionCount)
    {
        if (!CycleGroups.TryGetValue(currentAction, out var cycle))
        {
            // 不支持循环，返回原操作
            return currentAction;
        }

        // 找到当前操作在循环中的位置
        int currentIndex = -1;
        for (int i = 0; i < cycle.Length; i++)
        {
            if (cycle[i] == currentAction)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex == -1)
        {
            // 不应该发生，返回原操作
            return currentAction;
        }

        // 计算下一个操作的索引（循环）
        // executionCount 从 1 开始，所以第 2 次执行时切换到下一个
        int nextIndex = (currentIndex + executionCount - 1) % cycle.Length;
        
        return cycle[nextIndex];
    }

    /// <summary>
    /// 获取循环组中的所有操作
    /// </summary>
    public static WindowAction[] GetCycleGroup(WindowAction action)
    {
        return CycleGroups.TryGetValue(action, out var cycle) ? cycle : new[] { action };
    }

    /// <summary>
    /// 检查两个操作是否在同一个循环组中
    /// </summary>
    public static bool InSameCycleGroup(WindowAction action1, WindowAction action2)
    {
        if (!CycleGroups.TryGetValue(action1, out var cycle1))
            return false;
        
        if (!CycleGroups.TryGetValue(action2, out var cycle2))
            return false;
        
        return cycle1 == cycle2;
    }
}
