using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace BingWallpaper {
    public class AutoStart {

        public static void Run(string appName, string appPath) {
            RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (Check(runKey, appName))
                runKey.DeleteValue(appName);

            runKey.SetValue(appName, appPath);
            runKey.Close();

        }

        public static bool Check(RegistryKey runKey, string appName) {
            try {
                string[] runList = runKey.GetValueNames();
                foreach (string item in runList) {
                    if (item.Equals(appName))
                        return true;
                }
                return false;
            }
            catch (Exception ex) {
                EventLog.WriteEntry("BW", ex.Message, EventLogEntryType.Error);
                return false;
            }
        }
    }
}
