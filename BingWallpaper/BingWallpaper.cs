using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace BingWallpaper {
    public partial class BingWallpaper : Form {

        private static Thread Daemon = null;
        private static string BingSrc = "http://cn.bing.com/HPImageArchive.aspx?idx=0&n=1";
        private static bool _IsDispose = false;
        private static string ImgDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\VIFUNS.COM\\BingWallpaper\\";

        /*http://cn.bing.com/az/hprichbg/rb/Cyclops_ZH-CN12843334634_1920x1080.jpg*/

        public BingWallpaper() {
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            Run();
        }

        private void Run() {
            Daemon = new Thread(new ThreadStart(() => {
                while (!_IsDispose) {
                    string imgSrc = string.Empty;
                    bool flag = DownloadWallpaper(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, out imgSrc);
                    if (flag) {
                        Win32Api.SystemParametersInfo(20, 0, imgSrc, 0x2);
                    }

                    Thread.Sleep(10 * 60 * 1000);
                }
            }));
            Daemon.Start();
        }

        private bool DownloadWallpaper(int width, int height, out string imgSrc) {
            imgSrc = string.Empty;
            try {
                var content = Get(BingSrc);
                string pattern = @"<url>(.*?(\d{0,5})x(\d{0,5}).*)</url>";
                RegexOptions regexOptions = RegexOptions.None;
                Regex regex = new Regex(pattern, regexOptions);
                var matches = regex.Match(content);
                if (matches.Success && matches.Groups.Count >= 4) {
                    var url = string.Format("http://cn.bing.com/{0}", matches.Groups[1].Value);
                    url = url.Replace(matches.Groups[2].Value, width.ToString());
                    url = url.Replace(matches.Groups[3].Value, height.ToString());

                    if (!Directory.Exists(ImgDir))
                        Directory.CreateDirectory(ImgDir);

                    imgSrc = ImgDir + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".jpg";
                    using (WebClient client = new WebClient()) {
                        client.DownloadFile(url, imgSrc);
                    }
                }
            } catch (Exception ex) {
                EventLog.WriteEntry("BW", ex.Message, EventLogEntryType.Error);
                return false;
            }

            return true;
        }

        private string Get(string url) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BingSrc);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (var stream = response.GetResponseStream()) {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
