using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TbLoaderGate.Application
{
	public abstract class Recorded {
		public const string RecordingFolder = "Recordings";
		public void Save(IHostingEnvironment hostingEnvironment, string fileName)
		{
			string file = Path.Combine(GetRecordingFolder(hostingEnvironment), fileName);
			using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
			{
				JsonSerializerSettings settings = new JsonSerializerSettings();
				settings.Formatting = Formatting.Indented;
				JsonSerializer ser = JsonSerializer.Create(settings);
				ser.Serialize(new JsonTextWriter(sw), this);
			}
		}

		public string GetRecordingFolder(IHostingEnvironment hostingEnvironment)
		{
			string folder = Path.Combine(hostingEnvironment.ContentRootPath, RecordedHttpRequest.RecordingFolder);
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			return folder;
		}
	}
    public class RecordedHttpRequest : Recorded
    {
        public string Url { get; set; }
        public string Body { get; set; }
        public string Response { get; set; }
    }

    public class RecordedWSMessage : Recorded
	{
		public List<RecordedWSMessage> ResponseMessages = new List<RecordedWSMessage>();
		internal string FileName = "";
		public string Cmd { get; set; }
        public string Body { get; set; }
    }
}
