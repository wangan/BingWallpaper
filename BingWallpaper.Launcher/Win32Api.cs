using System.Runtime.InteropServices;

namespace BingWallpaper.Launcher {
    public class Win32Api {

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(
            int uAction,
            int uParam,
            string lpvParam,
            int fuWinIni
        );

    }
}
