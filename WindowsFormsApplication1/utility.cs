using System;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{
    public class Utility
    {
        public Utility()
        {
        }
        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", SetLastError = true)]
        static public extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static public extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        //Import find window function
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static public extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongA")]
        public static extern long GetWindowLong(IntPtr hwnd, long nIndex);
        //Import window changing function
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, long nIndex, long dwNewLong);
        [DllImport("user32")]
        public static extern long SetWindowPos(IntPtr hwnd, long hWndInsertAfter, long x, long y, long cx, long cy,
                                               long wFlags);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        static public IntPtr GetWindowHWND(string FormTitle)
        {
            return FindWindow("", FormTitle);
        }
        public static void ToggleTitleBar(IntPtr hwnd, bool showTitle)
        {
            long style = GetWindowLong(hwnd, -16L);
            if (showTitle)
                style |= 0xc00000L;
            else
                style &= -12582913L;
            SetWindowLong(hwnd, -16L, style);
            SetWindowPos(hwnd, 0L, 0L, 0L, 0L, 0L, 0x27L);
            SetFocus(hwnd);
        }

    }
}
