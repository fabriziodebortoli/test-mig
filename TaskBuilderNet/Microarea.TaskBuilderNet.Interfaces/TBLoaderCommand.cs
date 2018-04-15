using System;

namespace Microarea.TaskBuilderNet.Interfaces
{
	[Serializable]
	public class TBLoaderCommand
	{
		public enum CommandType{Ping, Start, Stop}
		public string Path { get; set; }
		public string Arguments { get; set; }
		public int ProcessId { get; set; }
		public string ClientId { get; set; }
		public CommandType Type { get; set; }
    }

	[Serializable]
	public class TBLoaderResponse
	{
		public bool Result { get; set; }
		public string Message { get; set; }
		public int ProcessId { get; set; }
		public int Port { get; set; }
	}
}
