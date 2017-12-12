using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Win32;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using Microarea.EasyBuilder.Localization;
using ICSharpCode.NRefactory.CSharp;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyBuilder
{
    //=========================================================================
    internal class MsBuild_BuildStrategy : BuildStrategy
    {
        static readonly string msBuildFullPath = InitMsBuildFullPath();

        string outputDllFilePath;
        bool debug;

        private static string InitMsBuildFullPath()
        {
            try
            {
                /*
                http://stackoverflow.com/questions/328017/path-to-msbuild
                http://www.csharp411.com/where-to-find-msbuild-exe/
                http://timrayburn.net/blog/visual-studio-2013-and-msbuild/
                http://blogs.msdn.com/b/visualstudio/archive/2013/07/24/msbuild-is-now-part-of-visual-studio.aspx
                */
                string msbuildFullPath = null;
                string keyName = @"SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0\";//VS 2015
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
                {
                    msbuildFullPath = key.GetValue("MSBuildToolsPath").ToString();
                }
                return Path.Combine(msbuildFullPath, "MSBuild.exe");
            }
            catch
            {
                return String.Empty;
            }
        }

        public MsBuild_BuildStrategy(string outputFolder, bool debug)
        {
            this.outputDllFilePath = outputFolder;
            this.debug = debug;
        }

        public override EBCompilerResults Build(Sources sources)
        {
            var generatedSources = sources.GenerateSources(this.outputDllFilePath);

            string args = String.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "/t:Rebuild /p:Configuration={0} /p:Platform=x86",
                (debug ? "Debug" : "Release")
                );

            var lpi = new LaunchProcessInfo
            {
                ProcessFilePath = msBuildFullPath,
                WorkingDirectory = BasePathFinder.GetSourcesFolderPathFromDll(this.outputDllFilePath),
                Args = args,
                TimeoutMillSecs = 360000//1 ora
            };

            LaunchProcess(lpi);

            return EBCompilerResultsFactory.FromMsBuildOutput(lpi.StandardOutput, this.outputDllFilePath);
        }

        private static void LaunchProcess(LaunchProcessInfo launchProcessInfo)
        {
            ProcessStartInfo psi = new ProcessStartInfo(launchProcessInfo.ProcessFilePath, launchProcessInfo.Args);
            psi.WorkingDirectory = launchProcessInfo.WorkingDirectory;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process p = Process.Start(psi);
            launchProcessInfo.StandardOutput = p.StandardOutput.ReadToEnd();
            launchProcessInfo.StandardError = p.StandardError.ReadToEnd();

            p.WaitForExit(launchProcessInfo.TimeoutMillSecs);

            launchProcessInfo.ExitCode = p.ExitCode;
        }
        class LaunchProcessInfo
        {
            public int ExitCode { get; set; }
            public string StandardError { get; set; }
            public string StandardOutput { get; set; }
            public string ProcessFilePath { get; set; }
            public string Args { get; set; }
            public int TimeoutMillSecs { get; set; }
            public string WorkingDirectory { get; set; }
        }
    }
}