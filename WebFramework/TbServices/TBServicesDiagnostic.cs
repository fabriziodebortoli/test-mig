using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.WebServices.TbServices
{
	/// <summary>
	/// Gestione della diagnostica
	/// </summary>
	/// <remarks>
	/// Classe che gestisce la diagnostica dei TBServices; non usiamo un Diagnostic perche' occorre distinguere le diagnostiche dei vari client, non si puo' mischiare tutto.
	/// Inoltre occorre tenere conto di problematiche di multithreading inserendo primitive di semaforizzazione.
	/// Ogni richiesta al web service accede alla propria diagnostica usando come chiave l'autentication token che individua il client.
	/// </remarks>
	//================================================================================
	public class TBServicesDiagnostic
	{
		/// <summary>
		/// Tabella che associa ad ogni authentication token la sua diagnostica
		/// </summary>
		private Dictionary<string, Diagnostic> diagnosticTable = new Dictionary<string, Diagnostic>();

		//--------------------------------------------------------------------------------
		private Diagnostic GetDiagnostic (string authenticationToken)
		{
			Diagnostic d = null;
			if (diagnosticTable.TryGetValue(authenticationToken, out d) && d != null)
				return d;
			d = new Diagnostic("TbServices");
			diagnosticTable[authenticationToken] = d;
			return d;
		}

		//--------------------------------------------------------------------------------
		internal void SetError (string authenticationToken, Exception ex)
		{
			lock (this)
			{
				SetError(authenticationToken, ex.Message);
				if (ex.InnerException != null)
					SetError(authenticationToken, ex.InnerException);
			}
		}

		//--------------------------------------------------------------------------------
		internal void SetError (string authenticationToken, string message)
		{
			lock (this)
			{
				GetDiagnostic(authenticationToken).Set(DiagnosticType.LogInfo | DiagnosticType.Error, message);
			}
		}
		//--------------------------------------------------------------------------------
		internal void SetError(string authenticationToken, string message, params object[] args)
		{
			lock (this)
			{
				GetDiagnostic(authenticationToken).Set(DiagnosticType.LogInfo | DiagnosticType.Error, string.Format(message, args));
			}
		}
		//--------------------------------------------------------------------------------
		internal void SetInfo(string authenticationToken, string message)
		{
			lock (this)
			{
				GetDiagnostic(authenticationToken).Set(DiagnosticType.LogInfo | DiagnosticType.Information, message);
			}
		}
		//--------------------------------------------------------------------------------
		internal void SetInfo(string authenticationToken, string message, params object[] args)
		{
			lock (this)
			{
				GetDiagnostic(authenticationToken).Set(DiagnosticType.LogInfo | DiagnosticType.Information, string.Format(message, args));
			}
		}
		//--------------------------------------------------------------------------------
		internal void Set (string authenticationToken, DiagnosticType diagnosticType, DateTime dateTime, StringCollection stringCollection, IExtendedInfo extendedInfo)
		{
			lock (this)
			{
				GetDiagnostic(authenticationToken).Set(diagnosticType | DiagnosticType.LogInfo, dateTime, stringCollection, extendedInfo, 0, 0);
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Recupera la diagnostica associata ad un particolare authentication token
		/// </summary>
		internal DiagnosticSimpleItem[] GetDiagnosticItems(string authenticationToken, bool clear)
		{
			lock (this)
			{
				Diagnostic d = GetDiagnostic(authenticationToken);
				if (clear)
					diagnosticTable.Remove(authenticationToken);

				//TODO Refactoring Matteo/Luca: ce ne vergognamo profondamente.
				//Morirà con l'accorpamento.
				IDiagnosticSimpleItem[] current = d.AllSimpleItems;
				DiagnosticSimpleItem[] temp = new DiagnosticSimpleItem[current.Length];
				for (int i = 0; i < current.Length; i++)
					temp[i] = (DiagnosticSimpleItem)current[i];

				return temp;
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Rimuove dalla tabella di diagnostica i messaggi associati ad un authentication token
		/// </summary>
		internal void Clear (string authenticationToken)
		{
			lock (this)
			{
				diagnosticTable.Remove(authenticationToken);
			}
		}

		//-----------------------------------------------------------------------
		internal static void CopyDiagnostic (string authenticationToken, Diagnostic source, TBServicesDiagnostic target)
		{
			foreach (DiagnosticItem item in source.AllItems)
				target.Set(authenticationToken, item.Type, item.Occurred, item.Explain, item.ExtendedInfo);

		}

	}
}
