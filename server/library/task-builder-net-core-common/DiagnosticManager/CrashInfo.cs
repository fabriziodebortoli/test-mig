using System.IO;

namespace Microarea.RSWeb.DiagnosticManager
{
    public struct CrashInfo
	{
		const int StructVersion = 1;
		public string UserCode;
		public string InstallationName;
		public string Version;
		public string LogFile;
		public byte[] LogFileContent;

		public static CrashInfo Parse(Stream sourceStream)
		{
			CrashInfo info = new CrashInfo();
			//BinaryFormatter fmt = new BinaryFormatter();                 TODO rsweb
			//int ver = (int)fmt.Deserialize(sourceStream);

			//info.UserCode = (string)fmt.Deserialize(sourceStream);
			//info.InstallationName = (string)fmt.Deserialize(sourceStream);
			//info.Version = (string)fmt.Deserialize(sourceStream);
			//info.LogFile = (string)fmt.Deserialize(sourceStream);
			//info.LogFileContent = (byte[])fmt.Deserialize(sourceStream);

			return info;


		}
		public void Unparse(Stream sourceStream)
		{
			//BinaryFormatter fmt = new BinaryFormatter();           todo rsweb
			//fmt.Serialize(sourceStream, StructVersion);

			//fmt.Serialize(sourceStream, UserCode);
			//fmt.Serialize(sourceStream, InstallationName);
			//fmt.Serialize(sourceStream, Version);
			//fmt.Serialize(sourceStream, LogFile);
			//fmt.Serialize(sourceStream, LogFileContent);
		}
	}
}
