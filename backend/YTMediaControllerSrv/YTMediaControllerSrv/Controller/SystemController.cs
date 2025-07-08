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
        public static void EnterSystemFullScreen()
        {
            keyboardEmulator.KeyPress(VirtualKeyCode.F11);
        }
        public static void TriggerYoutubeFullsceen()
        {
            keyboardEmulator.KeyPress(VirtualKeyCode.VK_F);
        }
        public static void TriggerYoutubePlay()
        {
            keyboardEmulator.KeyPress(VirtualKeyCode.VK_K);
        }
    }
}
