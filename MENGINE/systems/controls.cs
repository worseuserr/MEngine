using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Net.NetworkInformation;

namespace MEngine;

public static class Controls
{         
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int key);

    private const int VK_LBUTTON = 0x01;
    private const int VK_RBUTTON = 0x02;

    public static bool IsKeyDown(string key, bool overrideFocus = false)
    {
        if (!Enum.TryParse(key, true, out ConsoleKey consoleKey))
            throw new ArgumentException($"{key} is not a valid KeyCode.");

        if (!overrideFocus && !(ConsoleUtil.GetConsoleWindow() == GetForegroundWindow()))
            return false;

        int virtualKeyCode = (int)consoleKey;
        short keyState = GetAsyncKeyState(virtualKeyCode);
        return (keyState & 0x8000) != 0;
    }

    public static bool IsLeftMouseButtonDown()
    {
        short result = GetAsyncKeyState(VK_LBUTTON);
        return (result & 0x8000) != 0;
    }

    public static bool IsRightMouseButtonDown()
    {
        short result = GetAsyncKeyState(VK_RBUTTON);
        return (result & 0x8000) != 0;
    }

    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }

    /// <summary>
    /// Retrieves the cursor's position, in screen coordinates.
    /// </summary>
    /// <see>See MSDN documentation for further information.</see>
    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    public static Vector2 GetCursorPosition(bool bCenter = true)
    {
        POINT lpPoint;
        GetCursorPos(out lpPoint);
        // NOTE: If you need error handling
        // bool success = GetCursorPos(out lpPoint);
        // if (!success)

        if (bCenter)
            return new Vector2(lpPoint.X - ConsoleUtil.Center.X - ConsoleUtil.screenRes.X / 2, -(lpPoint.Y + ConsoleUtil.Center.Y - ConsoleUtil.screenRes.Y / 2));
        else
            return new Vector2(lpPoint.X, -(lpPoint.Y));
    }


    // Currently called from physics.cs
    public static void Tick(Entity ent)
    {
        if (ent == null || !ent.ClickListenerEnabled || !IsMouseHoveringOver(ent)) 
            return;

        if (IsLeftMouseButtonDown())
        {
            ent.InvokeClickedEvent(GetCursorPosition(true), bLeftClick: true);
            return;
        }

        if (IsRightMouseButtonDown())
        {
            ent.InvokeClickedEvent(GetCursorPosition(true), bLeftClick: false);
        }
        return;
    }

    public static bool IsMouseHoveringOver(Entity ent)
    {
        if (ent == null) return false;

        Vector2 Offset = new Vector2(
            Math.Round((ConsoleUtil.X - ConsoleUtil.Center.X) / 8d)
            ,
            Math.Round((ConsoleUtil.Y - ConsoleUtil.Center.Y) / 16d)
        );

    Vector2 cursorPos = GetCursorPosition(true).FromPixels();
        foreach (VertexGroup tri in ent.VisibleShape.Triangles)
        {
            tri.GenerateDenominator(ent.Position);
            if (tri.IsPosInside(cursorPos))
                return true;
        }
        return false;
    }
}

