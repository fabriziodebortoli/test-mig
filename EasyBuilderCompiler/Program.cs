using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microarea.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces;
using System.IO;

namespace EasyBuilderCompiler
{
	class Program
	{
		enum Action { None, Compile, CompileCS, CompileVB, Build }
		static int Main(string[] args)
		{
			if (args.Length == 0)
				return UsageDemoError();
			Action action = ParseAction(args[0]);
			try
			{
				switch (action)
				{
					case Action.None:
						return UsageDemoError();
					case Action.Compile:
					case Action.CompileCS:
					case Action.CompileVB:
						if (args.Length != 2)
							return UsageDemoError();
						return Compile(args[1]);
					case Action.Build:
						if (args.Length != 2)
							return UsageDemoError();
						return Build(args[1]);

				}
			}
			catch (Exception ex)
			{
				Console.Error.Write(ex.ToString());
				return -1;
			}
			return 0;
		}

		//--------------------------------------------------------------------------------
		private static int Build(string path)
		{
			throw new NotImplementedException();
		}

		//--------------------------------------------------------------------------------
		private static int Compile(string path)
		{
			//TODO MATTEO vedere dopo il rafactoring del code dom
			//Sources sources = Sources.CreateSourcesFromEbsFile(path, ApplicationType.Standardization);
			//string outFile = Path.ChangeExtension(path, sources.CustomizationInfos.GetSourceExtension());
			//File.WriteAllText(outFile, sources.GetAllCode(!(sources is ControllerSources)));
			return 0;
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Vissualizza il messaggio di errore con l'esempio di utilizzo
		/// </summary>
		private static int UsageDemoError()
		{
			Console.Out.Write("Usage:\r\n\tEasyBuilderCompiler Compile <ebs file path>");
			Console.Out.Write("\r\n\t\tEasyBuilderCompiler Build <ebs folder path>");
			return -1;
		}
		/// <summary>
		/// Trasforma la stringa in enumerativo
		/// </summary>
		private static Action ParseAction(string arg)
		{
			if (string.Compare(arg, "Compile", true) == 0)
				return Action.Compile;
			if (string.Compare(arg, "CompileCS", true) == 0)
				return Action.CompileCS;
			if (string.Compare(arg, "CompileVB", true) == 0)
				return Action.CompileVB;
			if (string.Compare(arg, "Build", true) == 0)
				return Action.Build;
			return Action.None;
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggancia la console di output del processo corrente a quella del processo indicato
		/// </summary>
		/// <param name="dwProcessId">ID del processo</param>
		[DllImport("kernel32.dll", SetLastError = true)]
		extern static bool AttachConsole(int dwProcessId);
		[DllImport("kernel32.dll", SetLastError = true)]
		extern static IntPtr GetLastError();

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Ritorna il processo chiamante (se esiste)
		/// </summary>
		private static Process GetCallingProcess()
		{
			try
			{
				Process p = Process.GetCurrentProcess();
				int n = 1;
				string name = p.ProcessName;
				while (true)
				{
					PerformanceCounter pc = new PerformanceCounter("Process", "ID Process", name);

					if (pc.RawValue == p.Id)
					{
						PerformanceCounter pc1 = new PerformanceCounter("Process", "Creating Process ID", name);
						return Process.GetProcessById((int)pc1.RawValue);
					}
					name = string.Format("{0}#{1}", p.ProcessName, n++);
				}
			}
			catch
			{
				return null;
			}
		}
	}
}
