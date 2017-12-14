using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.EasyAttachment.UI.Controls;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TBPicComponents;

namespace Microarea.EasyAttachment.UI.Forms
{
	///<summary>
	/// Form con le opzioni di acquisizione da device esterna (tipo scanner e simili)
	///</summary>
	//================================================================================
	public partial class Acquisition : Form
	{
		private static TBPicImaging gdPicture = null;
		private static TBPicPDF gdPDF = null;
		private static readonly object staticLockGdPicture = new object(); // per gestire il lock del GdPictureImaging
		private static readonly object staticLockGdPicturePDF = new object(); // per gestire il lock del GdPicturePDF

		private string selectedExtension = string.Empty; // estensione del file selezionato

		// variabili di appoggio
		private string pdfImageName = string.Empty;
		private int tiffID = -1;
		private int nTIFFImageCount = 0;

		private Diagnostic acquisitionDiagnostic = new Diagnostic("Acquisition");

        //--------------------------------------------------------------------------------
        public bool SplitOnBarCode = false;

		// properties
		//--------------------------------------------------------------------------------
		public List<string> AcquiredFileList { get; private set; } // lista di file acquisiti da archiviare

		//--------------------------------------------------------------------------------
        public static TBPicImaging GdPicture
		{
			get
			{
				lock (staticLockGdPicture)
				{
					if (gdPicture == null)
                        gdPicture = new TBPicImaging();

					return gdPicture;
				}
			}
		}

		//--------------------------------------------------------------------------------
        public static TBPicPDF GdPicturePDF
		{
			get
			{
				lock (staticLockGdPicturePDF)
				{
					if (gdPDF == null)
                        gdPDF = new TBPicPDF();

					return gdPDF;
				}
			}
		}
        
		///<summary>
		/// Metodo statico da richiamare per l'apertura della form
		/// Prima vengono effettuati i dovuti controlli
		///</summary>
		//--------------------------------------------------------------------------------
        public static List<string> OpenForm(BarcodeManager barcodeManager, bool forceSplitOnBarcode = false)
		{
			if (!GdPicture.TwainIsAvailable())
			{
				DiagnosticViewer.ShowCustomizeIconMessage(Strings.TwainNotInstalled, string.Empty, MessageBoxIcon.Error);
				return null;
			}
           
			Acquisition acquisitionForm = new Acquisition( barcodeManager);
            acquisitionForm.SplitOnBarCode = forceSplitOnBarcode;
			acquisitionForm.ShowDialog();

			// chiudo la form e ritorno la lista dei path dei file generati dall'acquisizione
			return acquisitionForm.AcquiredFileList;
		}


        private BarcodeManager barcodeManager = null;
		///<summary>
		/// Costruttore privato
		/// La new e Show della form avviene richiamando il metodo statico OpenForm()
		///</summary>
		//--------------------------------------------------------------------------------
        private Acquisition(BarcodeManager barcodeManager)
		{
            InitializeComponent();
			this.barcodeManager = barcodeManager;

			AcquiredFileList = new List<string>();

			// inizializzo la textbox con un nome temporaneo per il file da generare
			FileNameTextBox.Text = Utils.GetFileNameToScan();

			// carico le estensioni predefinite nella combobox
			PopulateExtensionsImageComboBox();

			// apro subito il source di default
			if (GdPicture.TwainOpenDefaultSource(this.Handle))
			{
				string source = GdPicture.TwainGetDefaultSourceName(this.Handle);
				if (!string.IsNullOrWhiteSpace(source))
					SourceTextBox.Text = source;
			}
		}

		///<summary>
		/// apre la dialog predefinita di TWAIN che consente di scegliere tra i source disponibili
		///</summary>
		//--------------------------------------------------------------------------------
		private void ChangeSourceButton_Click(object sender, EventArgs e)
		{
			// metodo che visualizza la dialog predefinita di TWAIN con l'elenco dei source disponibili
			// (sono visibili anche i source WIA)
			if (!GdPicture.TwainSelectSource(this.Handle))
			{
				// visualizzo un msg solo in caso di failure (se l'utente clicca su Cancel non viene fatto nulla)
				if (GdPicture.TwainGetLastResultCode() == TbPicTwainResultCode.TWRC_FAILURE)
					DiagnosticViewer.ShowCustomizeIconMessage(Strings.TwainUnableToSelectSource, string.Empty, MessageBoxIcon.Error);
				return;
			}

			string source = GdPicture.TwainGetDefaultSourceName(this.Handle);
			if (!string.IsNullOrWhiteSpace(source))
			{
				if (!GdPicture.TwainOpenSource(this.Handle, source))
					return; //  un messaggio viene visualizzato in automatico

				SourceTextBox.Text = source;
			}
		}

