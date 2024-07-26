using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace MEngine;

public class Window 
{         
    public static readonly ConsoleWindow Debug = new();
	public static readonly RenderHandler Renderer;
	public static Vector2 Size = new Vector2(100, 50);
	public static float DefaultFramerate = 1000;
	public static float Framerate = 0;
	public static bool bShowDebugConsole = false;
	public static Vector2 Center = new(0, 0);
	//public static bool IsDisposed = false;
	
	public static void ExitWindow()
	{
		//IsDisposed = true;
		SoundSystem.DisposeAllSounds();
		Environment.Exit(0);
	}

	//public static void ConfirmWindowExit()
	//{
	//	Environment.Exit(0);
	//}
}

public class ConsoleWindow
{
    public void Warn(string Message, bool bStackTrace = false)
	{
	    if (Message == null)
		    return;
			
		Console.WriteLine(
		    (bStackTrace) ? new StackTrace(true).ToString() : "" +
		    "\n! " + 
		    Message
		);
	}
	
}