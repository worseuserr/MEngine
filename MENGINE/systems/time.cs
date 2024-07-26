using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace MEngine;

public static class Time
{
    public static readonly Stopwatch Clock = new();

    private static List<MethodInfo> TimedMethods = new();

    /// <summary>
    /// Invokes a function every given interval using the main thread.
    /// </summary>
    /// <param name="msInterval">Interval in milliseconds.</param>
    /// <param name="function">Function to invoke. Must return a bool value: if <b>True</b>, ends loop; otherwise continues.</param>
    /// <remarks>Note: Function must return a bool value and the loop breaks if returned value is <b>True</b>.</remarks>
    public static void EachInterval(int msInterval, Func<bool> function)
    {
        TimedMethods.Add(new MethodInfo() { 
            interval = msInterval, 
            method = function, 
            lastInvoke = Clock.ElapsedMilliseconds,
            singleton = false
        });
        function();
    }

    /// <summary>
    /// Invokes a function once after an interval using the main thread.
    /// </summary>
    /// <param name="msInterval">Interval in milliseconds.</param>
    /// <param name="function">Function to invoke.</param>
    public static void Delay(int msInterval, Action function)
    {
        TimedMethods.Add(new MethodInfo()
        {
            interval = msInterval,
            Amethod = function,
            lastInvoke = Clock.ElapsedMilliseconds,
            singleton = true
        });
    }
    public static void Update()
    {
        List<MethodInfo> methods = new(TimedMethods);

        foreach (MethodInfo m in methods)
        {
            if (!(Clock.ElapsedMilliseconds > m.lastInvoke + m.interval))
                return;

            m.lastInvoke = Clock.ElapsedMilliseconds;
             
            if (m.singleton)
            {
                m.Amethod();
                TimedMethods.Remove(m);
                continue;
            }

            bool result = m.method();

            if (result)
                TimedMethods.Remove(m);
        }
    }

    public static void Init()
    {
        Clock.Start();
    }
}

internal class MethodInfo
{
    public int interval { get; set; }
    public Func<bool> method { get; set; }
    public Action Amethod { get; set; }
    public long lastInvoke { get; set; }
    public bool singleton { get; set; }
}