		///<summary>
		/// Popolo la combobox delle estensioni
		///</summary>
		//--------------------------------------------------------------------------------
		private void PopulateExtensionsImageComboBox()
		{
			// carico la lista delle estensioni disponibili per le operazioni di scanner (le ritorna in uppercase)
			IList<string> list = Utils.GetExtensionsForScan();

			// aggancio alla combobox l'elenco di estensioni disponibili aggiungendo prima una bitmap
			foreach (string extension in list)
				FileTypeComboBox.Items.Add(new ExtensionImageComboBox.ImageComboItem(extension));

			if (list.Count > 0)
				FileTypeComboBox.SelectedIndex = 0;
		}

		///<summary>
		/// Selected index nella combobox con le estensioni
		///</summary>
		//--------------------------------------------------------------------------------
		private void FileTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (((ExtensionImageComboBox)sender).SelectedIndex < 0)
				return;

			ExtensionImageComboBox.ImageComboItem selectedItem =
				((ExtensionImageComboBox.ImageComboItem)(((ExtensionImageComboBox)sender).SelectedItem));

			if (selectedItem == null)
				return;

			selectedExtension = selectedItem.ICText;

			// nel caso in cui per qualche motivo strano non abbiamo un estensione metto di default PDF
			if (string.IsNullOrWhiteSpace(selectedExtension))
				selectedExtension = FileExtensions.Pdf;

			// nel dubbio metto l'estensione lowercase, in modo da non aver problemi nei confronti
			selectedExtension = selectedExtension.ToLowerInvariant();

			// se l'estensione e' PDF o TIFF (o TIF) abilito la checkbox per il multipage
            CBCreateSeparateFile.Enabled = FileExtensions.IsPdfString(selectedExtension) || FileExtensions.IsTifString(selectedExtension);
		}

