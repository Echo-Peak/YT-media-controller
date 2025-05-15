using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace YTMediaControllerSrv.Controller
{
    internal class SystemController
    {
        static IKeyboardSimulator keyboardEmulator = new InputSimulator().Keyboard;
        public static void EnterFullScreen()
        {
            keyboardEmulator.KeyPress(VirtualKeyCode.F11);
        }
    }
}
