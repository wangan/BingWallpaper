using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace BingWallpaper {
    public partial class BingWallpaper : Form {

        private static Thread Daemon = null;
        private static string BingSrc = "http://cn.bing.com/HPImageArchive.aspx?idx=0&n={0}";
        private static bool _IsDispose = false;
        private static string ImgDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\VIFUNS.COM\\BingWallpaper\\";
        private static Queue<string> Slides = null;
        private static System.Threading.Timer DownloadTimer;
        private static System.Threading.Timer SlideTimer;
        private static DateTime LastDownload = DateTime.MinValue;
        private static List<KeyValuePair<int, int>> Resolution = null;
        private static int Width = -1;
        private static int Height = -1;

        /*http://cn.bing.com/az/hprichbg/rb/Cyclops_ZH-CN12843334634_1920x1080.jpg*/

        public BingWallpaper(string[] args) {
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            Resolution = new List<KeyValuePair<int, int>>() {
                new KeyValuePair<int, int>(1920, 1080),
                new KeyValuePair<int, int>(1680, 1050),
                new KeyValuePair<int, int>(1600, 900),
                new KeyValuePair<int, int>(1440, 900),
                new KeyValuePair<int, int>(1440, 1050),
                new KeyValuePair<int, int>(1366, 768),
                new KeyValuePair<int, int>(1360, 1050),
                new KeyValuePair<int, int>(1280, 1024),
                new KeyValuePair<int, int>(1280, 960),
                new KeyValuePair<int, int>(1280, 800),
                new KeyValuePair<int, int>(1280, 768),
                new KeyValuePair<int, int>(1280, 720),
                new KeyValuePair<int, int>(1280, 600),
                new KeyValuePair<int, int>(1152, 864),
                new KeyValuePair<int, int>(1024, 768),
                new KeyValuePair<int, int>(800, 600)
            };

            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;

            try {
                Dictionary<string, string> arguments = new Dictionary<string, string>();
                string argStr = "";
                if (null != args && args.Length > 0) {
                    arguments = CMDArgs.Parse(args, out argStr);
                    if (arguments.ContainsKey("?") || arguments.ContainsKey("help")) {
                        HelpMenu();

                        return;
                    }
                    else if (arguments.ContainsKey("a")) {
                        AutoRun(argStr);
                    }
                }

                Run(arguments);

            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                EventLog.WriteEntry("BW", ex.Message, EventLogEntryType.Error);
            }

            EventLog.WriteEntry("BW", "Start", EventLogEntryType.Information);
        }

        static void HelpMenu() {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("--------------------------------");
            sb.AppendLine("-a : AutoStart       \t开机启动");
            sb.AppendLine("-i : Interval        \t壁纸更换间隔");
            sb.AppendLine("-r : Range           \t更换最近N天的壁纸");
            sb.AppendLine("--------------------------------");

            MessageBox.Show(sb.ToString(), "提示");
        }

        private void AutoRun(string argStr) {
            AutoStart.Run("BingWallpaper", "\"" + Application.ExecutablePath + "\" " + argStr);
        }

        private void Run(Dictionary<string, string> arguments) {
            int interval = arguments.ContainsKey("i") ? Int32.Parse(arguments["i"]) * 60 * 1000 : 60 * 60 * 1000;
            int range = arguments.ContainsKey("r") ? Int32.Parse(arguments["r"]) : 5;
            Slides = new Queue<string>(range);

            DownloadWallpaper(range);

            /* 更换壁纸 */
            SlideTimer = new System.Threading.Timer((state) => {
                try {
                    string imgSrc = Next(range);
                    if (!string.IsNullOrEmpty(imgSrc))
                        Win32Api.SystemParametersInfo(20, 0, imgSrc, 0x2);
                }
                catch (Exception ex) {
                    EventLog.WriteEntry("BW", ex.Message, EventLogEntryType.Error);
                }
            }, null, 0, interval);


            /* 第一次运行 OR 换天 */
            /* 每分钟检查一次 */
            DownloadTimer = new System.Threading.Timer((state) => {
                if (DateTime.Now.Date > LastDownload.Date) {
                    try {
                        bool flag = DownloadWallpaper(range);
                        if (flag) {
                            Console.WriteLine("Success download latest wallpaper.");

                            string imgSrc = Next(range);
                            if (!string.IsNullOrEmpty(imgSrc))
                                Win32Api.SystemParametersInfo(20, 0, imgSrc, 0x2);

                            LastDownload = DateTime.Now;
                        }
                    }
                    catch (Exception ex) {
                        EventLog.WriteEntry("BW", ex.Message, EventLogEntryType.Error);
                    }
                }

            }, null, 10 * 60 * 1000, 60 * 1000);
        }

        private string Next(int range) {
            if (null != Slides && Slides.Count > 0) {
                string imgStr = Slides.Dequeue();
                Slides.Enqueue(imgStr);
                return imgStr;
            }
            else {
                if (!Directory.Exists(ImgDir)) {
                    Directory.CreateDirectory(ImgDir);

                    return "";
                }
                else {
                    var images = Directory.GetFiles(ImgDir, "*.jpg");
                    if (null != images && images.Length > 0) {
                        var unsortImages = images.ToList();
                        var selImages = unsortImages.OrderByDescending(I => I).Take(range).ToList();

                        selImages.ForEach(I => Slides.Enqueue(I));

                        return selImages.FirstOrDefault();
                    }
                }
            }

            return "";
        }

        private bool DownloadWallpaper(int range) {
            try {
                Slides.Clear();

                var content = Get(string.Format(BingSrc, range));
                string pattern = @"<fullstartdate>(\d{8})\d*</fullstartdate>.*?<url>(.*?(\d{0,5})x(\d{0,5}).*?)</url>";
                RegexOptions regexOptions = RegexOptions.None;
                Regex regex = new Regex(pattern, regexOptions);
                var allMatches = regex.Matches(content);
                foreach (Match matches in allMatches) {
                    if (matches.Success && matches.Groups.Count >= 4) {
                        var url = string.Format("http://cn.bing.com/{0}", matches.Groups[2].Value);
                        url = url.Replace(matches.Groups[3].Value, Width.ToString());
                        url = url.Replace(matches.Groups[4].Value, Height.ToString());

                        if (!Directory.Exists(ImgDir))
                            Directory.CreateDirectory(ImgDir);

                        string imgSrc = ImgDir + "\\" + matches.Groups[1].Value + ".jpg";
                        if (!File.Exists(imgSrc)) {
                            try {
                                using (WebClient client = new WebClient()) {
                                    client.DownloadFile(url, imgSrc);
                                }
                            }
                            catch (WebException ex) {
                                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound) {
                                    Width = Resolution.FirstOrDefault().Key;
                                    Height = Resolution.FirstOrDefault().Value;
                                }
                            }
                        }
                        Slides.Enqueue(imgSrc);
                    }
                }
            }
            catch (Exception ex) {
                EventLog.WriteEntry("BW", ex.Message, EventLogEntryType.Error);
                return false;
            }

            return true;
        }

        private string Get(string url) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";
            var response = request.GetResponse();
            using (var stream = response.GetResponseStream()) {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