		///<summary>
		/// Avvia il processo di acquisizione
		///</summary>
		//--------------------------------------------------------------------------------
		private void ScanButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(SourceTextBox.Text))
			{
				DiagnosticViewer.ShowCustomizeIconMessage(Strings.TwainSelectValidSource, string.Empty, MessageBoxIcon.Warning);
				return;
			}

			// se il nome del file e' vuoto compongo al volo un nome temporaneo
			if (string.IsNullOrWhiteSpace(FileNameTextBox.Text))
				FileNameTextBox.Text = Utils.GetFileNameToScan();

			// richiamo la procedura di acquisizione dei file da scanner
			RunScanProcess();

			this.Close();
		}

		///<summary>
		/// Algoritmo di acquisizione delle immagini da scanner e relativo salvataggio 
		///</summary>
		//--------------------------------------------------------------------------------
		private void RunScanProcess()
		{
            //casi di acquisizione di multipli file
            if (RBBarcode.Checked)
            {
                switch (selectedExtension)
				{
					case FileExtensions.Tiff:
                        AcquiredFileList.AddRange(MultipleScanWithBarcodeAsSeparator.ScanTif(FileNameTextBox.Text, barcodeManager)); 
						break;
					case FileExtensions.Pdf:
                        AcquiredFileList.AddRange(MultipleScanWithBarcodeAsSeparator.ScanPdf(FileNameTextBox.Text, barcodeManager)); 
						break;
                }
                return;
            }
            else if (RBSeparatorSheet.Checked)
            {
                switch (selectedExtension)
                {
					case FileExtensions.Tiff:
                        AcquiredFileList.AddRange(MultipleScanWithBarcodeSeparatorSheet.ScanTif(FileNameTextBox.Text, barcodeManager)); 
						break;
					case FileExtensions.Pdf:
                        AcquiredFileList.AddRange(MultipleScanWithBarcodeSeparatorSheet.ScanPdf(FileNameTextBox.Text, barcodeManager)); 
						break;
                }
                return;
            }
            
            string acquiredFilePath = string.Empty;
			TBPictureStatus gdStatus = TBPictureStatus.GenericError;
			
			try
			{
				if (GdPicture.TwainOpenDefaultSource(this.Handle))
				{
					// N.B. il source deve essere aperto per impostare queste proprieta'!!!!
					GdPicture.TwainSetAutoFeed(true); // Set AutoFeed Enabled
					GdPicture.TwainSetAutoScan(true); // To achieve the maximum scanning rate
					GdPicture.TwainSetHideUI(false); // mostro la finestra dei parametri del driver
					GdPicture.TwainSetIndicators(true);

					int nImage = 0; // serve per la gestione della numerazione in caso di salvataggio multiplo di file (laddove il driver lo consenta)

					do
					{
						int imageId = GdPicture.TwainAcquireToGdPictureImage(this.Handle);

						// se l'immagine e' stata acquisita con successo procedo al suo salvataggio su file system
						if (imageId != 0)
						{
							nImage++;

							switch (selectedExtension)
							{
								case FileExtensions.Bmp:
									{
										acquiredFilePath = Path.Combine(barcodeManager.DMSOrchestrator.EasyAttachmentTempPath, FileNameTextBox.Text);
										if (nImage > 1) // nel caso di acquisizione di piu' pagine aggiungo il numero in fondo dalla seconda pagina in poi
											acquiredFilePath += ("_" + nImage.ToString());
										acquiredFilePath += FileExtensions.DotBmp;
										
										gdStatus = GdPicture.SaveAsBMP(imageId, acquiredFilePath);
										if (gdStatus == TBPictureStatus.OK)
											AcquiredFileList.Add(acquiredFilePath);
										break;
									}
								case FileExtensions.Gif:
									{
										acquiredFilePath = Path.Combine(barcodeManager.DMSOrchestrator.EasyAttachmentTempPath, FileNameTextBox.Text);
										if (nImage > 1) // nel caso di acquisizione di piu' pagine aggiungo il numero in fondo dalla seconda pagina in poi
											acquiredFilePath += ("_" + nImage.ToString());
										acquiredFilePath += FileExtensions.DotGif;
										
										gdStatus = GdPicture.SaveAsGIF(imageId, acquiredFilePath);
                                        if (gdStatus == TBPictureStatus.OK)
											AcquiredFileList.Add(acquiredFilePath);
										break;
									}
								case FileExtensions.Jpeg:
									{
										acquiredFilePath = Path.Combine(barcodeManager.DMSOrchestrator.EasyAttachmentTempPath, FileNameTextBox.Text);
										if (nImage > 1) // nel caso di acquisizione di piu' pagine aggiungo il numero in fondo dalla seconda pagina in poi
											acquiredFilePath += ("_" + nImage.ToString());
										acquiredFilePath += FileExtensions.DotJpeg;
										
										gdStatus = GdPicture.SaveAsJPEG(imageId, acquiredFilePath);
                                        if (gdStatus == TBPictureStatus.OK)
											AcquiredFileList.Add(acquiredFilePath);
										break;
									}
								case FileExtensions.Png:
									{
										acquiredFilePath = Path.Combine(barcodeManager.DMSOrchestrator.EasyAttachmentTempPath, FileNameTextBox.Text);
										if (nImage > 1) // nel caso di acquisizione di piu' pagine aggiungo il numero in fondo dalla seconda pagina in poi
											acquiredFilePath += ("_" + nImage.ToString());
										acquiredFilePath += FileExtensions.DotPng;

										gdStatus = GdPicture.SaveAsPNG(imageId, acquiredFilePath);
                                        if (gdStatus == TBPictureStatus.OK)
											AcquiredFileList.Add(acquiredFilePath);
										break;
									}
								case FileExtensions.Pdf:
									{
										if (string.IsNullOrWhiteSpace(pdfImageName))
										{
											// il nome del path lo rigenero solo se pdfImageName e' empty, ovvero si tratta della prima pagina
											acquiredFilePath = Path.Combine(barcodeManager.DMSOrchestrator.EasyAttachmentTempPath, FileNameTextBox.Text);
											if (nImage > 1)
												acquiredFilePath += ("_" + nImage.ToString());
											acquiredFilePath += FileExtensions.DotPdf;
										}
										gdStatus = SaveAsPDF(imageId, acquiredFilePath);
										break;
									}
								case FileExtensions.Tiff:
									{
										if (tiffID == -1)
										{
											// il nome del path lo rigenero solo se tiffID e' -1, ovvero si tratta della prima pagina
											acquiredFilePath = Path.Combine(barcodeManager.DMSOrchestrator.EasyAttachmentTempPath, FileNameTextBox.Text);
											if (nImage > 1)
												acquiredFilePath += ("_" + nImage.ToString());
                                            acquiredFilePath += FileExtensions.DotTiff;
										}
										nTIFFImageCount++;
										gdStatus = SaveAsTIFF(imageId, acquiredFilePath);
										break;
									}
							}
						}
					} // loop sullo stato (nel caso di multipagina e' impostato con TWAIN_TRANSFER_READY)
					while (Convert.ToInt64(GdPicture.TwainGetState()) > Convert.ToInt64(TBPicTwainStatus.TWAIN_SOURCE_ENABLED)); 

					// se lo stato e' OK e l'estensione e' PDF o TIFF salvo l'eventuale multipagina e aggiungo il file acquisito alla lista
					// (non lo faccio prima perche' se qualcuno ha scelto il multipagina devo prima fare il loop su tutte le pagine memorizzate dal driver)
					if (gdStatus == TBPictureStatus.OK)
					{
                        if (FileExtensions.IsPdfString(selectedExtension))
						{
							// se pdfImageName e' valorizzata allora devo salvare il file PDF come multipagina
							if (!string.IsNullOrWhiteSpace(pdfImageName))
							{
								GdPicturePDF.SaveToFile(acquiredFilePath);
								AcquiredFileList.Add(acquiredFilePath);
							}
						}

                        if (FileExtensions.IsTifString(selectedExtension))
						{
							if (tiffID > 0) // se tiffID e' buono allora devo salvare il file TIFF come multipagina
							{
								GdPicture.TiffCloseMultiPageFile(tiffID);
								AcquiredFileList.Add(acquiredFilePath);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				ExtendedInfo ei = null;
				// se il message contiene dei valori assegno le variabili dell'eccezione, altrimenti
				// assegno solo le info base
				if (!string.IsNullOrWhiteSpace(e.Message))
				{
					ei = new ExtendedInfo();
					ei.Add(Strings.Description, e.Message);
					ei.Add("GdPictureStatus", gdStatus.ToString());
					ei.Add(Strings.Source, e.Source);
					ei.Add(Strings.StackTrace, e.StackTrace);
					ei.Add(Strings.Function, "RunScanProcess");
					ei.Add(Strings.Library, "Microarea.EasyAttachment.UI.Forms");
				}
				acquisitionDiagnostic.Set(DiagnosticType.Error, string.Format(Strings.TwainErrorAcquiringFile, selectedExtension), ei);
				using (SafeThreadCallContext context = new SafeThreadCallContext())
					DiagnosticViewer.ShowDiagnosticAndClear(acquisitionDiagnostic);
			}
			finally
			{
				GdPicture.TwainCloseSource();
			}
		}

		///<summary>
		/// Salvataggio di un file TIFF a pagina singola oppure, se richiesto, gestione del multipagina con append delle pagine
		/// successive alla prima
		///</summary>
		//--------------------------------------------------------------------------------
        private TBPictureStatus SaveAsTIFF(int imageID, string acquiredFilePath)
		{
            TBPictureStatus gdStatus = TBPictureStatus.OK;

			// se nTIFFImageCount == 1 so tratta della prima pagina
			if (nTIFFImageCount == 1)
			{
				tiffID = imageID;
				// creo il file TIFF e aggiungo la prima pagina
				gdStatus = GdPicture.TiffSaveAsMultiPageFile(tiffID, acquiredFilePath, TBPicTiffCompression.TiffCompressionAUTO);

                if (RBCreateSeparateFile.Checked && RBCreateSeparateFile.Enabled)
				{
					// se l'utente ha scelto di creare un singolo file per immagine
					// salvo il TIFF e reimposto il tiffID = -1 (cosi' non lo risalvo piu' alla fine del loop)
					// e ri-azzero il count delle pagine
					gdStatus = GdPicture.TiffCloseMultiPageFile(tiffID);
					// aggiungo il file alla lista
					AcquiredFileList.Add(acquiredFilePath);

					tiffID = -1;
					nTIFFImageCount = 0;
				}
			}
			else
			{
				// dalla seconda volta in poi passo a qui e vado in append con le pagine
				// si tratta di un documento multipagina
				gdStatus = GdPicture.TiffAddToMultiPageFile(tiffID, imageID);
				GdPicture.ReleaseGdPictureImage(imageID);
			}

			return gdStatus;
		}

		///<summary>
		/// Salvataggio di un file PDF a pagina singola oppure, se richiesto, gestione del multipagina con append delle pagine
		/// successive alla prima
		///</summary>
		//--------------------------------------------------------------------------------
        private TBPictureStatus SaveAsPDF(int imageID, string acquiredFilePath)
		{
            TBPictureStatus gdStatus = TBPictureStatus.OK;

			// se pdfImageName e' empty e' la prima volta che passo di qui
			if (string.IsNullOrWhiteSpace(pdfImageName))
			{
				// creo il file PDF (formato PDF/A 1-b compatibile)
				gdStatus = GdPicturePDF.NewPDF(true);

				// aggiungo l'immagine appena acquisita al file
				//@@TODOPORTING8: controllare il terzo parametro: If true, begin a new page and draw the added image on its whole surface.
				pdfImageName = GdPicturePDF.AddImageFromGdPictureImage(imageID, false, true);
				GdPicture.ReleaseGdPictureImage(imageID); // rilascio la memoria facendo la release dell'immagine

                if (RBCreateSeparateFile.Checked && RBCreateSeparateFile.Enabled)
				{
					// se l'utente ha scelto di creare un singolo file per immagine
					// salvo il PDF e reimposto il pdfImageName a empty (cosi' non lo risalvo piu' alla fine del loop)
					GdPicturePDF.SaveToFile(acquiredFilePath);
					// aggiungo il file alla lista (ma solo se lo status e' a true??)
					AcquiredFileList.Add(acquiredFilePath);

					pdfImageName = string.Empty;
				}

				return gdStatus;
			}
			else
			{
				// dalla seconda volta in poi passo a qui e vado in append con le pagine
				// si tratta di un documento multipagina
				//@@TODOPORTING8: controllare il terzo parametro: If true, begin a new page and draw the added image on its whole surface.
				pdfImageName = GdPicturePDF.AddImageFromGdPictureImage(imageID, false, true);
				GdPicture.ReleaseGdPictureImage(imageID); // rilascio la memoria facendo la release dell'immagine
			}

			return gdStatus;
		}

		///<summary>
		/// Sulla chiusura della form chiudo il Source ed il relativo SourceManager
		///</summary>
		//--------------------------------------------------------------------------------
		private void Acquisition_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				GdPicture.TwainCloseSource();
				GdPicture.TwainCloseSourceManager(this.Handle);
				GdPicture.TwainUnloadSourceManager(this.Handle);
			}
			catch
			{ }
		}

        //--------------------------------------------------------------------------------
        private void CBCreateSeparateFile_CheckedChanged(object sender, EventArgs e)
        {
            if (CBCreateSeparateFile.Enabled)
                GbMultiFileOptions.Enabled = CBCreateSeparateFile.Checked;
        }

        //--------------------------------------------------------------------------------
        private void GbMultiFileOptions_EnabledChanged(object sender, EventArgs e)
        {
            if (!GbMultiFileOptions.Enabled)
            {
                RBSeparatorSheet.Checked = RBBarcode.Checked =  false;
                RBCreateSeparateFile.Checked = CBCreateSeparateFile.Checked;
            }
            else RBCreateSeparateFile.Checked = true;
        }

        //--------------------------------------------------------------------------------
        private void CBCreateSeparateFile_EnabledChanged(object sender, EventArgs e)
        {
            GbMultiFileOptions.Enabled = CBCreateSeparateFile.Enabled;
			if (!CBCreateSeparateFile.Enabled)
			{
				RBCreateSeparateFile.Checked = true;
				CBCreateSeparateFile.Checked = true;
			}
			else
			{
				if (SplitOnBarCode)
				{
					RBSeparatorSheet.Checked = false;
					RBCreateSeparateFile.Checked = false;
					RBBarcode.Checked = true;
					CBCreateSeparateFile.Checked = true;
				}
				else
				{
					RBSeparatorSheet.Checked = false;
					RBCreateSeparateFile.Checked = false;
					CBCreateSeparateFile.Checked = false;
				}
			}
        }
	}
}
