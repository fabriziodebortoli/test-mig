using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using System.Globalization;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Unparsers
{
	///<summary>
	/// Classe base per gli unparser
	///</summary>
	//================================================================================
	internal class InfinityBaseUnparser
	{
		//--------------------------------------------------------------------------------
        internal ActionToExport GetXml(InfinityUnparserParams paramsHelper)
		{
            ActionToExport xmlText = null;
        
			switch (paramsHelper.SyncActionType)
			{
				case SynchroActionType.Insert:
				case SynchroActionType.Update:
				case SynchroActionType.Massive:
				case SynchroActionType.Exclude:
				case SynchroActionType.NewAttachment:
				case SynchroActionType.NewCollection:
				case SynchroActionType.UpdateCollection:
				case SynchroActionType.UpdateProvider:
					xmlText = Insert(paramsHelper); 
					break;

				case SynchroActionType.Delete:
				case SynchroActionType.DeleteAttachment:
					xmlText = Delete(paramsHelper); 
					break;
			}

			return xmlText;
		}

		///<summary>
		/// Inserimento di un record (se esiste gia' va in update)
		///</summary>
		//--------------------------------------------------------------------------------
        internal ActionToExport Insert(InfinityUnparserParams paramsHelper)
		{
			ActionToExport massivexmlStrings = new ActionToExport(paramsHelper.CRMAction.ActionName);

			XElement rootElem = new XElement(paramsHelper.CRMAction.ActionName);
			rootElem.Add(new XAttribute(CRMInfinityConnectorConsts.ApplicationIdAttr, paramsHelper.RegisteredApp));

			foreach (DTValuesToImport dtToImport in paramsHelper.ValDataTablesList)
			{
				// scrivo i dati del master con i suoi attributi
				foreach (DataRow dr in dtToImport.MasterDt.Rows)
				{
					XElement addElem = new XElement("Add_" + paramsHelper.CRMAction.ActionName);

					string keysForDisabled = string.Empty;
					string tbGuid = string.Empty;
					string image = string.Empty;

					// aggiungo gli attributi dell'entita' padre
					if (!SetAttributesToMasterElement(paramsHelper.CRMAction, addElem, dr, out keysForDisabled, out tbGuid, out image, paramsHelper.SyncActionType))
						continue;

					massivexmlStrings.Keys.Add(keysForDisabled);
					massivexmlStrings.TBGuid.Add(tbGuid);
                    massivexmlStrings.ErrorMessages.Add(string.Empty);
					massivexmlStrings.Image.Add(image);
					massivexmlStrings.IsSucceeded.Add(false);

					// gestione eventuali campi in append sulla testa
					for (int i = 0; i < paramsHelper.CRMAction.AppendActions.Count; i++)
					{
						// in coda aggiungo gli attributi dell'entita' in append
						if (!SetAttributesToMasterElement(paramsHelper.CRMAction.AppendActions[i], addElem, dtToImport.AppendDtList[i].MasterDt.Rows[0], out keysForDisabled, out tbGuid, out image, paramsHelper.SyncActionType))
							continue;
					}

                    string actualfather = "";
                    string actualAction = "";
                    string oldFather = "";
                    XElement[] ArrElem = new XElement[100];
                    int countArr = 0;
      

                    // scorro tutti gli slave con le subaction
                    foreach (DataTable dtSlave in dtToImport.SlavesDtList)
					{

                        int count = 0;

						foreach (DataRow slaveRow in dtSlave.Rows)
						{
							XElement addSlaveElem = new XElement(dtSlave.TableName);
						
							// faccio un loop nelle subaction di Mago
							foreach (CRMAction subAction in paramsHelper.CRMAction.Subactions)
							{
								// devo analizzare solo la Subaction con lo stesso TableName                                     
								if (string.Compare(subAction.ActionName, dtSlave.TableName, StringComparison.InvariantCultureIgnoreCase) != 0)
									continue;

                                actualfather = subAction.FatherActionName;
                                actualAction = subAction.ActionName;

                                if (!SetAttributesToSlaveElement(subAction, addSlaveElem, slaveRow))
                                    continue;
							}

							// gestione campi in append sulle righe
							foreach (DTValuesToImport dtAppendValues in dtToImport.AppendDtList)
							{
								foreach (DataTable dtAppendSlave in dtAppendValues.SlavesDtList)
								{
									// devo analizzare solo la Subaction con lo stesso TableName
									if (string.Compare(dtSlave.TableName, dtAppendSlave.TableName, StringComparison.InvariantCultureIgnoreCase) != 0)
										continue;

									foreach (CRMAction appendAction in paramsHelper.CRMAction.AppendActions)
									{
										foreach (CRMAction appendSubAction in appendAction.Subactions)
										{
											// devo analizzare solo la Subaction con lo stesso TableName
											if (string.Compare(appendSubAction.ActionName, dtAppendSlave.TableName, StringComparison.InvariantCultureIgnoreCase) != 0)
												continue;

											try
											{
												if (!SetAttributesToSlaveElement(appendSubAction, addSlaveElem, dtAppendSlave.Rows[count]))
													continue;
											}
											catch (Exception)
											{
											}
										}
									}
								}
							}

							try
							{
                                if (string.IsNullOrEmpty(actualfather))
                                {
                                    for(int i=0; i <= countArr; i++)
                                        addSlaveElem.Add(ArrElem[i]);

                                    XElement addBOSlaveElem = new XElement(string.Concat(CRMInfinityConnectorConsts.BOPrefixNode, dtSlave.TableName));
                                    addBOSlaveElem.Add(addSlaveElem);

                                    addElem.Add(addBOSlaveElem);

                                    for (int i = 0; i < countArr; i++)
                                       ArrElem[i] = null;

                                    countArr = 0;
                                }
                                else if(actualAction != oldFather && !string.IsNullOrEmpty(actualfather))
                                {
                                    oldFather = actualfather;

                                    XElement addBOSlaveElem = new XElement(string.Concat(CRMInfinityConnectorConsts.BOPrefixNode, dtSlave.TableName));
                                    addBOSlaveElem.Add(addSlaveElem);

                                    ArrElem[countArr] = addBOSlaveElem;
                                    countArr++;
                                }
                                else if (actualAction == oldFather && !string.IsNullOrEmpty(actualfather))
                                {
                                    oldFather = actualfather;

                                    for (int i = 0; i <= countArr; i++)
                                        addSlaveElem.Add(ArrElem[i]);

                                    for (int i = 0; i < countArr; i++)
                                        ArrElem[i] = null;

                                    XElement addBOSlaveElem = new XElement(string.Concat(CRMInfinityConnectorConsts.BOPrefixNode, dtSlave.TableName));
                                    addBOSlaveElem.Add(addSlaveElem);

                                    ArrElem[0] = addBOSlaveElem;

                                    countArr = 1;
                                }


                            }
							catch (System.InvalidOperationException ex)
							{
								System.Diagnostics.Debug.WriteLine(ex.Message);
							}
						}

						count++;
					}
					rootElem.Add(addElem);
				}
			}

			XDocument xDoc = new XDocument(new XDeclaration(CRMInfinityConnectorConsts.XmlVersion, CRMInfinityConnectorConsts.XmlEncoding, null), rootElem);

			MemoryStream memstream = new MemoryStream();
			XmlTextWriter xmlwriter = new XmlTextWriter(memstream, Encoding.GetEncoding(CRMInfinityConnectorConsts.XmlEncoding));
			xDoc.Save(xmlwriter);
			xmlwriter.Flush();
			memstream.Seek(0, SeekOrigin.Begin);
			StreamReader streamreader = new StreamReader(memstream, Encoding.GetEncoding(CRMInfinityConnectorConsts.XmlEncoding));
			string xml = streamreader.ReadToEnd();
			massivexmlStrings.XmlToImport = xml;

			return massivexmlStrings;
		}

		///<summary>
		/// Update di un record (richiama la insert perche' se il record esiste gia' va in automatico in update)
		///</summary>
		//--------------------------------------------------------------------------------
        internal ActionToExport Delete(InfinityUnparserParams paramsHelper)
		{
			XmlDocument xDoc = CreateXmlDocument(paramsHelper.CRMAction.ActionName, paramsHelper.RegisteredApp);

			Dictionary<string, string> keyValuesToDelete = paramsHelper.GetKeyValuesToDelete();

			if (keyValuesToDelete.Count == 0)
				return new ActionToExport();

			XmlElement deleteElem = xDoc.CreateElement("Delete_" + paramsHelper.CRMAction.ActionName);

			// gestione ad-hoc per la cancellazione di un articolo, che in Infinity ha la PK composta da due segmenti:
			// il codice articolo e una chiave calcolata sulla base della nostra colonna IsGood (se true mettiamo 1, se false mettiamo 4)
			if (string.Compare(paramsHelper.CRMAction.ActionName, "MAGO_ITEMS", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				foreach (KeyValuePair<string, string> k in keyValuesToDelete)
				{
					// gestione ad-hoc per la chiave secondaria
					if (string.Compare(k.Key, "ARFLGART_K", StringComparison.InvariantCultureIgnoreCase) == 0)
						deleteElem.SetAttribute(k.Key, (k.Value == bool.TrueString.ToLowerInvariant() ? "1" : "4"));
					else
						deleteElem.SetAttribute(k.Key, k.Value);
				}
			}
			else
			{
				foreach (KeyValuePair<string, string> k in keyValuesToDelete)
					deleteElem.SetAttribute(k.Key, k.Value);
			}

			xDoc.DocumentElement.AppendChild(deleteElem);

			return new ActionToExport(paramsHelper.CRMAction.ActionName, xDoc.InnerXml);
		}

		///<summary>
		/// Crea un XmlDocument, aggiungendo la Declaration ed il nodo Root
		///</summary>
		//--------------------------------------------------------------------------------
		protected XmlDocument CreateXmlDocument(string rootName, string registeredApp)
		{
			XmlDocument xDoc = new XmlDocument();

			XmlDeclaration xDeclaration = xDoc.CreateXmlDeclaration(CRMInfinityConnectorConsts.XmlVersion, CRMInfinityConnectorConsts.XmlEncoding, null);
			xDoc.AppendChild(xDeclaration);

			XmlElement root = xDoc.CreateElement(rootName);
			root.SetAttribute(CRMInfinityConnectorConsts.ApplicationIdAttr, registeredApp);

			xDoc.AppendChild(root);

			return xDoc;
		}

		///<summary>
		/// aggiunge ad un XmlElement master tutti i suoi attributi letti dal datatable
		/// Di default il test dell'attributo mandatory viene fatto sempre (in update non e' necessario)
		///</summary>
		//--------------------------------------------------------------------------------
        internal bool SetAttributesToMasterElement(CRMAction action, XElement masterElem, DataRow dr, out string keys, out string docTBGuid, out string image, SynchroActionType actionType, bool testIsMandatory = true)
		{
			keys = string.Empty; 
            docTBGuid = string.Empty;
			image = string.Empty;

            if (!action.IgnoreError)
            {
                try
                {
					docTBGuid = string.Empty;
					if (dr.Table.Columns.Contains("TBGuid"))
						docTBGuid = dr["TBGuid"].ToString();
                }
                catch
                {
                    docTBGuid = string.Empty;
                }
            }

			if (action.ActionName.Equals("UploadDocuments"))
			{
				try
				{
					image = dr["Picture"].ToString();
				}
				catch
				{
					image = string.Empty;
				}
			}

			foreach (CRMField crmF in action.Fields)
			{
				if (crmF.InternalUse || masterElem.Attribute(crmF.Target) != null)
					continue;

				try
				{
                    string targetValue = string.Empty; 

                    if (actionType == SynchroActionType.Exclude && crmF.Target.Equals("FLACTIVE"))
                        targetValue = "0";
                    else
                        targetValue = dr[crmF.Target].ToString();

                    if (crmF.Key && !string.IsNullOrWhiteSpace(docTBGuid))
                        keys += string.Format("{0}=\"{1}\" ", crmF.Target, targetValue);

					// solo per gli attachment tengo da parte la chiave (che sarebbe l'AttachmentId)
					if (crmF.Key && actionType == SynchroActionType.NewAttachment)
						keys = targetValue;

					// visto che il DataTable formatta la data come vuole lui, devo rimetterla nel formato atteso (yyyy-mm-dd)
					DataColumn col = dr.Table.Columns[crmF.Target];
					if (col.DataType == typeof(DateTime))
						targetValue = DateTime.Parse(targetValue).ToString(CRMInfinityConnectorConsts.CRMDateFormat);

					// visto che il DataTable formatta i decimali/double e mette la virgola, mentre in Infinity
					// i double devono avere il punto e non la virgola e se necessario tronco i decimali a 5 cifre!
					if (col.DataType == typeof(Double) || col.DataType == typeof(Decimal))
					{
                        targetValue = targetValue.Replace(",", ".");
                        try
                        {
                            // visto che il DataTable formatta i decimali/double e mette la virgola, mentre in Infinity
                            // i double devono avere il punto e non la virgola e se necessario tronco i decimali a 5 cifre!
                            if (col.DataType == typeof(Double) || col.DataType == typeof(Decimal))
                            {
                                targetValue = targetValue.Replace(",", ".");
                                try
                                {
                                    decimal dec = Decimal.Parse(targetValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);

                                    if (dec <= 0.0001m)
                                        targetValue = dec.ToString(CultureInfo.InvariantCulture);

                                    if (targetValue.Contains("."))
                                        if (targetValue.Length - targetValue.IndexOf(".") > 5)
                                        {
                                            if ((col.ColumnName == "LDPREZZO") && (col.Table.TableName == "MA_ItemsPriceLists"))
                                                targetValue = targetValue.Remove(targetValue.IndexOf(".") + 6);
                                            else
                                                targetValue = targetValue.Remove(targetValue.IndexOf(".") + 5);
                                        }
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch
                        {
                        }
                    }

					// gestione indirizzi mail (per ovviare all'errore Dettaglio tecnico [gsfr_acompany:Formato indirizzo mail non valido])
					if ((crmF.Target.Equals("OFMAIL") || crmF.Target.Equals("OFEMAIL2")) && !string.IsNullOrWhiteSpace(targetValue))
						targetValue = GetEmailAddress(targetValue);

                    //masterElem.Add(new XAttribute(crmF.Target, Regex.Replace(targetValue.Trim(), "[^ -~]", "")));

                    Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                    byte[] defBytes = Encoding.Default.GetBytes(targetValue.Trim());
                    byte[] isoBytes = Encoding.Convert(Encoding.Default, iso, defBytes);
                    string msg = iso.GetString(isoBytes);

                    //masterElem.Add(new XAttribute(crmF.Target, Regex.Replace(targetValue.Trim(), "[^ -~]", "")));
                    masterElem.Add(new XAttribute(crmF.Target, msg));
                }
				catch (Exception)
				{
					// nel caso non ci fosse quella colonna (ovvero la dr[x] genera un'eccezione)
					// se il Field e' key o e' mandatory ritorno subito false, altrimenti vado avanti
					if (crmF.Key || (crmF.Mandatory && testIsMandatory))
						return false;
				}
			}

			return true;
		}

		///<summary>
		/// aggiunge ad un XmlElement slave tutti i suoi tag letti dal datatable
		///</summary>
		//--------------------------------------------------------------------------------
        internal bool SetAttributesToSlaveElement(CRMAction subAction, XElement slaveElem, DataRow slaveRow, bool testIsMandatory = true)
		{
			foreach (CRMField crmF in subAction.Fields)
			{
				if (crmF.InternalUse)
					continue;
				try
				{
					string targetValue = slaveRow[crmF.Target].ToString();

					DataColumn col = slaveRow.Table.Columns[crmF.Target];
					// visto che il DataTable formatta la data come vuole lui, devo rimetterla nel formato atteso (yyyy-mm-dd)
					if (col.DataType == typeof(DateTime))
						targetValue = DateTime.Parse(targetValue).ToString(CRMInfinityConnectorConsts.CRMDateFormat);

					// visto che il DataTable formatta i decimali/double e mette la virgola, mentre in Infinity
					// i double devono avere il punto e non la virgola e se necessario tronco i decimali a 5 cifre!
					if (col.DataType == typeof(Double) || col.DataType == typeof(Decimal))
                    {
                        targetValue = targetValue.Replace(",", ".");
						try
						{
							if (targetValue.Contains("."))
								if (targetValue.Length - targetValue.IndexOf(".") > 5)
									targetValue = targetValue.Remove(targetValue.IndexOf(".") + 5);
						}
						catch
						{
						}
                    }

					// gestione indirizzi mail (per ovviare all'errore Dettaglio tecnico [gsfr_acompany:Formato indirizzo mail non valido])
					if ((crmF.Target.Equals("OFMAIL") || crmF.Target.Equals("OFEMAIL2")) && !string.IsNullOrWhiteSpace(targetValue))
						targetValue = GetEmailAddress(targetValue);

                    //slaveElem.Add(new XElement(crmF.Target, Regex.Replace(targetValue.Trim(), "[^ -~]", "")));

                    Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                    byte[] defBytes = Encoding.Default.GetBytes(targetValue.Trim());
                    byte[] isoBytes = Encoding.Convert(Encoding.Default, iso, defBytes);
                    string msg = iso.GetString(isoBytes);

                    //masterElem.Add(new XAttribute(crmF.Target, Regex.Replace(targetValue.Trim(), "[^ -~]", "")));
                    slaveElem.Add(new XElement(crmF.Target, msg));
                }
				catch (Exception)
				{
					// nel caso non ci fosse quella colonna, ovvero la dr[x] genera un'eccezione
					// se il Field e' key o e' mandatory ritorno subito false, altrimenti vado avanti
					if (crmF.Key || (crmF.Mandatory && testIsMandatory))
						return false;
				}
			}

			return true;
		}

		///<summary>
		/// Data una stringa estratta dal database ritorna il primo indirizzo email formattato correttamente
		/// se presente, altrimenti stringa vuota
		///</summary>
		//--------------------------------------------------------------------------------
		private string GetEmailAddress(string email)
		{
			//instantiate with this pattern 
			Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);

			//find items that matches with our pattern
			MatchCollection emailMatches = emailRegex.Matches(email);
			return (emailMatches.Count > 0) ? emailMatches[0].Value : string.Empty;
		}
	}

	///<summary>
	/// Classe generica per i parametri da passare alle classi di Unparser
	///</summary>
	//================================================================================
	internal class InfinityUnparserParams
	{
		public string RegisteredApp { get; set; }
		public CRMAction CRMAction { get; set; }
		public SynchroActionType SyncActionType { get; set; }
		public List<DTValuesToImport> ValDataTablesList { get; set; }
		public ActionDataInfo ActionDataInfo { get; set; } // TODO: passare direttamente la lista con le chiavi, spostare la creazione di questa lista nel padre

		//--------------------------------------------------------------------------------
		public InfinityUnparserParams(string registeredApp, CRMAction action, SynchroActionType syncActionType, List<DTValuesToImport> dtValuesList, ActionDataInfo actionDataInfo)
		{
			RegisteredApp = registeredApp;
			CRMAction = action;
			SyncActionType = syncActionType;
			ValDataTablesList = dtValuesList;
			ActionDataInfo = actionDataInfo;
		}

		///<summary>
		/// Metodo da richiamare per avere l'elenco dei valori delle chiavi da eliminare
		/// ma non e' detto che vada bene per tutti gli unparser! (vedi il caso delle banche)
		///</summary>
		//--------------------------------------------------------------------------------
		public Dictionary<string, string> GetKeyValuesToDelete()
		{
			Dictionary<string, string> keyValuesToDelete = new Dictionary<string, string>(); // elenco valori delle chiavi da eliminare

			// scorro tutti i Field definiti nel solo master e con l'attributo key="true"
			// per ognuno vado a cercare il valore corrispondente memorizzato nella colonna ActionData 
			// esempio: <?xml version="1.0"?> <DeleteRecord> <KeysForDeleteAction InternationalCode="ZF"/> </DeleteRecord>
			// (cerco il contenuto dell'attributo source se presente, altrimenti considero il target)
			// in una lista metto l'elenco dei valori da inserire nell'xml 
			foreach (CRMField f in this.CRMAction.Fields)
			{
				if (!f.Key || f.InternalUse) continue;

                if (!string.IsNullOrWhiteSpace(f.DeleteKey))
                {
                    keyValuesToDelete.Add(f.Target, f.DeleteKey);
                }

				string valToDelete;

				if (!string.IsNullOrWhiteSpace(f.Source))
				{
					foreach (Row r in this.ActionDataInfo.KeysForDeleteActionList)
					{
						if (r.Values.TryGetValue(f.Source, out valToDelete))
							keyValuesToDelete.Add(f.Target, f.DeletePrefix+valToDelete);
					}
				}
				else
				{
					foreach (Row r in this.ActionDataInfo.KeysForDeleteActionList)
					{
						foreach (KeyValuePair<string, string> kvp in r.Values)
						{
							// TODO: non va bene in presenza di record multipli
							if (!keyValuesToDelete.ContainsKey(f.Target)) //  paramento in caso di record multipli
								keyValuesToDelete.Add(f.Target, f.DeletePrefix+kvp.Value);
						}
					}
				}
			}

			return keyValuesToDelete;
		}
	}
}
