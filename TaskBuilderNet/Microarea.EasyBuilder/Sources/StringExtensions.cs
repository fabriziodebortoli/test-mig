using System;
using System.IO;
using System.Text;

using Microarea.TaskBuilderNet.Core.Generic;
namespace Microarea.EasyBuilder
{
    //=========================================================================
    internal static class StringExtensions
    {
        //---------------------------------------------------------------------
        public static string RelativeOn(this string @this, string path)
        {
            string res = null;
            if (@this.IndexOf(path) != -1)
            {
                res = @this.Replace(path, string.Empty);
                if (res.StartsWith("\\"))
                {
                    res = res.Substring(1);
                }

                return res;
            }

            string commonPath = FindCommonPath(@this, path);
			if (commonPath.IsNullOrEmpty())
				return res;
            res = @this.Replace(commonPath, string.Empty);
            path = path.Replace(commonPath, string.Empty);
            int tokens = path.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Length;

            var pathBld = new StringBuilder();
            for (int i = 0; i < tokens; i++)
            {
                pathBld.Append("\\..");
            }
            pathBld.Append(res);

            return pathBld.ToString().Trim(Path.DirectorySeparatorChar);
        }

        private static string FindCommonPath(string path1, string path2)
        {
            var tokens1 = path1.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var tokens2 = path2.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var driver = tokens1.Length > tokens2.Length ? tokens2 : tokens1;
            var passenger = tokens1.Length <= tokens2.Length ? tokens2 : tokens1;

            var resBld = new StringBuilder();
            for (int i = 0; i < driver.Length; i++)
            {
                if (driver[i] != passenger[i])
                {
                    break;
                }
                resBld.Append(driver[i]).Append(Path.DirectorySeparatorChar);
            }

            return resBld.ToString().TrimEnd(Path.DirectorySeparatorChar);
        }
    }    
}
