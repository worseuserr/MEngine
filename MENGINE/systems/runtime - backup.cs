using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

namespace MEngine;

public static class Runtime 
{
    private static bool IsRunning = true;
	public static List<Entity> Entities = new();
	public static double currentFPS = 0;
	public static double averageFPS = 0;
	public static double dTime;
	public static Stopwatch innerTimer = new();
        

    public static void Main()
    {
        Initialize();
    }
	public static void Initialize()
	{
		innerTimer.Start();
        Time.Init();

		Thread.Sleep(100);

		_debug.RuntimeInit();

		AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
		ConsoleUtil.QuickEditMode(false);

        var Windows = Assembly
            .GetAssembly(typeof(Window))
            .GetTypes()
            .Where(Class => Class.IsSubclassOf(typeof(Window)));
		// Get all window instances
            
        //if (Windows == null) { return; }
        foreach (Type window in Windows)
		{
            var SizeField = window.GetField("Size", BindingFlags.Public | BindingFlags.Static);
            if (SizeField == null)
                SizeField = typeof(Window).GetField("Size", BindingFlags.Public | BindingFlags.Static);
            Vector2 vec = (Vector2)SizeField.GetValue(null);
            var windowSize = new Vector2(vec.X * 2, vec.Y);

            ConsoleUtil.Update();

            var center = window.GetField("Center", BindingFlags.Public | BindingFlags.Static);

            if (center == null)
                ConsoleUtil.Center = new Vector2(0, 0);
            else
                ConsoleUtil.Center = (Vector2)center.GetValue(null);
		} 
		// Wait for Init() on windows with WaitForInit
				
		foreach (Type window in Windows)
		{
            Thread TickThread = new(() => { HandleWindowTick(window); });
			TickThread.Start();
		}
		// Call tick method on all windows 

		ConsoleUtil.SetActive();
    }



	private static void HandleWindowTick(Type window)
	{
		window.GetMethod("Init", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);

        var CenterField = window.GetField("Center", BindingFlags.Public | BindingFlags.Static);
        if (CenterField == null)
            CenterField = typeof(Window).GetField("Center", BindingFlags.Public | BindingFlags.Static);
        Vector2 Center = (Vector2)CenterField.GetValue(null);

        //ConsoleUtil.Update();
        var FramerateField = window.GetField("Framerate", BindingFlags.Public | BindingFlags.Static);
        if (FramerateField == null)
            FramerateField = typeof(Window).GetField("Framerate", BindingFlags.Public | BindingFlags.Static);

        var bShowDebugConsoleField = window.GetField("bShowDebugConsole", BindingFlags.Public | BindingFlags.Static);
        if (bShowDebugConsoleField == null)
            bShowDebugConsoleField = typeof(Window).GetField("bShowDebugConsole", BindingFlags.Public | BindingFlags.Static);

        var DebugField = window.GetField("Debug", BindingFlags.Public | BindingFlags.Static);
        if (DebugField == null)
            DebugField = typeof(Window).GetField("Debug", BindingFlags.Public | BindingFlags.Static);

        var renderer = (RenderHandler)window.GetField("Renderer", BindingFlags.Public | BindingFlags.Static).GetValue(null);

        renderer.Tick(ConsoleUtil.X, ConsoleUtil.Y);
        ConsoleUtil.SetConsolePosition(Center, bCenter: true);

        while (IsRunning)
		{
            ConsoleUtil.Update();
            _debug.RuntimeTick(true);

            var Framerate = (float)FramerateField.GetValue(null);
            var bShowDebugConsole = (bool)bShowDebugConsoleField.GetValue(null);
            var Debug = (ConsoleWindow)DebugField.GetValue(null);

            ParticleSystem.Tick();
            Physics.Tick();
							
			if (Framerate <= 0.1 || Framerate > 1000)
			{	
			    if (bShowDebugConsole)
					Debug.Warn($"Framerate is invalid or unset. Use Window.Framerate = value.\nMin: 0.1, Max: 1000, Current: {Framerate}\nFramerate automatically set to default. ({Window.DefaultFramerate})");
						
				Framerate = Window.DefaultFramerate;
			}
            // Handle invalid framerate

            window.GetMethod("Tick", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            Time.Update();
			ConsoleUtil.UpdateBuffer(true);

            renderer.Tick(ConsoleUtil.X, ConsoleUtil.Y);
            Console.Out.Flush();

            _debug.RuntimeTick(false);
            /*Thread.Sleep((int)Math.Round(
			    1f /
				Framerate *
				1000f
			));*/
			
			ConsoleUtil.UpdateBuffer(false);
            var __ = window.GetMethod("PostRenderTick", BindingFlags.Public | BindingFlags.Static);
            if (__ != null) __.Invoke(null, null);

            // Turn fps to milliseconds
            dTime = innerTimer.ElapsedMilliseconds / 1000d;
			currentFPS = 1d / dTime;
            innerTimer.Restart();

			if (!(framerates.Count > 9)) 
			{
				framerates.Add(currentFPS);
				averageFPS = currentFPS;
				continue;
			}

			if (framerateIndex > 9)
				framerateIndex = 0;

			framerates[framerateIndex] = currentFPS;
			framerateIndex++;

			averageFPS =(framerates[0] + framerates[1] + framerates[2] + framerates[3] +framerates[4] +
						 framerates[5] + framerates[6] + framerates[7] + framerates[8] +framerates[9]) / 10;
        }
    }




	private static List<double> framerates = new();
	private static int framerateIndex = 0;
    static void ProcessExit(object sender, EventArgs e)
    {
		ConsoleUtil.QuickEditMode(true);
    }
}