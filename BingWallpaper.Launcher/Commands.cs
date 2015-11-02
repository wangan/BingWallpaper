using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BingWallpaper.Launcher {
    public class Commands {
        public static Dictionary<string, string> Parse(string[] args) {
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (null != args && args.Length > 0) {
                string inputData = args.Aggregate((i, j) => i + " " + j);
                string pattern = @"-((\S*)(.*?))";
                RegexOptions regexOptions = RegexOptions.RightToLeft;
                Regex regex = new Regex(pattern, regexOptions);
                var matches = regex.Matches(inputData);
                foreach (Match match in matches) {
                    if (match.Success && match.Groups.Count == 4) {
                        param[match.Groups[2].Value] = match.Groups[3].Value;
                    }
                }
            }

            return param;
        }
    }
}
