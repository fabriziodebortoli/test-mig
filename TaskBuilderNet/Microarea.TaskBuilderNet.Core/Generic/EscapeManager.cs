using System;
using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Escapa le stringhe sostituendo i caratteri da escapare con la loro
	/// rappresentazione ASCII tra <code>&</code> e <code>;</code>.
	/// </summary>
	/// <example>
	/// il carattere <code>{</code> è restituito come <code>&123;</code>.
	/// </example>
	//=========================================================================
	public abstract class EscapeManager
	{
		private IDictionary<string, string> escapeTable;
		private IDictionary<string, string> unescapeTable;

		protected abstract IDictionary<string, string> InitUnscapeTable();
		protected abstract IDictionary<string, string> InitEscapeTable();

		//---------------------------------------------------------------------
		protected virtual void Init()
		{
			escapeTable = InitEscapeTable();
			unescapeTable = InitUnscapeTable();
		}

		/// <summary>Data una stringa ritorna la stessa escapata.</summary>
		//---------------------------------------------------------------------
		public string Escape(string toBeEscaped)
		{
			if (escapeTable == null)
				throw new InvalidOperationException("Cannot escape because 'escapeTable' is not initialized");

			// TODO: da ottimizzare agendo char per char.
			string temp = toBeEscaped;
			foreach (string str in escapeTable.Keys)
				temp = temp.Replace(str, escapeTable[str]);

			return temp;
		}

		/// <summary>Data una stringa escapata ritorna la stessa escapata.</summary>
		//---------------------------------------------------------------------
		public string Unescape(string toBeUnescaped)
		{
			if (unescapeTable == null)
				throw new InvalidOperationException("Cannot unescape because 'unescapeTable' is not initialized");

			// TODO: da ottimizzare agendo char per char.
			string temp = toBeUnescaped;
			foreach (string str in unescapeTable.Keys)
				temp = temp.Replace(str, unescapeTable[str]);

			return temp;
		}
	}
}
