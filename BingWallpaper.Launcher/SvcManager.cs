using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

namespace BingWallpaper.Launcher {
    class SvcManager : ServiceBase {
        private const string InstallCommand = "i";
        private const string UninstallCommand = "u";
        private const string Name = "BingWallpaper";
        private const string DisplayName = "Bing 每日壁纸";
        private static readonly string _ServicePath = Assembly.GetExecutingAssembly().Location;
        private static Process _BingWallpaper = null;

        /// <summary>
        /// 指定服务启动时采取的操作
        /// </summary>
        /// <param name="args">启动命令传递的数据</param>
        protected override void OnStart(string[] args) {
            try {
                _BingWallpaper = new Process();
                _BingWallpaper.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "/BingWallpaper.exe";
                _BingWallpaper.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _BingWallpaper.StartInfo.UseShellExecute = true;
                _BingWallpaper.StartInfo.CreateNoWindow = true;
                _BingWallpaper.Start();

            } catch (Exception ex) {

            }
        }

        /// <summary>
        /// 指定服务停止时采取的操作
        /// </summary>
        protected override void OnStop() {
            try {
                _BingWallpaper.Kill();

            } catch (Exception ex) {

            }
        }

        static void Main(string[] args) {
            var param = Commands.Parse(args);
            bool result = false;
            if (param.ContainsKey(InstallCommand)) {
                result = SrvInst.InstallService(_ServicePath, Name, DisplayName);
            } else if (param.ContainsKey(UninstallCommand)) {
                result = SrvInst.UnInstallService(Name);
            } else {
                try {
                    ServiceBase.Run(new SvcManager());

                    result = true;
                } catch {
                }
            }
            if (result) {
                Console.WriteLine("Successful");
            } else {
                Console.WriteLine("There was a problem");
            }
        }
    }
}
