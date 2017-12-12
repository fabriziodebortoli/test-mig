using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	//Direzione per la selezione del nodo richiesto
	//=========================================================================
	public enum Direction	{ NEXT = 0, PREVIOUS = 1, NULL = 2 };
	
	// Tipo di Ordinamento 
	//=========================================================================
	public enum OrderType  { None = 0, Date = 1, Type = 2, All = Date | Type};

	// Tipo di Log (per inserire un attributo nel file di log generato dal Diagnostic)
	//=========================================================================
	public enum LogType  { Generic, Application, Database, Migration, WebService, ImportExport };

	/// <summary>
	/// Visualizzazione di messaggi di errori e diagnostica.
	/// </summary>
	//=========================================================================
	public partial class DiagnosticView : System.Windows.Forms.Form
	{
		private const int nReasonableMaxChars = 2000;
		private IDiagnostic diagnostic = null;
		private OrderType initialOrderType = OrderType.Date;
		private DiagnosticType initialDiagnosticType = DiagnosticType.Error | DiagnosticType.Warning | DiagnosticType.Information;

		#region Constructors
		//---------------------------------------------------------------------
		public DiagnosticView() 
			: this (null, OrderType.Date)
		{}

		//---------------------------------------------------------------------
		public DiagnosticView(OrderType orderType) 
			: this (null, orderType)
		{}

		//---------------------------------------------------------------------
		public DiagnosticView(IDiagnostic diagnostic)
			: this(diagnostic, OrderType.Date)
		{}

		//---------------------------------------------------------------------
		public DiagnosticView(IDiagnostic diagnostic, OrderType orderType)
			: this(diagnostic, orderType, false)
		{}

		//---------------------------------------------------------------------
		public DiagnosticView(IDiagnostic diagnostic, DiagnosticType diagnosticType)
		{
			InitializeComponent();

			this.initialOrderType = OrderType.Date;
			this.initialDiagnosticType = diagnosticType;
			this.diagnostic = diagnostic;

			InitView(false);

			LoadMessages();
		}

		//---------------------------------------------------------------------
		public DiagnosticView(IDiagnostic diagnostic, OrderType orderType, bool enableFilter)
		{
			InitializeComponent();
			
			this.initialOrderType = orderType;
			this.diagnostic = diagnostic;
			
			InitView(enableFilter);

			LoadMessages();
		}
		#endregion

		#region Private methods
		//---------------------------------------------------------------------
		private void InitView(bool enableFilter)
		{
			//setto i tag dei bottoni next e previous
            this.PreviousToolStripButton.Tag= Direction.PREVIOUS;
            this.NextToolStripButton.Tag	= Direction.NEXT;		
			
            //Di default la bottoniera di Filtro e Sort sono disabilitate
			this.FilterToolStrip.Visible= enableFilter;
			this.SortToolStrip.Visible	= false;
			
			LblFileName.Text = string.Empty;

			MessagesList.DoubleClick +=new EventHandler(MessagesList_DoubleClick);
		}

		//---------------------------------------------------------------------
		private bool AskSaving()
		{
			DialogResult result =
				MessageBox.Show
				(
					DiagnosticViewerStrings.SaveMessage,
					DiagnosticViewerStrings.SaveLabel,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1
				);

			if (result == DialogResult.No)
				return false;

			if (result == DialogResult.Yes)
			{
				SaveFileDialog saveDlg = new SaveFileDialog();
				saveDlg.CheckPathExists = true;
				saveDlg.Title = DiagnosticViewerStrings.SaveLabel;
				saveDlg.DefaultExt = "*" + Constants.XmlExtension;
				saveDlg.Filter = "XML files|*" + Constants.XmlExtension + "|All files|*.*";

				if (saveDlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(saveDlg.FileName))
					return WriteXmlFile(saveDlg.FileName);
			}

			return true;
		}
	
		/// <summary>
		/// Esegue il parsing di un singolo elemento letto dal file di diagnostica.
		/// </summary>
		/// <remarks>
		/// Da terminare (modificare la DiagnosticManager per poter inserire manualmente anche la data e l'ora - 
		/// attualmente lo fa in automatico a seconda di quando avviene la Set).
		/// </remarks>
		//---------------------------------------------------------------------
		private void Parse(XmlElement message)
		{
			string aValueType = message.GetAttribute(MessagesXML.Attribute.Type);
			string aValueTime = message.GetAttribute(MessagesXML.Attribute.Time);
			string aValueText = string.Empty;

			DiagnosticType itemType = DiagnosticType.None;
			ExtendedInfo extendedInfo = new ExtendedInfo();

			foreach (XmlElement aNode in message.ChildNodes)
			{
				if (aNode.Name == MessagesXML.Element.MessageText)
					aValueText = aNode.GetAttribute(MessagesXML.Attribute.Text);
				else 
					if (aNode.Name == MessagesXML.Element.ExtendedInfos)
					{
						foreach (XmlElement aNodeExtended in aNode.ChildNodes)
							extendedInfo.Add(aNodeExtended.GetAttribute(MessagesXML.Attribute.Name), aNodeExtended.GetAttribute(MessagesXML.Attribute.Value));
					}
			}

			if (aValueType == DiagnosticType.Information.ToString())
				itemType = DiagnosticType.Information;
			else if (aValueType == DiagnosticType.Error.ToString())
				itemType = DiagnosticType.Error;
			else if (aValueType == DiagnosticType.Warning.ToString())
				itemType = DiagnosticType.Warning;

			if (extendedInfo.Count == 0)
				diagnostic.Set(itemType, Convert.ToDateTime(aValueTime), aValueText);
			else
				diagnostic.Set(itemType, Convert.ToDateTime(aValueTime), aValueText, extendedInfo);
		}

		/// <summary>
		/// Aggiunge il singolo messaggio alla finestra.
		/// </summary>
		//---------------------------------------------------------------------
		private void FillMessage(DiagnosticItem itemToAdd)
		{
			if (itemToAdd == null) 
				return;

			if (itemToAdd.ExtendedInfo != null)
				MessagesList.Items.Add(new ParseMessageEventArgs(itemToAdd.Type, itemToAdd.Occurred, itemToAdd.FullExplain, itemToAdd.ExtendedInfo));
			else
				MessagesList.Items.Add(new ParseMessageEventArgs(itemToAdd.Type, itemToAdd.Occurred, itemToAdd.FullExplain));

			if (MessagesList.Items.Count != 0)
			{
				//seleziono il primo elemento
				MessagesList.SelectedItem = MessagesList.Items[0];

				this.PreviousToolStripButton.Enabled = false;
				this.SaveToolStripButton.Enabled = true;

				if (MessagesList.Items.Count == 1)
				{
                    this.NextToolStripButton.Enabled		= false;
                    this.FilterToolStripButton.Enabled		= false;
                    this.SortToolStripButton.Enabled		= false;
				}
				else
				{
                    this.NextToolStripButton.Enabled		= true;
                    this.FilterToolStripButton.Enabled		= true;
                    this.SortToolStripButton.Enabled		= true;
				}
			}
			else
			{
                this.PreviousToolStripButton.Enabled	= false;
                this.NextToolStripButton.Enabled		= false;
                this.FilterToolStripButton.Enabled		= false;
                this.SortToolStripButton.Enabled		= false;
                this.DetailsToolStripButton.Enabled		= false;
                this.SaveToolStripButton.Enabled		= false;		
			}
		}

		//---------------------------------------------------------------------
		private void ViewDetails()
		{
			if (MessagesList.SelectedItems.Count == 0)
				return;

			ParseMessageEventArgs messageDetails = (ParseMessageEventArgs)MessagesList.SelectedItem;

			if (messageDetails.ExtendedInfo != null && messageDetails.ExtendedInfo.Count > 0)
			{
				MessageDetail detailsForm = new MessageDetail(messageDetails);
				detailsForm.Icon = this.Icon;
				detailsForm.ShowDialog(this);
			}
		}

        //---------------------------------------------------------------------
        private void PreviousToolStripButton_Click(object sender, EventArgs e)
        {
            ChangePosition(true);
        }

        //---------------------------------------------------------------------
        private void NextToolStripButton_Click(object sender, EventArgs e)
        {
            ChangePosition(false);
        }

        //---------------------------------------------------------------------
        private void DetailsToolStripButton_Click(object sender, EventArgs e)
        {
            ViewDetails();
        }

        //---------------------------------------------------------------------
        private void FilterToolStripButton_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterToolStrip.Visible = this.FilterToolStripButton.Checked;
        }

        //---------------------------------------------------------------------
        private void SortToolStripButton_CheckedChanged(object sender, EventArgs e)
        {
            this.SortToolStrip.Visible = this.SortToolStripButton.Checked;
        }

        //---------------------------------------------------------------------
        private void SaveToolStripButton_Click(object sender, EventArgs e)
        {
            AskSaving();
        }

        //---------------------------------------------------------------------
        private void OpenToolStripButton_Click(object sender, EventArgs e)
        {
            LoadXmlFile();
        }
		
		//---------------------------------------------------------------------
        private void FilteredView_CheckedChanged(object sender, EventArgs e)
		{
            DiagnosticType filterType = DiagnosticType.None;
            filterType = (this.ViewErrorsToolStripButton.Checked) ? DiagnosticType.Error : filterType;
            filterType = (this.ViewWarningsToolStripButton.Checked) ? filterType | DiagnosticType.Warning : filterType;
            filterType = (this.ViewInfoToolStripButton.Checked) ? filterType | DiagnosticType.Information : filterType;

			// ora valuto il criterio di ordinamento
			OrderType orderType = OrderType.None;
			orderType = (this.SortByDateTimeToolStripButton.Checked) ? OrderType.Date: orderType;
			orderType = (this.SortByTypeToolStripButton.Checked) ? orderType | OrderType.Type: orderType;

			//applico i settings
            FillListOfMessages(orderType, filterType);
        }

		//---------------------------------------------------------------------
		private void ChangePosition(bool previous)
		{
			if (MessagesList.SelectedItem == null ) 
                return;
			
            int pos = MessagesList.Items.IndexOf(MessagesList.SelectedItem);

            if (previous && pos > 0 && pos < MessagesList.Items.Count)
                MessagesList.SelectedItem = MessagesList.Items[pos - 1];
            else if (!previous && pos >= 0 && pos + 1 < MessagesList.Items.Count)
                MessagesList.SelectedItem = MessagesList.Items[pos + 1];

			MessagesList.Invalidate();
		}

		/// <summary>
		/// Selezione di un item nella lista. Abilito/disabilito i bottoni del caso.
		/// </summary>
		//---------------------------------------------------------------------
		private void MessagesList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (MessagesList.SelectedItems.Count == 0) 
				return;

			if (MessagesList.SelectedIndex == 0)
			{
                this.PreviousToolStripButton.Enabled = false;
                this.NextToolStripButton.Enabled = true;	
			}
			else if (MessagesList.SelectedIndex == MessagesList.Items.Count - 1)
			{
                this.PreviousToolStripButton.Enabled= true;
                this.NextToolStripButton.Enabled	= false;			
			}
			else
			{
                this.PreviousToolStripButton.Enabled= true;
                this.NextToolStripButton.Enabled	= true;
			}

			ParseMessageEventArgs details = (ParseMessageEventArgs)MessagesList.SelectedItem;

			if (details.ExtendedInfo == null || details.ExtendedInfo.Count == 0)
				this.DetailsToolStripButton.Enabled = false;
			else
                this.DetailsToolStripButton.Enabled = true;
		}

		//---------------------------------------------------------------------
		private void BtnClose_Click(object sender, System.EventArgs e)
		{
			Close();
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Carica un file xml, ne fa il parse e lo visualizza nel controllo.
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadXmlFile()
		{
			LoadXmlFile(string.Empty);
		}

		/// <summary>
		/// Carica un file xml specificato, ne fa il parse e lo visualizza nel controllo.
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadXmlFile(string filePath)
		{
			string fileToShow = string.Empty;
			
			if (filePath.Length == 0)
			{
				OpenFileDialog openDlg = new OpenFileDialog();
				openDlg.CheckFileExists = true;
				openDlg.Title = DiagnosticViewerStrings.OpenLabel;
				openDlg.DefaultExt = "*" + Constants.XmlExtension;
				openDlg.Filter = "XML files|*" + Constants.XmlExtension + "|All files|*.*";

				if (openDlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(openDlg.FileName))
					fileToShow = openDlg.FileName;
			}
			else
				fileToShow = filePath;

			if (string.IsNullOrWhiteSpace(fileToShow))
				return;

			MessagesList.Items.Clear();

			if (diagnostic != null)
				diagnostic.Clear();
			else
				diagnostic = new Diagnostic(fileToShow);

			if (ReadXmlFile(fileToShow))
				LblFileName.Text = fileToShow;

			FillListOfMessages(this.initialOrderType, DiagnosticType.Error | DiagnosticType.Warning | DiagnosticType.Information);
			this.Show();
		}

		/// <summary>
		/// Dato un file xml, lo legge e restituisce una struttura di tipo Diagnostic.
		/// </summary>
		//---------------------------------------------------------------------
		public bool ReadXmlFile(string filePath)
		{
			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.Load(filePath);
				XmlElement root = xDoc.DocumentElement;

				if (root == null)
				{
					Debug.Fail(string.Format(DiagnosticViewerStrings.CannotReadXmlFile, filePath));
					diagnostic.Set(DiagnosticType.Error, string.Format(DiagnosticViewerStrings.CannotReadXmlFile, filePath));
					return false;
				}

				foreach (XmlNode aNode in root.ChildNodes)
				{
					if (aNode.NodeType != XmlNodeType.Element)
						continue;
					if (aNode.Name == MessagesXML.Element.File)
						continue;

					XmlElement aElement = aNode as XmlElement;
					foreach (XmlElement aElementNode in aElement.ChildNodes)
						Parse(aElementNode);
				}
			}
			catch (XmlException err)
			{
				Debug.Fail(err.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DiagnosticViewerStrings.Description, err.Message);
				extendedInfo.Add(DiagnosticViewerStrings.Source, "ReadXmlFile");
				extendedInfo.Add(DiagnosticViewerStrings.LinePosition, err.LinePosition.ToString());
				extendedInfo.Add(DiagnosticViewerStrings.LineNumber, err.LineNumber.ToString());
				diagnostic.Set(DiagnosticType.Error, string.Format(DiagnosticViewerStrings.CannotReadXmlFile, filePath), extendedInfo);
				return false;
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DiagnosticViewerStrings.Description, exc.Message);
				extendedInfo.Add(DiagnosticViewerStrings.Source, "ReadXmlFile");
				diagnostic.Set(DiagnosticType.Error, string.Format(DiagnosticViewerStrings.CannotReadXmlFile, filePath), extendedInfo);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Scrive il file XML.
		/// </summary>
		//---------------------------------------------------------------------
		public bool WriteXmlFile(string filePath)
		{
			return WriteXmlFile(filePath, LogType.Generic);
		}
		
		/// <summary>
		/// Scrive il file XML.
		/// </summary>
		//---------------------------------------------------------------------
		public bool WriteXmlFile(string filePath, LogType type)
		{
			try
			{
				XmlDocument xDoc = new XmlDocument();
				// Intestazione
				XmlDeclaration xDec = 
					xDoc.CreateXmlDeclaration(Constants.XmlDeclarationVersion, Constants.XmlDeclarationEncoding, null);
				xDoc.AppendChild(xDec);

				// nome, data e ora di creazione, tipo di log
				XmlElement xLog = xDoc.CreateElement(MessagesXML.Element.File);
				xLog.SetAttribute(MessagesXML.Attribute.Name, Path.GetFileName(filePath));
				xLog.SetAttribute(MessagesXML.Attribute.CreationDate, DateTime.UtcNow.ToString("s"));
				xLog.SetAttribute(MessagesXML.Attribute.LogType, type.ToString());
				xDoc.AppendChild(xLog);

				// Nodo Messages
				XmlElement xMessages = xDoc.CreateElement(MessagesXML.Element.Messages);
				xDoc.DocumentElement.AppendChild(xMessages);
				// Popolo il nodo con tutti i Messaggi trovati
				foreach (ParseMessageEventArgs itemSelected in MessagesList.Items)
					xDoc = UnparseMessages(xDoc, itemSelected);

				FileInfo fileInfo = new FileInfo(filePath);
				if (fileInfo.Exists &&
					(fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					fileInfo.Attributes -= FileAttributes.ReadOnly;

				// scrivo il file xml, in formato indentato
				XmlTextWriter tr = new XmlTextWriter(filePath, null);
				tr.Formatting = Formatting.Indented;
				xDoc.WriteContentTo(tr);
				tr.Close();	
			}
			catch (XmlException exc)
			{
				Debug.Fail(exc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DiagnosticViewerStrings.Description, exc.Message);
				extendedInfo.Add(DiagnosticViewerStrings.Source, exc.Source);
				extendedInfo.Add(DiagnosticViewerStrings.LinePosition, exc.LinePosition.ToString());
				extendedInfo.Add(DiagnosticViewerStrings.LineNumber, exc.LineNumber.ToString());
				diagnostic.Set(DiagnosticType.Error, string.Format(DiagnosticViewerStrings.CannotWriteXmlFile, filePath), extendedInfo);
				return false;
			}
			catch (IOException ioEx)
			{
				Debug.Fail(ioEx.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DiagnosticViewerStrings.Description, ioEx.Message);
				extendedInfo.Add(DiagnosticViewerStrings.Source, ioEx.Source);
				diagnostic.Set(DiagnosticType.Error, string.Format(DiagnosticViewerStrings.CannotWriteXmlFile, filePath), extendedInfo);
				return false;
			}
			catch (UnauthorizedAccessException uaeEx)
			{
				Debug.Fail(uaeEx.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DiagnosticViewerStrings.Description, uaeEx.Message);
				extendedInfo.Add(DiagnosticViewerStrings.Source, uaeEx.Source);
				diagnostic.Set(DiagnosticType.Error, string.Format(DiagnosticViewerStrings.CannotWriteXmlFile, filePath), extendedInfo);
				return false;
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DiagnosticViewerStrings.Description, e.Message);
				extendedInfo.Add(DiagnosticViewerStrings.Source, e.Source);
				diagnostic.Set(DiagnosticType.Error, string.Format(DiagnosticViewerStrings.CannotWriteXmlFile, filePath), extendedInfo);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Scrive un nodo Message dentro il nodo Messages nel file XML.
		/// </summary>
		//---------------------------------------------------------------------
		public XmlDocument UnparseMessages(XmlDocument xmlDocument, ParseMessageEventArgs messageItem)
		{
			//Creo il nodo Message
			XmlNode message = xmlDocument.CreateElement(MessagesXML.Element.Message);
			//gli aggiungo l'Attributo Type
			XmlAttribute typeAttribute = xmlDocument.CreateAttribute(MessagesXML.Attribute.Type);
			typeAttribute.Value = (messageItem.MessageType == DiagnosticType.None) 
				? "Information" //DiagnosticSimpleItemsStrings.Info
				: messageItem.MessageType.ToString();

			message.Attributes.SetNamedItem(typeAttribute);

			//gli aggiungo l'Attributo Time
			XmlAttribute timeAttribute = xmlDocument.CreateAttribute(MessagesXML.Attribute.Time);
			timeAttribute.Value = messageItem.Time.ToString("s");
			message.Attributes.SetNamedItem(timeAttribute);

			//creo il sottonodo MessageText
			XmlNode messageText = xmlDocument.CreateElement(MessagesXML.Element.MessageText);

			//gli aggiungo l'Attributo Message
			XmlAttribute messageAttribute = xmlDocument.CreateAttribute(MessagesXML.Attribute.Text);
			messageAttribute.Value = messageItem.MessageText;
			messageText.Attributes.SetNamedItem(messageAttribute);
			message.AppendChild(messageText);

			//gli aggiungo l'Attributo ExtendedMessage
			if (messageItem.ExtendedInfo != null && messageItem.ExtendedInfo.Count > 0)
			{
				//creo il sottonodo ExtendedInfo
				XmlNode messageExtendedInfo = xmlDocument.CreateElement(MessagesXML.Element.ExtendedInfos);

				foreach (ExtendedInfoItem extendedInfoItem in messageItem.ExtendedInfo)
				{
					XmlNode itemExtendedInfo = xmlDocument.CreateElement(MessagesXML.Element.ExtendedInfo);
					XmlAttribute itemExtendedInfoName = xmlDocument.CreateAttribute(MessagesXML.Attribute.Name);
					itemExtendedInfoName.Value = extendedInfoItem.Name;
					itemExtendedInfo.Attributes.SetNamedItem(itemExtendedInfoName);
					XmlAttribute itemExtendedInfoValue = xmlDocument.CreateAttribute(MessagesXML.Attribute.Value);
				
					itemExtendedInfoValue.Value = extendedInfoItem.Info.ToString();
					itemExtendedInfo.Attributes.SetNamedItem(itemExtendedInfoValue);
					messageExtendedInfo.AppendChild(itemExtendedInfo);
				}
				message.AppendChild(messageExtendedInfo);
			}

			//cerco il gruppo Messages
			XmlNodeList XmlNodeMessages = xmlDocument.GetElementsByTagName(MessagesXML.Element.Messages);

			//ci aggiungo il nodo
			XmlNode XmlNodeMessage = XmlNodeMessages[0].AppendChild(message);

			return xmlDocument;
		}

		/// <summary>
		/// Aggiunge un singolo messaggio alla lista.
		/// </summary>
		//---------------------------------------------------------------------
		public void Add(DiagnosticItem itemToAdd)
		{
			// aggiungo al diagnostico corrente il messaggio
			diagnostic.Set(itemToAdd.Type, itemToAdd.Occurred, itemToAdd.Explain.ToString(), itemToAdd.ExtendedInfo);
			RefreshMessage(itemToAdd);
		}

		/// <summary>
		/// Legge i dati e crea la view da visualizzare nel datagrid.
		/// </summary>
		//---------------------------------------------------------------------
		public void FillListOfMessages(OrderType orderType, DiagnosticType typeOfMessages)
		{
			if (diagnostic == null)
				return;

			//carico tutti gli errori
			DiagnosticItems items = diagnostic.AllMessages(typeOfMessages) as DiagnosticItems;
			if (items == null)
				return; //non ne ho trovati, esco

			MessagesList.Items.Clear();

			switch (orderType)
			{
				case OrderType.Date:
				{
					IComparer comparer = new CustomSortDateTime();
					items.Sort(0, items.Count, comparer);
					break;
				}
				case OrderType.Type:
				{ 
					IComparer comparer = new CustomSortType();
					items.Sort(0, items.Count,comparer);
					break;
				}
				case OrderType.All:
				{
					IComparer comparer = new CustomSortDateType();
					items.Sort(0, items.Count, comparer);
					break;
				}
			}

			foreach (DiagnosticItem item in items)
			{
				if ((typeOfMessages & item.Type) == item.Type)
				{
					int nSplits = item.FullExplain.Length / nReasonableMaxChars;
					for (int i = 0; i <= nSplits; i++)
					{
						int nStart = i * nReasonableMaxChars;
						int nHowRemains = item.FullExplain.Length - nStart;
						if (nHowRemains <= 0)
							break;

						string splittedText = item.FullExplain.Substring(nStart, nHowRemains < nReasonableMaxChars ? nHowRemains : nReasonableMaxChars);
						if (item.ExtendedInfo != null && item.ExtendedInfo.Count > 0)
							MessagesList.Items.Add(new ParseMessageEventArgs(item.Type, item.Occurred, splittedText, item.ExtendedInfo));
						else
							MessagesList.Items.Add(new ParseMessageEventArgs(item.Type, item.Occurred, splittedText));
					}
				}
			}

			MessagesList.Invalidate();

			// ripristinato come prima xchè in fase di pulizia è stato inserito involontariamente un bug 
			// (ovvero in presenza di un solo messaggio nella listview rimaneva abilitato il pulsante Next 
			if (MessagesList.Items.Count != 0)
			{
				// seleziono il primo elemento
				MessagesList.SelectedItem = MessagesList.Items[0];
				
				this.PreviousToolStripButton.Enabled = false;
				this.SaveToolStripButton.Enabled = true;

				if (MessagesList.Items.Count == 1)
				{
                    this.NextToolStripButton.Enabled	= false;
                    this.FilterToolStripButton.Enabled	= false;
					this.SortToolStripButton.Enabled	= false;
				}
				else
				{
                    this.NextToolStripButton.Enabled		= true;
					this.FilterToolStripButton.Enabled	    = true;
                    this.SortToolStripButton.Enabled		= true;
				}
			}
			else
			{
                this.PreviousToolStripButton.Enabled	= false;
                this.NextToolStripButton.Enabled		= false;
                this.FilterToolStripButton.Enabled		= false;
                this.SortToolStripButton.Enabled		= false;
                this.DetailsToolStripButton.Enabled		= false;
                this.SaveToolStripButton.Enabled		= false;		
			}
		}

		//---------------------------------------------------------------------
		public int SetDiagnosticImage(DiagnosticType typeOfMessage)
		{
			int indexOfImage = 0;

			if (typeOfMessage == DiagnosticType.Error)
				indexOfImage = 5;
			else if (typeOfMessage == DiagnosticType.Warning)
				indexOfImage = 7;
			else indexOfImage = 6;

			return indexOfImage;
		}

		/// <summary>
		/// Carica i messaggi nella listview
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadMessages()
		{
			//Default - Non imposto filtri e visualizzo Errori, Warning e None (=Information)
			FillListOfMessages(this.initialOrderType, this.initialDiagnosticType);
		}

		/// <summary>
		/// Effettua il refresh del singolo messaggio.
		/// </summary>
		//---------------------------------------------------------------------
		private void RefreshMessage(DiagnosticItem itemToAdd)
		{
			FillMessage(itemToAdd);
		}
		#endregion

        //---------------------------------------------------------------------
        private void MessagesList_DoubleClick(object sender, EventArgs e)
		{
			ViewDetails();
		}
	}

	/// <summary>
	/// Effettua un ordinamento dei messaggi per data e ora.
	/// < 0 se d1 < d2
	/// = 0 se d1 = d2
	/// > 0 se d1 > d2
	/// </summary>
	//=========================================================================
	public class CustomSortDateTime : IComparer
	{
		int IComparer.Compare(Object d1, Object d2)
		{
			return DateTime.Compare
				(
					((DiagnosticItem)d2).Occurred,
					((DiagnosticItem)d1).Occurred
				);
		}
	}

	/// <summary>
	/// Effettua un ordinamento dei messaggi per Type.
	/// </summary>
	//=========================================================================
	public class CustomSortType : IComparer
	{
		int IComparer.Compare(Object d1, Object d2)
		{
			Debug.Assert(d1 != null);
			Debug.Assert(d2 != null);
			Debug.Assert(d1.GetType() == d2.GetType());
			if (d1.GetType() != d2.GetType())
				throw new ArgumentException();
			if (d1 == null && d2 == null)
				return 0;
			if (d1 == null) return -1;
			if (d2 == null) return 1;

			return ((DiagnosticItem)d1).Type.CompareTo(((DiagnosticItem)d2).Type);
		}
	}

	/// <summary>
	/// Effettua un ordinamento dei messaggi per Type e Data.
	/// </summary>
	//=========================================================================
	public class CustomSortDateType : IComparer
	{
		int IComparer.Compare(Object d1, Object d2)
		{
			Debug.Assert(d1 != null);
			Debug.Assert(d2 != null);
			Debug.Assert(d1.GetType() == d2.GetType());
			if (d1.GetType() != d2.GetType())
				throw new ArgumentException();
			if (d1 == null && d2 == null)
				return 0;
			if (d1 == null) return -1;
			if (d2 == null) return 1;

			DiagnosticItem di1 = (DiagnosticItem)d1;
			DiagnosticItem di2 = (DiagnosticItem)d2;
			
			// ordered by DateTime DESC
			int compareDate = DateTime.Compare(di2.Occurred, di1.Occurred);

			int compareType = di1.Type.CompareTo(di2.Type);
			return (compareType == 0) ? compareDate : compareType;
		}
	}
}