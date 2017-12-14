using System;
using System.IO;
using System.Text;

namespace Microarea.TaskBuilderNet.Licence.Activation.Components
{
	/// <summary>
	/// Fornisce servizi per formattare in modo adeguato la stringa contenente il serial number.
	/// </summary>
	public class SNFormatter
	{
		/// <summary>
		/// Elimina da "toBeCleaned" tutte le occorrenze di ogni stringa dell'array "toBeRemoved".
		/// </summary>
		//---------------------------------------------------------------------------------------------------
		public static string CleanSN(string toBeCleaned, string[] toBeRemoved)
		{
			string	cleanedString	=	toBeCleaned;
			int		position		=	0;
			for (int i = 0; i < toBeRemoved.Length; i++)
			{
				position = cleanedString.LastIndexOf(toBeRemoved[i]);
				while(position != -1)
				{
					cleanedString	= cleanedString.Remove(position, toBeRemoved[i].Length);
					position		= cleanedString.LastIndexOf(toBeRemoved[i]);
				}
			}

			return cleanedString;
		}

		/// <summary>
		/// Elimina da "toBeCleaned" ogni carattere che non sia alfanumerico e maiuscolo se rientrante tra
		/// quelli contemplati da codice ASCII (i quali combaciano con i primi 255 del codice Unicode).
		/// Se non compaiono tra quelli del codice ASCII li lascia inalterati. 
		/// </summary>
		//---------------------------------------------------------------------------------------------------
		public static string CleanSN(string toBeCleaned)
		{
			StringBuilder	strBuilder			=	new StringBuilder();
			StringReader	strReader			=	new StringReader(toBeCleaned);
			int				unicodeValue		=	-1;

			while((unicodeValue = strReader.Read()) != -1)
			{
				if(unicodeValue > 255)
					strBuilder.Append(Convert.ToChar(unicodeValue));
				else
				{
					if	( 
							((unicodeValue > 47) && (unicodeValue < 58)) ||
							((unicodeValue > 64) && (unicodeValue < 91)) 
						)
						strBuilder.Append(Convert.ToChar(unicodeValue));
				}
			}

			strReader.Close();

			return strBuilder.ToString();
		}
	}
}
