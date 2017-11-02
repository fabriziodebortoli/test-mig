using System;

namespace Microarea.AdminServer.Controllers.Helpers
{
	//================================================================================
	public class APIMessageData
    {
		string destination;
		string subject;
		string body;

		public string Destination { get => destination; set => destination = value; }
		public string Subject { get => subject; set => subject = value; }
		public string Body { get => body; set => body = value; }

		//--------------------------------------------------------------------------------
		public APIMessageData()
		{
			this.destination = this.subject = this.body = String.Empty;
		}

		//--------------------------------------------------------------------------------
		public bool HasData()
		{
			return !String.IsNullOrEmpty(this.destination) &&
				!String.IsNullOrEmpty(this.body) &&
				!String.IsNullOrEmpty(this.subject);
		}
	}
}
