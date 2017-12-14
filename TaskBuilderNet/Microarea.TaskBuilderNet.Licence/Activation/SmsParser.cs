using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	//=========================================================================
	public class SmsParser : ISmsParser
	{
		public const string ActivationIdTag = "ActivationId";
		public const string CodeTag = "Code";
		public const string RegisteredCelNumberTag = "RegisteredCelNumber";
		public const string VatNumberTag = "VatNumber";
		public const int VatNumberLength = 11;
		public const string ReleaseDateTag = "ReleaseDate";

		//Il gruppo 0 rappresenta l'activation id: stringa lunga uno o più cifre
		//Il gruppo 1 rappresenta il codice su cui generare il controcodice per il ping: stringa di 5 cifre
		//Il gruppo 3 rappresenta la data della release: stringa di tre caratteri di cui il carattere centrale è sicuramente una lettera mentre il primo e l'ultimo carattere possono essere lettere o numeri.
		//Il gruppo 2 rappresenta il numero di cellulare registrato o la partita iva. È un gruppo opzionale ed è composto di soli numeri.
		private readonly string smsPattern = String.Format("(?<{0}>[0-9]+) *(?<{1}>[0-9]{{5}}) *((?<{3}>([0-9a-zA-Z]{{1}}[a-zA-Z]{{1}}[0-9a-zA-Z]{{1}})))? *((?<{2}>[0-9]*))?", ActivationIdTag, CodeTag, RegisteredCelNumberTag, ReleaseDateTag);
		private Regex smsTextRegEx;

		private readonly string smsCheckPattern = "[^A-Za-z0-9 ]";
		private Regex smsCheckRegEx;

		private IDictionary<string, string> results = new Dictionary<string, string>();

		//---------------------------------------------------------------------
		public SmsParser()
		{
			smsTextRegEx = new Regex(smsPattern);
			smsCheckRegEx = new Regex(smsCheckPattern);
		}

		//---------------------------------------------------------------------
		public virtual string this[string tag]
		{
			get
			{
				if (tag == null || tag.Trim().Length == 0)
					return null;

				string res = null;
				if (!results.TryGetValue(tag, out res))
					res = String.Empty;
				return res;
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Interpreta il testo di un sms in ingresso per estrarne l'id
		/// dell'attivazione del cliente e il codice per il ping.
		/// </summary>
		/// <param name="smsText"></param>
		/// <returns></returns>
		public virtual void ParseSms(string smsText)
		{
			if (smsText == null)
				throw new ArgumentNullException("smsText");

			smsText = smsText.Trim().Replace("\n", "");
			smsText = smsText.Trim().Replace("\r", "");
			smsText = smsText.Trim().Replace("\t", "");
			if (smsText.Length < 6)//un sms almeno può contenere un activation id e un codice da 5 cifre senza spazi, quindi almeno 6
				throw new ArgumentException("smsText");

			string workingSmsText = smsText.Trim().Replace("+", "");

			Match checkMatch = smsCheckRegEx.Match(workingSmsText);
			if (checkMatch.Success)
				throw new NotAllowedCharactersException();

			Match aMatch = smsTextRegEx.Match(workingSmsText);
			if (!aMatch.Success)
				throw new ArgumentException("Invalid tokens number");

			results.Clear();

			results.Add(ActivationIdTag, aMatch.Groups[ActivationIdTag].Value);
			results.Add(CodeTag, aMatch.Groups[CodeTag].Value);

			results.Add(ReleaseDateTag, aMatch.Groups[ReleaseDateTag].Value);

			Group registeredCellNumberGroup = aMatch.Groups[RegisteredCelNumberTag];

			if (registeredCellNumberGroup != null && registeredCellNumberGroup.Success)
			{
				if (registeredCellNumberGroup.Value.Trim().Length == VatNumberLength)//num di cifre di una partita iva: http://it.wikipedia.org/wiki/Partita_IVA
					results.Add(VatNumberTag, registeredCellNumberGroup.Value);
				else
					results.Add(RegisteredCelNumberTag, registeredCellNumberGroup.Value);
			}
		}
	}
}
