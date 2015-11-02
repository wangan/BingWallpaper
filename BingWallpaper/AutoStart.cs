using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace BingWallpaper {
    public class AutoStart {

        public static void Run(string appName, string appPath) {
            if (!Check(appName)) {
                RegistryKey runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                runKey.SetValue(appName, appPath);
                runKey.Close();
            }
        }

        public static bool Check(string appName) {
            try {
                RegistryKey runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                string[] runList = runKey.GetValueNames();
                runKey.Close();
                foreach (string item in runList) {
                    if (item.Equals(appName))
                        return true;
                }
                return false;
            } catch (Exception ex) {
                EventLog.WriteEntry("BW", ex.Message, EventLogEntryType.Error);
                return false;
            }
        }
    }
}
