using System.Collections.Generic;

namespace Microarea.AdminServer.Controllers.Helpers
{
	/// <summary>
	/// This class represents a set of parameters used by the client 
	/// to query data on the server
	/// </summary>
	//================================================================================
	public class APIQueryData
    {
		Dictionary<string, string> matchingFields;
		Dictionary<string, string> likeFields;

		public Dictionary<string, string> MatchingFields { get => matchingFields; set => matchingFields = value; }
		public Dictionary<string, string> LikeFields { get => likeFields; set => likeFields = value; }

		//--------------------------------------------------------------------------------
		public APIQueryData()
		{
			this.matchingFields = new Dictionary<string, string>();
			this.likeFields = new Dictionary<string, string>();
		}
	}
}
