using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace BingWallpaper {
    static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main() {
            using (Mutex mutex = new Mutex(false, "Global\\" + "71981632-A427-497F-AB91-241CD227ECFF")) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (!mutex.WaitOne(0, false)) {
                    Process[] oldProcesses = Process.GetProcessesByName("BingWallpaper");
                    if (oldProcesses.Length > 0) {
                        Process oldProcess = oldProcesses[0];
                    }

                    return;
                }

                Application.Run(new BingWallpaper());
            }
        }
    }
}
