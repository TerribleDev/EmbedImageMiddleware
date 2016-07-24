using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmbedImagesMiddlewear
{
    public static class BclExtensions
    {
        private static Regex regexImgSrc = new Regex(@"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static IEnumerable<string> ToImageLinks(this string htmlSource)
        {
            var matchesImgSrc = regexImgSrc.Matches(htmlSource);
            foreach (Match m in matchesImgSrc)
            {
                yield return m.Groups[1].Value;
            }
        }
    }
}