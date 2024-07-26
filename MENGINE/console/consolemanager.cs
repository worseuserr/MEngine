using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Numerics;
using System.Drawing;
using Vector2 = MEngine.Vector2;
using static ConsoleUtil;
public static class ConsoleUtil
{
    public static int Width;
    public static int Height;
    public static int X;
    public static int Y;
    public static Vector2 Center = new(0,0);
    private static List<Vector2> MoveBuffer = new();
    public static Vector2 screenRes = new Vector2(1920,1080);

    private static class NativeFunctions
    {
        public enum StdHandle : int
        {
            STD_INPUT_HANDLE = -10,
            STD_OUTPUT_HANDLE = -11,
            STD_ERROR_HANDLE = -12,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle); //returns Handle

        public enum ConsoleMode : uint
        {
            ENABLE_ECHO_INPUT = 0x0004,
            ENABLE_EXTENDED_FLAGS = 0x0080,
            ENABLE_INSERT_MODE = 0x0020,
            ENABLE_LINE_INPUT = 0x0002,
            ENABLE_MOUSE_INPUT = 0x0010,
            ENABLE_PROCESSED_INPUT = 0x0001,
            ENABLE_QUICK_EDIT_MODE = 0x0040,
            ENABLE_WINDOW_INPUT = 0x0008,
            ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200,

            //screen buffer handle
            ENABLE_PROCESSED_OUTPUT = 0x0001,
            ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
            ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
            DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
            ENABLE_LVB_GRID_WORLDWIDE = 0x0010
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    }

    public static void QuickEditMode(bool Enable)
    {
        //QuickEdit lets the user select text in the console window with the mouse, to copy to the windows clipboard.
        //But selecting text stops the console process (e.g. unzipping). This may not be always wanted.
        IntPtr consoleHandle = NativeFunctions.GetStdHandle((int)NativeFunctions.StdHandle.STD_INPUT_HANDLE);
        UInt32 consoleMode;

        NativeFunctions.GetConsoleMode(consoleHandle, out consoleMode);
        if (Enable)
            consoleMode |= ((uint)NativeFunctions.ConsoleMode.ENABLE_QUICK_EDIT_MODE);
        else
            consoleMode &= ~((uint)NativeFunctions.ConsoleMode.ENABLE_QUICK_EDIT_MODE);

        consoleMode |= ((uint)NativeFunctions.ConsoleMode.ENABLE_EXTENDED_FLAGS);

        NativeFunctions.SetConsoleMode(consoleHandle, consoleMode);
    }
    static class Imports
    {
        public static IntPtr HWND_BOTTOM = (IntPtr)1;
        // public static IntPtr HWND_NOTOPMOST = (IntPtr)-2;
        public static IntPtr HWND_TOP = (IntPtr)0;
        // public static IntPtr HWND_TOPMOST = (IntPtr)-1;

        public static uint SWP_NOSIZE = 1;
        public static uint SWP_NOZORDER = 4;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);
    }

    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    public static void SetConsolePosition(Vector2 newPos, bool bCenter = false)
    {
        IntPtr consoleWindowHandle = GetConsoleWindow();
        RECT windowRect;
        GetWindowRect(consoleWindowHandle, out windowRect);

        int consoleWindowWidth = windowRect.Right - windowRect.Left;
        int consoleWindowHeight = windowRect.Bottom - windowRect.Top;

        if (bCenter)
            SetWindowPos(consoleWindowHandle, IntPtr.Zero, 

                (int)(screenRes.X/2 + Center.X + newPos.X - consoleWindowWidth / 2),
                (int)(screenRes.Y/2 - Center.Y - newPos.Y - consoleWindowHeight / 2)

                , consoleWindowWidth, consoleWindowHeight, 0);
        else
            SetWindowPos(consoleWindowHandle, IntPtr.Zero, 

                (int)(newPos.X), 
                (int)(-newPos.Y), 

                consoleWindowWidth, consoleWindowHeight, 0);

        Update();
    }

    public static void SetDryConsolePosition(Vector2 newPos)
    {
        X = (int)-(screenRes.X / 2 - ((int)(screenRes.X / 2 + Center.X + newPos.X - Width / 2)) - Width * 0.5f);
        Y = (int)(screenRes.Y / 2 - ((int)(screenRes.Y / 2 - Center.Y - newPos.Y - Height / 2)) - Height * 0.5f);
    }

    public static Vector2 GetConsolePosition(bool bCenter = false, bool bVectorized = false)
    {
        IntPtr consoleWindowHandle = GetConsoleWindow();
        RECT windowRect;
        GetWindowRect(consoleWindowHandle, out windowRect);

        int consoleWindowLeft = windowRect.Left;
        int consoleWindowTop = windowRect.Top;
        int consoleWindowWidth = windowRect.Right - windowRect.Left;
        int consoleWindowHeight = windowRect.Bottom - windowRect.Top;

        if (bCenter)
        {
            return new Vector2(
                -(screenRes.X / 2 - consoleWindowLeft - consoleWindowWidth * 0.5f),
                (screenRes.Y / 2 - consoleWindowTop - consoleWindowHeight * 0.5f)
                );
        }
        
        return new Vector2(consoleWindowLeft, -consoleWindowTop);
    }

    public static void Update()
    {
        IntPtr consoleWindowHandle = GetConsoleWindow();
        RECT windowRect;
        GetWindowRect(consoleWindowHandle, out windowRect);

        Vector2 posi = GetConsolePosition(bCenter: true);
        int consoleWindowWidth = windowRect.Right - windowRect.Left;
        int consoleWindowHeight = windowRect.Bottom - windowRect.Top;
        Width = consoleWindowWidth;
        Height = consoleWindowHeight;
        X = (int)posi.X; 
        Y = (int)posi.Y;
    }

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void SetActive()
    {
        IntPtr consoleWindowHandle = GetConsoleWindow();
        SetForegroundWindow(consoleWindowHandle);
    }

    public static void UpdateBuffer(bool bFirst) 
    {
        if (MoveBuffer.Count == 0)
            return;

        foreach (Vector2 pos in MoveBuffer)
        {
            if (bFirst)
            {
                SetDryConsolePosition(pos);
                return;
            }
            SetConsolePosition(pos, true);
        }

        if (!bFirst)
            MoveBuffer.Clear();
    }

    public static void MoveTo(Vector2 newPos)
    {
        if (newPos == null)
            throw new ArgumentNullException();

        MoveBuffer.Add(newPos);
    }
}