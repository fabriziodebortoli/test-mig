using Microarea.Snap.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    internal static class LaunchProcessTrait
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "this")]
        public static void LaunchProcess(this IClickOnceService @this, string processFilePath, string args, int timeoutInMillSecs)
        {
            LaunchProcess(processFilePath, args, timeoutInMillSecs);
        }

        private static void LaunchProcess(string processFilePath, string args, int timeoutInMillSecs)
        {
            ProcessStartInfo psi = new ProcessStartInfo(processFilePath, args);
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process p = Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd()
                .Replace("\0", string.Empty);
            string error = p.StandardError.ReadToEnd()
                .Replace("\0", string.Empty);

            p.WaitForExit(timeoutInMillSecs);
            if (p.ExitCode != 0)
            {
                throw new LaunchProcessException(String.Format(System.Globalization.CultureInfo.InvariantCulture, "Process '{0}' returned following errors: {1}, {2}", processFilePath, output, error));
            }
        }
    }
}
