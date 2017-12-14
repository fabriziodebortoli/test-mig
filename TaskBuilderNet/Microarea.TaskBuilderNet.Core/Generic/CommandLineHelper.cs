using System;
using System.Collections;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Contiene i parametri letti da linea di comando
	/// </summary>
	public struct CommandLineParam
	{
		public string Name;
		public string Value;

		
		public CommandLineParam(string name, string val)
		{
			this.Name = name;
			this.Value = val;
		}

		/// <summary>
		/// Ritorna il valore di un parametro passato da linea di comando dato il nome
		/// </summary>
		//---------------------------------------------------------------------
		public static string GetCommandLineParameterValue (string lookFor)
		{
			foreach (CommandLineParam param in FromCommandLine())
			{
				if (string.Compare(param.Name, lookFor, StringComparison.InvariantCultureIgnoreCase) == 0)
					return param.Value;
			}

			return string.Empty;
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Legge un array di coppie nome/valore da linea di comando
		/// </summary>
		/// <returns>Lista di strutture dei comandi</returns>
		public static CommandLineParam[] FromCommandLine()
		{
			string cmdName = null, cmdValue = null;
			ArrayList cmds = new ArrayList();
			cmds.AddRange(Environment.GetCommandLineArgs());
			cmds.RemoveAt(0); //first is program name
			if (cmds.Count == 0)
				return new CommandLineParam[0];

			ArrayList commands = new ArrayList();
			foreach (string arg in cmds)
			{
				if (arg.StartsWith("/")) //nome di comando
				{
					cmdName = arg.TrimStart('/');
					continue;
				}
				else //valore di comando
				{
					//priva di avere un command value devo aver avuto un command name...
					if (cmdName == null)
						throw new ApplicationException(string.Format("Invalid command line: '{0}';\r\nvalid sintax is: '/<command name 1> <command value 1> [, /<command name 2> <command value 2> ...]'", Environment.CommandLine));

					cmdValue = arg;

					commands.Add(new CommandLineParam(cmdName, cmdValue));
					//riazzero la coppia nome-comando
					cmdName = cmdValue = null;
				}

			}
			return (CommandLineParam[]) commands.ToArray(typeof(CommandLineParam));
		}
	}
}
