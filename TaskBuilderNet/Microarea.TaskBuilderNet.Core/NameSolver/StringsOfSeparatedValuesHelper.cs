using System;
using System.Collections.Generic;
using System.Text;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	//=========================================================================
	public sealed class StringsOfSeparatedValuesHelper
	{
		private readonly static Object staticLockTicket = new Object();

		//---------------------------------------------------------------------
		private StringsOfSeparatedValuesHelper()
		{ }

		//---------------------------------------------------------------------
		/// <param name="valuesLen">if 0, then perfomrs no check on length</param>
		public static IList<string> ParseValues(
			string commaSeparatedValues,
			char separator,
			int valuesLen
			)
		{
			return ParseValues(commaSeparatedValues, separator, valuesLen, false);
		}

		//---------------------------------------------------------------------
		/// <param name="valuesLen">if 0, then performs no check on length</param>
		public static IList<string> ParseValues(
			string commaSeparatedValues,
			char separator,
			int valuesLen,
			bool ignoreBlanksOnLenCheck
			)
		{
			lock (staticLockTicket)
			{
				List<string> values = new List<string>();

				if (commaSeparatedValues == null || commaSeparatedValues.Trim().Length == 0)
					return values;

				string buffer = null;
				if (valuesLen > 0)
				{
					buffer = ignoreBlanksOnLenCheck ? commaSeparatedValues.Trim() : commaSeparatedValues;
					if (buffer.Length < valuesLen)
						throw new ParseException("Invalid value code specified");
				}

				if (commaSeparatedValues.IndexOf(separator) == -1 && valuesLen > 0)
				{
					buffer = ignoreBlanksOnLenCheck ? commaSeparatedValues.Trim() : commaSeparatedValues;
					if (buffer.Length != valuesLen)
						throw new ParseException("Invalid value code specified");
				}

				string[] codes = commaSeparatedValues.Split(separator);

				values.Clear();

				foreach (string code in codes)
				{
					if (valuesLen > 0 && code.Length == 0)
						continue;

					buffer = ignoreBlanksOnLenCheck ? code.Trim() : code;

					if (valuesLen > 0 && buffer.Length != valuesLen)
						throw new ParseException("Invalid value found: " + buffer);

					values.Add(buffer);
				}

				return values;
			}
		}

		//---------------------------------------------------------------------
		public static string SerializeValues(
			IList<string> values,
			char separator
			)
		{
			lock (staticLockTicket)
			{
				if (values == null || values.Count == 0)
					return String.Empty;

				StringBuilder localBuilder = new StringBuilder();

				for (int i = 0; i < values.Count - 1; i++)
					localBuilder.Append(values[i]).Append(separator);

				localBuilder.Append(values[values.Count - 1]);

				return localBuilder.ToString();
			}
		}
	}
}
