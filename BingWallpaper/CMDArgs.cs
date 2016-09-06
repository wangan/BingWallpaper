using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BingWallpaper {
    public class CMDArgs {
        private static string CMDRegStr = @"[-|/]+(\S*?)\s*?(\S*?)";
        private static RegexOptions regexOptions = RegexOptions.RightToLeft;
        private static Regex regex = new Regex(CMDRegStr, regexOptions);

        /// <summary>
        /// "-m abc -b 123 -c ---d  --y --f 99 /q  77";
        /// </summary>
        /// <param name="oArgs"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Parse(string[] oArgs, out string oArgStr) {
            Dictionary<string, string> Args = new Dictionary<string, string>();
            oArgStr = oArgs.Aggregate((i, j) => i + " " + j);
            var mRes = regex.Matches(oArgStr);
            foreach (Match cur in mRes) {
                if (cur.Groups.Count == 3) {
                    Args[cur.Groups[1].Value] = cur.Groups[2].Value;
                }
            }

            return Args;
        }
    }
}
