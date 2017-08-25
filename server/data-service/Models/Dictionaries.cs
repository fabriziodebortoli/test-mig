

using System.Collections.Generic;
namespace Microarea.DataService.Models
{
	internal class Dictionaries
	{
		public List<Dictionary> dictionaries = new List<Dictionary> ();

	}

	internal class Dictionary
	{
		public string code = "";
		public string description = "";

		public Dictionary(string code, string description)
		{
			this.code = code;
			this.description = description;
		}
	}
}