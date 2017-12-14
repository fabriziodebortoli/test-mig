using System.Reflection;

using Microarea.Console.Core.PlugIns;

[assembly: AssemblyConfiguration("")]
//
[assembly: IsPlugIn(true)]
[assembly: DependencyFromPlugIn("Microarea.Console.Plugin.SysAdmin.SysAdmin")]
[assembly: Activated(true)]
