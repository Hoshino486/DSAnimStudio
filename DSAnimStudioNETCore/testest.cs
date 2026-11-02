using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace EventInput
{
    public class CharacterEventArgs : EventArgs
    {
        private readonly char character; 
        private readonly int lParam;

        public CharacterEventArgs(char character, int lParam)
        {
            this.character = character; 
            this.lParam = lParam;
        }

        public char Character => character;

        public int Param => lParam;
        public int RepeatCount => lParam & 0xffff;
        public bool ExtendedKey => (lParam & (1 << 24)) > 0;
        public bool AltPressed => (lParam & (1 << 29)) > 0;
        public bool PreviousState => (lParam & (1 << 30)) > 0;
        public bool TransitionState => (lParam & (1 << 31)) > 0;
    }

    public class KeyEventArgs : EventArgs
    {
        private Keys keyCode;

        public KeyEventArgs(Keys keyCode)
        {
            this.keyCode = keyCode;
        }

        public Keys KeyCode => keyCode;
    }

    public delegate void CharEnteredHandler(object sender, CharacterEventArgs e); 
    public delegate void KeyEventHandler(object sender, KeyEventArgs e);

    public static class EventInput

    {
        /// <summary>
        /// 输入字符时触发的事件。
        /// </summary>
        public static event CharEnteredHandler CharEntered;

        /// <summary>
        /// 按键按下时触发的事件。键盘重复会导致它触发好几次。
        /// </summary>
        public static event KeyEventHandler KeyDown;

        /// <summary>
        /// 按键松开时触发的事件。
        /// </summary>
        public static event KeyEventHandler KeyUp;

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private static bool initialized; 
        private static IntPtr prevWndProc; 
        private static WndProc hookProcDelegate; 
        private static IntPtr hIMC;		
        
        //我们需要的各种 Win32 常量
        private const int GWL_WNDPROC = -4; 
        private const int WM_KEYDOWN = 0x100; 
        private const int WM_KEYUP = 0x101; 
        private const int WM_CHAR = 0x102; 
        private const int WM_IME_SETCONTEXT = 0x0281; 
        private const int WM_INPUTLANGCHANGE = 0x51; 
        private const int WM_GETDLGCODE = 0x87; 
        private const int WM_IME_COMPOSITION = 0x10f; 
        private const int DLGC_WANTALLKEYS = 4;

        //我们用到的 Win32 函数
        [DllImport("Imm32.dll")] 
        private static extern IntPtr ImmGetContext(IntPtr hWnd);
        
        [DllImport("Imm32.dll")] 
        private static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);
        
        [DllImport("user32.dll")] 
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")] 
        private static extern int SetWindowLongPtrA(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// 用给定的 GameWindow 初始化 TextInput。
        /// </summary>
        /// <param name="window">The XNA window to which text input should be linked.</param>
        public static void Initialize(GameWindow window) 
        { 
            if (initialized) 
                throw new InvalidOperationException("TextInput.Initialize can only be called once!"); 
            hookProcDelegate = new WndProc(HookProc);
            //var ptr = (int)Marshal.GetFunctionPointerForDelegate(hookProcDelegate);
            prevWndProc = (IntPtr)SetWindowLongPtrA(window.Handle, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(hookProcDelegate)); 
            hIMC = ImmGetContext(window.Handle); 
            initialized = true; 
        }

        private static IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr returnCode = CallWindowProc(prevWndProc, hWnd, msg, wParam, lParam);
            switch (msg)
            {
                case WM_GETDLGCODE: 
                    returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS); 
                    break;
                case WM_KEYDOWN:
                    KeyDown?.Invoke(null, new KeyEventArgs((Keys)wParam)); 
                    break;
                case WM_KEYUP: 
                    KeyUp?.Invoke(null, new KeyEventArgs((Keys)wParam)); 
                    break;
                case WM_CHAR: 
                    CharEntered?.Invoke(null, new CharacterEventArgs((char)wParam, lParam.ToInt32())); 
                    break;
                case WM_IME_SETCONTEXT: 
                    if (wParam.ToInt32() == 1) 
                        ImmAssociateContext(hWnd, hIMC); 
                    break;
                case WM_INPUTLANGCHANGE: 
                    ImmAssociateContext(hWnd, hIMC); 
                    returnCode = (IntPtr)1; 
                    break;
            }
            return returnCode;
        }
    }
}