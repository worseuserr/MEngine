using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Numerics;
using System.Drawing;
using MEngine;

namespace MEngine;

public class MainWindow : Window
{
    public static new float Framerate = 1000;
    public static new bool bShowDebugConsole = true;
    public static new RenderHandler Renderer = new(typeof(MainWindow));
    public static new Vector2 Size = new(80, 50);
    public static new Vector2 Center = new(20, 0);

    public static void Init()
    {
        _debug.Output = "Initialized!";

    }

    public static void Tick()
    {
        //Center = new Vector2(ConsoleUtil.X, ConsoleUtil.Y).FromPixels();  // EXPERIMENTAL: Uncomment to disconnect rendering from window position. Does not work correctly with a multi-monitor setup.

    }

    public static void PostRenderTick()
    {

    }

    public static void Main()
    { Runtime.Initialize(typeof(MainWindow)); }
}