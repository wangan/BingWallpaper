using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace BingWallpaper {
    static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main() {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BingWallpaper());

            AutoStart.Run("BingWallpaper", AppDomain.CurrentDomain.BaseDirectory + "//BingWallpaper.exe");
        }
    }
}
