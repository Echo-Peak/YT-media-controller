using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace YTMediaControllerSrv.Controller
{
    internal class SystemController
    {
        static IKeyboardSimulator keyboardEmulator = new InputSimulator().Keyboard;
        public static void EnterFullScreen()
        {
            keyboardEmulator.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.LWIN, WindowsInput.Native.VirtualKeyCode.VK_F);
        }
    }
}
