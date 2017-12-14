using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.EasyAttachment.UI.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TBPicComponents;
using System.Threading;

namespace Microarea.EasyAttachment.BusinessLogic
{
	///<summary>
	/// Classe con le informazioni base di un barcode
	///</summary>
	//================================================================================
	public class Barcode
	{
		public BarcodeStatus Status = BarcodeStatus.OK;
        public string Value { get { return barcode.Value; } set { barcode.Value = value; } }
        public BarCodeType Type { get { return barcode.Type; } set { barcode.Type = value; } } 
		public int ImageId = -1;
        public TypedBarcode barcode = new TypedBarcode("", BarCodeType.NONE);
	}

    //================================================================================
    public class TypedBarcode
    {
        public string Value = string.Empty;
        public BarCodeType Type = BarCodeType.BC_CODE39;

        public string TypeDescription { get { return BarcodeMapping.GetBarCodeDescription(Type); } }

        //--------------------------------------------------------------------------------
        public TypedBarcode()
        {
            Value = string.Empty;
            Type = BarCodeType.BC_CODE39;
        }

        //--------------------------------------------------------------------------------
        public TypedBarcode(string avalue, BarCodeType type)
        {
            Value = avalue;
            Type = type;
        }
    }

    ///<summary>
    /// Classe per la gestione dei barcode:
    /// - scrittura
    /// - lettura
    /// - generazione immagine correlata
    ///</summary>
    //================================================================================
    public class BarcodeManager : BaseManager
    {
        private TBPicImaging gdPicture = new TBPicImaging();
        private TBPicPDF gdPDF = new TBPicPDF();

        private DMSModelDataContext dc = null;

        private bool checkIfBarcodeExists = true;

        ///<summary>
        /// Proprieta' per disattivare il controllo di esistenza di un valore di barcode
        /// Di default nasce a true, ma puo' essere impostato diversamente dall'esterno
        ///</summary>
        //--------------------------------------------------------------------------------
        public bool CheckIfBarcodeExists { get { return checkIfBarcodeExists; } set { checkIfBarcodeExists = value; } }

        //--------------------------------------------------------------------------------
        public BarcodeManager(DMSOrchestrator dmsOrchestrator)
        {
            ManagerName = "BarcodeManager";
            DMSOrchestrator = dmsOrchestrator;
            dc = DMSOrchestrator.DataContext;
        }
        ///<summary> 
        /// Funzione che ritorna se un valore di barcode e' valido per EA
        /// (dai settings)
        ///</summary>
        //--------------------------------------------------------------------------------
        internal bool IsValidEABarcodeValue(string value)
        {

            return value != null && value.StartsWith(DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodePrefix, StringComparison.InvariantCulture) && value.Length < 18;
        }
        internal bool IsValidBarcodeType(BarCodeType barCodeType)
        {
            return (barCodeType == DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeType ||
                (barCodeType == BarCodeType.BC_CODE128 && DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeType == BarCodeType.BC_EAN128)
                );
        }
        ///<summary>
        /// Genera un codice random di x cifre da assegnare al barcode come valore, piu' un prefisso
        ///</summary>
        //--------------------------------------------------------------------------------
        internal TypedBarcode CreateRandomBarcodeValue()
        {
            Random randomizerCode = new Random();
            int maxval = (int)Math.Pow(10, (GetBarcodeMaxLen())) - 1;

            string code = randomizerCode.Next(1, maxval).ToString();

            if (code.Length > GetBarcodeMaxLen())
                return CreateRandomBarcodeValue();

            TypedBarcode tbc = new TypedBarcode
                (
                string.Concat(DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodePrefix, code),
                DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeType
                );

            if (BarcodeAlreadyExists(tbc.Value, -1))
                return CreateRandomBarcodeValue();

            return tbc;
        }

        //--------------------------------------------------------------------------------
        private int GetBarcodeMaxLen()
        {
            return 9;
            //int prefixlen = DMSOrchestrator.SettingsManager.UserSettingState.Options.BarcodeDetectionOptionsState.BarcodePrefix.Length;
            //switch (DMSOrchestrator.SettingsManager.UserSettingState.Options.BarcodeDetectionOptionsState.BarcodeType)
            //{
            //    case (BarCodeType.BC_EAN13):
            //        return 12 - prefixlen;
            //    case (BarCodeType.BC_EAN8):
            //        return 7- prefixlen;
            //    case (BarCodeType.BC_UPCA):
            //        return 12 - prefixlen;
            //    case (BarCodeType.BC_UPCE):
            //        return 6 - prefixlen;
            //    //case (BarCodeType.BC_INT25):
            //    //    return 7 - prefixlen;
            //} 
        }

        ///<summary>
        /// Genera un barcode dato uno specifico valore
        ///</summary>
        //--------------------------------------------------------------------------------
        internal Barcode GetBarcodeImageFromValue(TypedBarcode barcode)
        {
            
            // se il value e' empty non procedo
            if (string.IsNullOrWhiteSpace(barcode.Value))
                return null;

            Barcode myBarcode = null;

            TBPictureStatus gStatus = TBPictureStatus.InvalidBarCode;

            try
            {
                // Crea una GdPicture Image vuota
                int imageId = gdPicture.CreateNewGdPictureImage(250/*width*/, 100/*height*/, PixelFormat.Format24bppRgb, Color.White);
                //tring bct = GetBarcodeTypeFromBarcodeValue(barcode);
                //BarCodeType bctype = BarcodeMapping.GetBarCodeType(bct);
                TBPicBarcode1DWriterType TBPICbctype = TBPicBarcode1DWriterType.Barcode1DWriterCode39;
                try
                {
                    TBPICbctype = BarcodeMapping.GetTBPicBarcode1DWriterType(BarcodeMapping.GetBarCodeDescription(barcode.Type));
                }
                catch //(Exception exc)
                {
                    //SetMessage(barcode, exc, "GetBarcodeImageFromValue"); 
                    gStatus = gdPicture.DrawText
                                (
                                imageId,
                                barcode.Value,
                                40, /*DstLeft*/
                                80, /*DstTop*/
                                8, /*FontSize*/
                                TBPicFontStyle.FontStyleRegular,
                                Color.Black,
                                "Arial", /*FontName*/
                                true /*apply the Antialiasing algorithm*/
                                );
                    Debug.WriteLine("****---> " + gStatus);
                    myBarcode = new Barcode();
                    myBarcode.barcode = barcode;
                    myBarcode.Status = BarcodeStatus.TypeNotValid;
                    return myBarcode;
                }

                if (imageId > 0)
                {
                    // disegno il barcode nell'immagine appena creata (il valore alfanumerico NON compare, uso dopo la DrawText!)
                    gStatus = gdPicture.Barcode1DWrite
                        (
                            imageId,
                            TBPICbctype, 1, -1,
                            barcode.Value,
                            0, /*DstLeft*/
                            0, /*DstTop*/
                            250, /*DstWidth*/
                            80, /*DstHeight*/
                            Color.Black,
                            TBPicBarcodeAlign.BarcodeAlignCenter,
                            TBPicBarcodeVerticalAlign.BarcodeAlignCenter,
                            false
                        );

                    if (gStatus == TBPictureStatus.OK)
                    {
                        // per avere il reale valore del barcode devo effettuare uno scan immediato
                        // questo perche' dato un valore specificato, alcuni tipi di barcode aggiungono dei caratteri di controllo
                        // quindi prima di aggiungere il testo sotto al barcode e' necessario effettuare uno scan immediato, ottenere il valore
                        // e disegnarlo sotto l'immagine con la funzione DrawText
                        myBarcode = CreateBarcodeForImage(imageId);
                        if (myBarcode == null)
                            return null;
                        myBarcode.Type = barcode.Type;

                        // disegno il testo del barcode
                        if (myBarcode != null && !string.IsNullOrWhiteSpace(myBarcode.Value))
                            gStatus = gdPicture.DrawText
                                (
                                imageId,
                                myBarcode.Value,
                                40, /*DstLeft*/
                                80, /*DstTop*/
                                8, /*FontSize*/
                                TBPicFontStyle.FontStyleRegular,
                                Color.Black,
                                "Arial", /*FontName*/
                                true /*apply the Antialiasing algorithm*/
                                );
                        Debug.WriteLine("****---> " + gStatus);
                    }
                    else
                    {
                        // tengo traccia solo dello status
                        myBarcode = new Barcode();
                        myBarcode.Status = BarcodeStatus.DrawingError;
                    }
                }
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.BarcodeImageCreationError, barcode), e, "GetBarcodeImageFromValue");
            }

            return myBarcode;
        }

        ///<summary>
        /// Ritorna una lista di barcode individuati nell'immagine passata come parametro
        ///</summary>
        //--------------------------------------------------------------------------------
        internal List<Barcode> GetBarcodesFromImage(int imageId)
        {
            List<Barcode> bCodes = new List<Barcode>();

            if (imageId <= 0)
                return bCodes;

            try
            {
                // effettua una scansione dell'immagine per identificare gli eventuali codici a barre
                if (gdPicture.Barcode1DReaderDoScan(imageId) == TBPictureStatus.OK)
                {
                    for (int i = 1; i <= gdPicture.Barcode1DReaderGetBarcodeCount(); i++)
                    {
                        Barcode bc = CreateBarcode(imageId, i);
                        bCodes.Add(bc);
                    }
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.BarcodeScanError, e, "GetBarcodesFromImage");
            }
            finally
            {
                gdPicture.Barcode1DReaderClear();
            }

            return bCodes;
        }

        ///<summary>
        /// Ritorna il primo codice a barre individuato nell'imageId specificato
        /// Questo metodo e' utilizzato in fase di disegno di un codice a barre partendo da un valore
        ///</summary>
        //--------------------------------------------------------------------------------
        internal Barcode CreateBarcodeForImage(int imageId)
        {
            Barcode bCode = new Barcode();

            if (imageId <= 0)
                return bCode;

            try
            {
                // effettua una scansione dell'immagine per identificare gli eventuali codici a barre
                if (gdPicture.Barcode1DReaderDoScan(imageId) == TBPictureStatus.OK && gdPicture.Barcode1DReaderGetBarcodeCount() >= 1)
                    bCode = CreateBarcode(imageId, 1);
            }
            catch (Exception e)
            {
                SetMessage(Strings.BarcodeScanError, e, "CreateBarcodeForImage");
            }
            finally
            {
                gdPicture.Barcode1DReaderClear();
            }

            return bCode;
        }

        ///<summary>
        /// Un barcode e' valido se:
        /// 1) inizia con il prefisso EA e non eccede i 17 chr
        /// 2) e' del tipo Code39
        /// 3) non e' gia' stato associato ad documento archiviato gia' presente nel database 
        /// 
        /// Metodo che ispeziona le pagine presenti in un documento (a seconda del , 
        /// individua tutti i barcode all'interno delle stesse e:
        /// 1. se non esiste un barcode valido (vedi sopra): visualizza un messaggio all'utente e ritorna null
        /// 2. esiste un solo barcode valido: ritorna il valore del barcode
        /// 3. se esistono due o piu' barcode validi: mostra una form con elenco dei barcode, dove l'utente deve 
        /// selezionare il valore che desidera
        ///</summary>
        /////--------------------------------------------------------------------------------
        internal TypedBarcode DetectBarcodeValueInFile(AttachmentInfo ai)
        {
            Barcode aBarcode = new Barcode();

            bool isPdf = false;
            bool isTIFF = false;
            int imageId = 0;

            List<Barcode> bcodes = null;

            // carica cmq la prima pagina (da file o dal database a seconda dell'estensione)
            imageId = LoadFromAttachmentInfo(ai);

            isPdf = FileExtensions.IsPdfPath(ai.TempPath);
            isTIFF = FileExtensions.IsTifPath(ai.TempPath);

            // se l'imageId e' buono, allora devo fare diversamente a seconda del tipo di estensione
            if (imageId > 0)
            {
                if (isPdf)
                    bcodes = DetectBarcodeInPdfFile(ai, imageId);
                else
                    if (isTIFF)
                    bcodes = DetectBarcodeInTIFFFile(ai, imageId);
                else
                {
                    try
                    {
                        // eseguo il detect dei barcode
                        bcodes = GetBarcodesFromImage(imageId);
                    }
                    catch (Exception e)
                    {
                        SetMessage(string.Format(Strings.BarcodeDetectInFileError, ai.TempPath), e, "DetectBarcodeValueInFile");
                    }
                }
            }

            // ATTENZIONE: qui si deve anche controllare che per i barcode individuati nel documento 
            // non ci sia gia' una pending papery!!!!

            if (bcodes == null || bcodes.Count == 0)
            {
                // nessun barcode
            }
            else
            {
                /*int countValid = 0; // contatore barcode validi

				foreach (Barcode bc in bcodes)
				{
					if (bc.Status == BarcodeStatus.OK)
					{
						barcode = bc;
						countValid++;
					}
					else if (bc.Status == BarcodeStatus.AlreadyExists)
					{
						// barcode esistente
					}
					else if (bc.Status == BarcodeStatus.DrawingError)
					{
						// barcode DrawingError
					}
					else if (bc.Status == BarcodeStatus.SyntaxNotValid)
					{
						// barcode SyntaxNotValid
					}
					else if (bc.Status == BarcodeStatus.TypeNotValid)
					{
						// barcode TypeNotValid
					}
				}*/

                // se la CurrentTabPage e' null significa che mi trovo nel RepositoryExplorer, 
                // pertanto non procedo a visualizzare alcuna messagebox, visto che potrebbe essere
                // un'importazione massiva di documenti
                /*if (DMSOrchestrator.CurrentTabPage == null)
				{
					// se ho trovato solo un barcode controllo che non sia gia' stato assegnato nel database
					if (countValid == 1 && BarcodeAlreadyExists(bcodes[0].Value, ai.ArchivedDocId))
						bcodes[0].Value = string.Empty;

					// se ho trovato piu' barcode, considero quelli validi e controllo che non siano gia' stato assegnato nel database
					// appena ne trovo uno valido lo assegno e procedo
					if (countValid > 1)
					{
						for (int i = 0; i < bcodes.Count; i++)
						{
							Barcode currBarcode = bcodes[0];
							if (currBarcode.Status == BarcodeStatus.OK && !BarcodeAlreadyExists(currBarcode.Value, ai.ArchivedDocId))
							{
								barcode = currBarcode;
								break;
							}
						}
					}
				}
				else
				{
					// se non trovato neppure un barcode valido, mostro un messaggio, assegno null al barcode 
					// N.B. non faccio return null perche' devo disposare prima l'imageId
					if (countValid == 0)
					{
						using (SafeThreadCallContext context = new SafeThreadCallContext())
						{
							InvalidBarcode ibForm = new InvalidBarcode();
							ibForm.Owner = DMSOrchestrator.CurrentTabPage.FindForm();
							ibForm.SetMessageText = Strings.BarcodeDetectedAreNoValid;
							ibForm.ShowDialog(ibForm.Owner);
						}
					}

					// se ho trovato piu' di un barcode valido faccio scegliere all'utente
					if (countValid > 1)
					{
						using (SafeThreadCallContext context = new SafeThreadCallContext())
						{
							// show detection form
							BarcodeDetection bcDetection = new BarcodeDetection(bcodes);
							bcDetection.Owner = DMSOrchestrator.CurrentTabPage.FindForm();
							DialogResult dr = bcDetection.ShowDialog(bcDetection.Owner);
							barcode = bcDetection.SelectedBarcode;
						}
					}
				}*/

                // ASSEGNO SEMPRE IL PRIMO BARCODE! (deciso con Anna&Ilaria il 29/11/11!) ;-)
                foreach (Barcode bc in bcodes)
                {
                    if (bc.Status == BarcodeStatus.OK)// && !BarcodeAlreadyExists(bc.Value, ai.ArchivedDocId))
                    {
                        aBarcode = bc;
                        break;
                    }
                }
            }

            //faccio la release solo per i file diversi da pdf e tiff (x loro viene effettuata dentro gli appositi metodi)
            if (!isPdf && !isTIFF && imageId > 0)
                gdPicture.ReleaseGdPictureImage(imageId);

            // se ho trovato un solo barcode valido ho gia' assegnato il valore prima e lo ritorno
            return aBarcode.barcode;
        }

        //--------------------------------------------------------------------------------
        internal Dictionary<string, Barcode> SplitPdfOnBarcode(Dictionary<Barcode, int> splitData, string path)
        {
            //non ho trovato barcode, non è possibile effettuare lo split... considero il file intero 
            if (splitData == null || splitData.Count == 0)
                return null;

            Dictionary<string, Barcode> files = new Dictionary<string, Barcode>();
            TBPicPDF oGdPicturePDFDest = new TBPicPDF();

            int nrPages = gdPDF.GetPageCount();
            if (nrPages <= 1)
            {
                //non c'`e nulla da splittare aggiungo il file originario
                files.Add(path, new Barcode());
                return files;
            }

            bool docopened = false;
            int docCounter = 1;

            Barcode prevBarcode = null;
            Barcode currBarcode = null;
            string currPath = string.Empty;
            for (int page = 1; page <= nrPages; page++)
            {
                foreach (KeyValuePair<Barcode, int> kvp in splitData)
                {
                    if (page == kvp.Value)
                    {
                        prevBarcode = currBarcode;
                        currBarcode = kvp.Key;

                        currPath = CloseDoc(path, docopened, docCounter, oGdPicturePDFDest);
                        if (!currPath.IsNullOrEmpty())
                            files.Add(currPath, prevBarcode);
                        oGdPicturePDFDest.NewPDF(true);// creo il file PDF (formato PDF/A 1-b compatibile)
                        docopened = true;
                    }
                }
                oGdPicturePDFDest.ClonePage(gdPDF, page);
            }
            //chiudo l'ultimo doc che è rimasto aperto dopo l'uscita dal ciclo
            currPath = CloseDoc(path, docopened, docCounter, oGdPicturePDFDest);
            if (!currPath.IsNullOrEmpty())
                files.Add(currPath, currBarcode);

            gdPDF.CloseDocument();//chiudo il file originale
            return files;
        }

        //--------------------------------------------------------------------------------
        internal Dictionary<string, Barcode> SplitTiffOnBarcode(string path)
        {
            int imageId = gdPicture.TiffCreateMultiPageFromFile(path);
            int nrPages = gdPicture.TiffGetPageCount(imageId);

            if (nrPages <= 1)
                return null;

            Dictionary<string, Barcode> files = new Dictionary<string, Barcode>();
            int docCounter = 1;
            int intDestDocID = 0;

            for (int page = 1; page <= nrPages; page++)
            {
                if (gdPicture.TiffSelectPage(imageId, page) == 0)
                {
                    if (gdPicture.Barcode1DReaderDoScan(imageId) == TBPictureStatus.OK)
                    {
                        if (gdPicture.Barcode1DReaderGetBarcodeCount() > 0)
                        {
                            Dictionary<Barcode, int> barcodes = new Dictionary<Barcode, int>();
                            GetBarcodesInPage(ref barcodes, imageId, page);
                            if (barcodes.Count > 0)
                            {
                                if (intDestDocID != 0)
                                    gdPicture.ReleaseGdPictureImage(intDestDocID);

                                intDestDocID = gdPicture.CreateClonedGdPictureImageI(imageId);
                                string destpath = GetDestPath(path, docCounter);
                                gdPicture.TiffSaveAsMultiPageFile(intDestDocID, destpath, TBPicTiffCompression.TiffCompressionCCITT4);
                                files.Add(destpath, barcodes.First().Key);
                            }

                        }
                        else
                            gdPicture.TiffAddToMultiPageFile(intDestDocID, imageId);
                    }
                }

                if (intDestDocID != 0)
                    gdPicture.ReleaseGdPictureImage(intDestDocID);

            }
            gdPicture.ReleaseGdPictureImage(imageId);
            return files;
        }

        /// <summary>
        /// recupera  un path temporaneo in cui salvare i vari file splittai da un unico file originale, verifica l'esistenza di un file uguale e cambia il path incrementando un contatore
        /// </summary>
        /// <param name="path"></param>
        /// <param name="doccounter"></param>
        /// <returns></returns>
        //--------------------------------------------------------------------------------
        private string GetDestPath(string path, int doccounter)
        {
            string p = Path.Combine(DMSOrchestrator.EasyAttachmentTempPath, Path.GetFileNameWithoutExtension(path) + (++doccounter).ToString() + Path.GetExtension(path));
            if (File.Exists(p))
                p = GetDestPath(path, doccounter);

            return p;
        }

        //--------------------------------------------------------------------------------
        private string CloseDoc(string path, bool docopened, int doccounter, TBPicPDF oGdPicturePDFDest)
        {
            if (docopened)
            {
                string destPath = GetDestPath(path, doccounter);
                oGdPicturePDFDest.SaveToFile(destPath);
                oGdPicturePDFDest.CloseDocument();

                docopened = false;
                return destPath;
            }
            return string.Empty;
        }

        //--------------------------------------------------------------------------------
        internal Dictionary<Barcode, int> GetSplitDataOnBarcodeInPDFFile(string path)
        {
            Dictionary<Barcode, int> bCodes = new Dictionary<Barcode, int>();

            // se path vuoto, file non esiste o file non termina con pdf non procedo
            if (!FileExtensions.IsPdfPath(path) || !File.Exists(path))
                return bCodes;

            try
            {
                gdPDF.LoadFromFile(path, false);
                int imageId = gdPDF.RenderPageToGdPictureImage(DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.DpiQualityImage, false);
                // Debug.WriteLine(DateTime.Now);
                bCodes = GetPDFBarcodes(imageId, gdPDF.GetPageCount());
                //Debug.WriteLine(DateTime.Now);
                // gdPDF.CloseDocument();//non chiudo che serve nella seconda fase
            }

            catch (Exception e)
            {
                SetMessage(string.Format(Strings.BarcodeDetectInFileError, path), e, "GetSplitDataOnBarcodeInPDFFile");
            }

            return bCodes;
        }

        ////--------------------------------------------------------------------------------
        //internal Dictionary<Barcode, int> GetSplitDataOnBarcodeInTIFFFile(string path)
        //{
        //    Dictionary<Barcode, int> bCodes = new Dictionary<Barcode, int>();

        //    // se path vuoto, file non esiste o file non termina con pdf non procedo
        //    if ( !FileExtensions.IsTifPath(path) || !File.Exists(path))
        //        return bCodes;

        //    try
        //    {                
        //        int imageId = LoadFromFile(path);
        //        bCodes = GetTIFFBarcodes(imageId, gdPicture.TiffGetPageCount(imageId));
        //        gdPicture.ClearGdPicture();
        //    }

        //    catch (Exception e)
        //    {
        //        SetMessage(string.Format(Strings.BarcodeDetectInFileError, path), e, "GetSplitDataOnBarcodeInPDFFile");
        //    }

        //    return bCodes;
        //}

        //--------------------------------------------------------------------------------
        private void GetBarcodesInPage(ref Dictionary<Barcode, int> bCodesAndPages, int imageId, int page)
        {
            // eseguo la ricerca dei barcode nella pagina i-esima
            if (gdPicture.Barcode1DReaderDoScan(imageId) == TBPictureStatus.OK)
            {
                // per ogni barcode individuato memorizzo le sue informazioni
                for (int j = 1; j <= gdPicture.Barcode1DReaderGetBarcodeCount(); j++)
                {
                    Barcode bc = CreateBarcode(imageId, j);

                    // se il barcode e' valido e non esiste gia' per un altro doc memorizzo e fermo il loop
                    if (bc.Status == BarcodeStatus.OK)// && !BarcodeAlreadyExists(bc.Value, ai.ArchivedDocId))
                    {
                        bCodesAndPages.Add(bc, page);
                        break;
                    }
                    else
                        continue; //altrimenti continuo
                }
            }
        }

        //--------------------------------------------------------------------------------
        private Dictionary<Barcode, int> GetPDFBarcodes(int imageId, int nrPages = 1)
        {
            Dictionary<Barcode, int> bCodesAndPages = new Dictionary<Barcode, int>();
            for (int i = 1; i <= nrPages; i++)
            {
                // seleziono la pagina corrente
                if (gdPDF.SelectPage(i) == TBPictureStatus.OK)
                {
                    // estraggo l'immagine ma solo per le pagine successive alla prima
                    if (i > 1)
                        imageId = gdPDF.RenderPageToGdPictureImage(DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.DpiQualityImage, false);

                    if (imageId > 0)
                    {
                        GetBarcodesInPage(ref bCodesAndPages, imageId, i);
                        gdPicture.Barcode1DReaderClear();
                        gdPicture.ReleaseGdPictureImage(imageId);
                    }
                }
            }

            return bCodesAndPages;
        }

        //--------------------------------------------------------------------------------
        private Dictionary<Barcode, int> GetTIFFBarcodes(int imageId, int nrPages)
        {
            Dictionary<Barcode, int> bCodesAndPages = new Dictionary<Barcode, int>();
            for (int i = 1; i <= nrPages; i++)
            {
                // seleziono la pagina corrente
                if (gdPicture.TiffSelectPage(imageId, i) == TBPictureStatus.OK)
                {
                    GetBarcodesInPage(ref bCodesAndPages, imageId, i);
                    gdPicture.Barcode1DReaderClear();
                }
            }
            gdPicture.ClearGdPicture();
            return bCodesAndPages;
        }

        ///<summary>
        /// Metodo che ispeziona tutte le pagine presenti in un documento PDF e ritorna
        /// la lista dei Barcode in esse presenti
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<Barcode> DetectBarcodeInPdfFile(AttachmentInfo ai, int imageId)
        {
            List<Barcode> bCodes = new List<Barcode>();

            // se path vuoto, file non esiste o file non termina con pdf non procedo
            if (!FileExtensions.IsPdfPath(ai.TempPath) || !File.Exists(ai.TempPath))
                return bCodes;

            try
            {
                // determino il numero delle pagine da sfogliare a seconda dei settings
                int nrPages = (DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeDetectionAction
                                == BarcodeDetectionAction.DetectOnlyInFirstPage)
                                ? 1
                                : gdPDF.GetPageCount();

                Dictionary<Barcode, int> bCodesPages = new Dictionary<Barcode, int>();
                bCodesPages = GetPDFBarcodes(imageId, nrPages);
                bCodes = bCodesPages.Keys.ToList();
                gdPDF.CloseDocument();
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.BarcodeDetectInFileError, ai.TempPath), e, "DetectBarcodeInPdfFile");
            }

            return bCodes;
        }

        ///<summary>
        /// Metodo che ispeziona tutte le pagine presenti in un documento TIFF e ritorna
        /// la lista dei Barcode in esse presenti
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<Barcode> DetectBarcodeInTIFFFile(AttachmentInfo ai, int imageId)
        {
            List<Barcode> bCodes = new List<Barcode>();
            Dictionary<Barcode, int> bCodesAndPages = new Dictionary<Barcode, int>();
            // se path vuoto o file non termina con tiff/tif non procedo
            if (!FileExtensions.IsTifPath(ai.TempPath))
                return bCodes;

            try
            {
                // eseguo lo scan
                if (imageId > 0)
                {
                    // se il file ha una sola pagina, procedo direttamente allo scan per trovare eventuali barcode
                    if (!gdPicture.TiffIsMultiPage(imageId))
                        bCodes = GetBarcodesFromImage(imageId);
                    else
                    {
                        // se il file e' multi-pagina, devo scorrere tutte le pagine
                        int nrPages = (DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeDetectionAction
                                        == BarcodeDetectionAction.DetectOnlyInFirstPage)
                                        ? 1
                                        : gdPicture.TiffGetPageCount(imageId);

                        bCodesAndPages = GetTIFFBarcodes(imageId, nrPages);
                        bCodes = bCodesAndPages.Keys.ToList();
                    }

                    gdPicture.ReleaseGdPictureImage(imageId);
                }
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.BarcodeDetectInFileError, ai.TempPath), e, "DetectBarcodeInTIFFFile");
            }

            return bCodes;
        }

        ///<summary>
        /// Per ogni barcode individuato in un'immagine, identifico il suo valore ed altre proprieta'
        ///</summary>
        //--------------------------------------------------------------------------------
        internal Barcode CreateBarcode(int imageId, int barcodeIdx)
        {
          
            Barcode myBarcode = new Barcode();
            myBarcode.ImageId = imageId;
            // leggo il valore del barcode
            myBarcode.Value = gdPicture.Barcode1DReaderGetBarcodeValue(barcodeIdx);
            myBarcode.Type = BarcodeMapping.GetBarCodeType(gdPicture.Barcode1DReaderGetBarcodeType(barcodeIdx));

            // controllo il tipo di barcode
            if (myBarcode.Type != DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeType/*TBPicBarcode1DReaderType.Barcode1DReaderCode39*/)
            {
                myBarcode.Status = BarcodeStatus.TypeNotValid;
                return myBarcode;
            }

            // controllo se il value e' valido per EA
            if (!DMSOrchestrator.IsValidEABarcodeValue(myBarcode.Value))
            {
                myBarcode.Status = BarcodeStatus.SyntaxNotValid;
                return myBarcode;
            }

            // NON DEVO FARE ALCUN CONTROLLO QUI!
            // controllo se questo valore e' gia' presente nel db 
            /*if (BarcodeAlreadyExists(myBarcode.Value, archiveDocId))
			{
				myBarcode.Status = BarcodeStatus.AlreadyExists;
				return myBarcode;
			}*/

            return myBarcode;
        }

        //--------------------------------------------------------------------------------
        public static string GetStatusText(MassiveStatus s)
        {
            switch (s)
            {
                case MassiveStatus.NoBC:
                    return Strings.NoBarcode;
                case MassiveStatus.OnlyBC:
                    return Strings.OnlyBarcode;
                case MassiveStatus.Papery:
                    return Strings.Ok;
                case MassiveStatus.BCDuplicated:
                    return Strings.DuplicatedBC;
                case MassiveStatus.ItemDuplicated:
                    return Strings.ItemDuplicated;
            }
            return string.Empty;
        }

        //--------------------------------------------------------------------------------
        public static string GetActionToDoText(MassiveAction action)
        {
            switch (action)
            {
                case (MassiveAction.Archive) : return Strings.Archive;
                case (MassiveAction.Attach)  : return Strings.Attach;
                case (MassiveAction.Substitute): return Strings.Substitute;
            }
            return Strings.None;
        }


        //--------------------------------------------------------------------------------
        public static string GetResultText(MassiveResult action)
        {
            switch (action)
            {
                case (MassiveResult.Done): return Strings.Done;
                case (MassiveResult.WithError): return Strings.WithError;
                case (MassiveResult.Failed): return Strings.Failed;
                case (MassiveResult.PreFailed): return Strings.Failed;
                case (MassiveResult.Ignored): return Strings.None;
            }
            return Strings.None;
        }

		//--------------------------------------------------------------------------------
		public static string GetInfoText(MassiveStatus status, MassiveAction action)
		{
			string information = string.Empty;

			switch (status)
			{
				case MassiveStatus.Papery:
					information = Strings.Papery;
					break;
				case MassiveStatus.OnlyBC:
					information = Strings.NoPapery;
					break;
				case MassiveStatus.NoBC:
					information = Strings.NoBarcode;
					break;
				case MassiveStatus.ItemDuplicated:
					information = Strings.ItemDuplicatedVerbose;
					break;
				case MassiveStatus.BCDuplicated:
					information = Strings.BarcodeAlreadyInUse;
					break;
				default:
					information = Strings.BarcodeAlreadyInUse;
					break;
			}

			information += Environment.NewLine;

			switch (action)
			{
				case MassiveAction.None:
					information += Strings.NoActionToExecute;
					break;
				case MassiveAction.Archive:
					information += Strings.ItemToArchive;
					break;
				case MassiveAction.Substitute:
					information += Strings.SustituteDocDescription;
					break;
			}

			return information;
		}


		///<summary>
		/// Metodo che controlla che uno specifico valore del barcode non sia gia' stato
		/// utilizzato da un altro documento archiviato
		///</summary>
		//--------------------------------------------------------------------------------
		internal bool BarcodeAlreadyExists(string barcodeValue, int archiveDocId)
        {
            bool exists = false;

            if (!checkIfBarcodeExists)
                return exists;

            // se il valore e' vuoto non effettuo alcun controllo
            if (string.IsNullOrWhiteSpace(barcodeValue))
                return exists;

            try
            {
                var count = (from doc in dc.DMS_ArchivedDocuments
                             where doc.Barcode == barcodeValue && doc.ArchivedDocID != archiveDocId
                             select doc).Count();

                exists = (count != 0);
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "BarcodeAlreadyExists");
            }

            return exists;
        }

        //questo metodo viene chiamato da woorm:
        //in fase di archiviazione per farsi dare un nuovo barcode
        //in fase di run del report per farsi dare l'eventuale barcode se report risulta già archiviato (vedi impr. #5167: stampa report con barcode)
        //--------------------------------------------------------------------------------
        internal TypedBarcode GetBarcodeForReport(string fileName, bool isArchiving)
        {
            TypedBarcode barcode = new TypedBarcode("", DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeType);

            //se non sto archiviando il report ma lo sto visualizzando/stampando verifico il come è impostato il settings PrintBarcodeInReport
            //se true allora il barcode va sempre visualizzato nel report
            //se false il barcode va considerato solo in fase di archiviazione ed è presente solo nel pdf archiviato
            if (!isArchiving && !DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.PrintBarcodeInReport)
                return barcode;

            //bugFix #19668
            // se sono in unattended ritorno subito il valore del parametro per le batch
            DuplicateDocumentAction duplicateAction = (DMSOrchestrator.InUnattendedMode)
                ? DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BCActionForBatch
                : DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BCActionForDocument;

            if (isArchiving && duplicateAction == DuplicateDocumentAction.ArchiveAndKeepBothDocs)
                return CreateRandomBarcodeValue();

            FileInfo file = new FileInfo(fileName);

            //first I search for an archived document with same name, extension and CreationTimeUtc
            var var = (from doc in dc.DMS_ArchivedDocuments
                       where doc.Name == file.Name && doc.ExtensionType == file.Extension && doc.Barcode != string.Empty
                       orderby doc.TBModified descending
                       select doc);

            try
            {
                if (var != null && var.Any())
                {
                    if (isArchiving && duplicateAction == DuplicateDocumentAction.ArchiveAndKeepBothDocs)
                        return CreateRandomBarcodeValue();

                    DMS_ArchivedDocument archiveDoc = var.First();
                    barcode = new TypedBarcode(archiveDoc.Barcode, BarcodeMapping.GetBarCodeType(archiveDoc.BarcodeType));
                }
                else
                {
                    //verifico che non sia presente come papery
                    var var1 = (from br in dc.DMS_ErpDocBarcodes
                                where br.Name == file.Name && br.Barcode != string.Empty
                                select br);

                    if (var1 != null && var1.Any())
                    {
                        DMS_ErpDocBarcode erpBarcode = var1.First();
                        barcode = new TypedBarcode(erpBarcode.Barcode, BarcodeMapping.GetBarCodeType(erpBarcode.BarcodeType));
                    }
                    else
                        barcode = (isArchiving) ? CreateRandomBarcodeValue() : new TypedBarcode();
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingBarcodeValue, e, "GetBarcodeForArchivedDoc");
                barcode = new TypedBarcode("", DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeType);
            }

            return barcode;
        }

        ///<summary>
        /// Metodo che ritorna l'oggetto DMS_ArchivedDocument relativa al valore del barcode passato parametro
        ///</summary>
        //--------------------------------------------------------------------------------
        internal DMS_ArchivedDocument GetArchivedDocFromBarcodeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // I search the archived document with barcode
            var var = from doc in dc.DMS_ArchivedDocuments
                      where doc.Barcode == value
                      select doc;
            try
            {
                if (var != null && var.Any())
                    return (DMS_ArchivedDocument)var.First();
            }
            catch (SqlException sqlExc)
            {
                SetMessage(string.Format(Strings.ErrorLoadingArchivedDocFromBarcode, value), sqlExc, "GetArchivedDocFromBarcodeValue");
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorLoadingArchivedDocFromBarcode, value), e, "GetArchivedDocFromBarcodeValue");
            }

            return null;
        }

        ///<summary>
        /// Dato un barcode e i dati che identificano un documento di ERP, ritorna l'eventuale allegato
        /// del documento archiviato identificato al valore del barcode
        ///</summary>
        //---------------------------------------------------------------------
        internal DMS_Attachment GetAttachmentFromBarcodeValue(string value, string docNamespace, string primaryKey)
        {
            try
            {
                var archDoc = from doc in dc.DMS_ArchivedDocuments
                              where doc.Barcode == value
                              select doc;

                if (archDoc == null || !archDoc.Any())
                    return null;

                DMS_ArchivedDocument archiveDoc = (DMS_ArchivedDocument)archDoc.Single();

                var attachment = (from att in dc.DMS_Attachments
                                  where att.ArchivedDocID == archiveDoc.ArchivedDocID &&
                                  att.DMS_ErpDocument.DocNamespace == docNamespace &&
                                  att.DMS_ErpDocument.PrimaryKeyValue == primaryKey
                                  select att);

                return (attachment != null && attachment.Any()) ? (DMS_Attachment)attachment.Single() : null;
            }
            catch (SqlException sqlExc)
            {
                SetMessage(string.Format(Strings.ErrorLoadingAttachmentFromBarcode, value), sqlExc, "GetAttachmentFromBarcodeValue");
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorLoadingAttachmentFromBarcode, value), e, "GetAttachmentFromBarcodeValue");
            }

            return null;
        }

        ///<summary>
        ///
        ///</summary>
        //--------------------------------------------------------------------------------
        internal string GetBarcodeTypeFromBarcodeValue(string value)
        {

            if (string.IsNullOrWhiteSpace(value))
                return null;

            // I search the archived document with barcode
            var var = from doc in dc.DMS_ArchivedDocuments
                      where doc.Barcode == value
                      select doc;
            try
            {
                if (var != null && var.Any())
                    return ((DMS_ArchivedDocument)var.First()).BarcodeType;

                // carico la riga interessata
                var barcodeRow = (from bcDoc in dc.DMS_ErpDocBarcodes
                                  where bcDoc.Barcode == value
                                  select bcDoc);

                if (barcodeRow != null && barcodeRow.Any())
                    return ((DMS_ErpDocBarcode)barcodeRow.First()).BarcodeType;

                //se il barcode non esiste sul db  nelle due tabelle uso il tipo dei settings:                
                return BarcodeMapping.GetBarCodeDescription(DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeType);
            }

            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorLoadingArchivedDocFromBarcode, value), e, "GetArchivedDocFromBarcodeValue");
            }
            return null;
        }

        ///<summary>
        /// Dato un barcode e un ErpDocumentId ritorna l'eventuale allegato
        /// del documento archiviato identificato al valore del barcode
        ///</summary>
        //---------------------------------------------------------------------
        internal DMS_Attachment GetAttachment(int archivedDocId, int erpDocumentID)
        {
            try
            {
                var attachment = (from att in dc.DMS_Attachments
                                  where att.ArchivedDocID == archivedDocId &&
                                  att.DMS_ErpDocument.ErpDocumentID == erpDocumentID
                                  select att);

                return (attachment != null && attachment.Any()) ? (DMS_Attachment)attachment.Single() : null;
            }
            catch (SqlException sqlExc)
            {
                SetMessage(string.Format(Strings.ErrorLoadingAttachmentFromBarcode, ""), sqlExc, "GetAttachment");
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorLoadingAttachmentFromBarcode, ""), e, "GetAttachment");
            }

            return null;
        }

        // dato il barcode recupero tutti i document ERP cui è stato associato.
        //--------------------------------------------------------------------------------
        internal List<int> GetErpDocumentIds(string barc)
        {
            List<int> docs = new List<int>();

            try
            {
                var ids = from bc in dc.DMS_ErpDocBarcodes where bc.Barcode == barc select bc;

                if (ids != null && ids.Any())
                {
                    // scorro tutte le righe della tabella DMS_ErpDocBarcodes e riempio il datatable
                    foreach (var id in ids)
                    {
                        DMS_ErpDocBarcode erpDocBC = (DMS_ErpDocBarcode)id;
                        docs.Add(erpDocBC.ErpDocumentID);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                SetMessage("SqlException", sqlEx, "GetErpDocumentId");
            }
            catch (Exception ex)
            {
                SetMessage("Exception", ex, "GetErpDocumentId");
            }

            return docs;
        }

        //---------------------------------------------------------------------
        internal List<DMS_ErpDocument> GetErpDocumentFromBarCode(string bc)
        {
            List<DMS_ErpDocument> docs = new List<DMS_ErpDocument>();

            try
            {
                List<int> ids = GetErpDocumentIds(bc);

                foreach (int id in ids)
                {
                    var erpDocuments = (from att in dc.DMS_ErpDocuments
                                        where att.ErpDocumentID == id
                                        select att);

                    if (erpDocuments != null && erpDocuments.Any())
                        // scorro tutte le righe della tabella DMS_ErpDocBarcodes e riempio il datatable
                        foreach (var erpDocument in erpDocuments)
                            docs.Add(erpDocument);
                }
            }
            catch (SqlException sqlExc)
            {
                SetMessage("SqlException", sqlExc, "GetErpDocumentFromBarCode");
            }
            catch (Exception e)
            {
                SetMessage("Exception", e, "GetErpDocumentFromBarCode");
            }

            return docs;
        }

        //--------------------------------------------------------------------------------
        internal bool UpdatePapery(string barcode, string notes, string fileName, DMSDocOrchestrator dmsDocOrchestrator)
        {
            bool success = false;

            if (string.IsNullOrWhiteSpace(barcode))
                return success;

            try
            {
                // carico la riga interessata
                var barcodeRow = (from bcDoc in dc.DMS_ErpDocBarcodes
                                  where bcDoc.ErpDocumentID == dmsDocOrchestrator.ErpDocumentID &&
                                  bcDoc.Barcode == barcode
                                  select bcDoc);

                // se non esiste la inserisco
                if (barcodeRow == null || !barcodeRow.Any())
                {
                    //dc.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dc.DMS_ErpDocBarcodes);
                    DMS_ErpDocBarcode erpDocBarcode = new DMS_ErpDocBarcode();
                    erpDocBarcode.Barcode = barcode;
                    erpDocBarcode.BarcodeType = dmsDocOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeTypeValue;
                    erpDocBarcode.Notes = (notes.Length > 128) ? notes.Substring(0, 128) : notes; // per evitare truncate in caso di spazi e a capo
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo file = new FileInfo(fileName);
                        erpDocBarcode.Name = file.Name;
                    }
                    else
                        erpDocBarcode.Name = string.Empty;

                    if (dmsDocOrchestrator.ErpDocumentID == -1)
                        erpDocBarcode.DMS_ErpDocument = dmsDocOrchestrator.ErpDocument;
                    else
                        erpDocBarcode.ErpDocumentID = dmsDocOrchestrator.ErpDocumentID;

                    dc.DMS_ErpDocBarcodes.InsertOnSubmit(erpDocBarcode);
                    dc.SubmitChanges();

                    if (dmsDocOrchestrator.ErpDocumentID == -1)
                        dmsDocOrchestrator.ErpDocumentID = erpDocBarcode.DMS_ErpDocument.ErpDocumentID;
                }
                else
                {
                    DMS_ErpDocBarcode erpDocBarcode = barcodeRow.Single();
                    if (erpDocBarcode != null)
                        erpDocBarcode.Notes = (notes.Length > 128) ? notes.Substring(0, 128) : notes; // per evitare truncate in caso di spazi e a capo
                    dc.SubmitChanges();
                }

                success = true;
            }
            catch (SqlException sqlEx)
            {
                SetMessage(Strings.ErrorUpdatingErpDocBarcodes, sqlEx, "UpdateErpDocBarcode");
                success = false;
            }
            catch (Exception ex)
            {
                SetMessage(Strings.ErrorUpdatingErpDocBarcodes, ex, "UpdateErpDocBarcode");
                success = false;
            }

            return success;
        }


        ///<summary>
        /// Elimina una riga dalla tabella DMS_ErpDocBarcodes, corrispondente al barcode e all'erpDocumentId
        ///</summary>
        //--------------------------------------------------------------------------------
        internal DMS_ErpDocBarcode GetERPDocumentBarcode(string barcode, int erpDocumentId)
        {
            DMS_ErpDocBarcode erpDocBC = null;
            try
            {
                // carico la riga interessata
                var barcodeRow = (from bcDoc in dc.DMS_ErpDocBarcodes
                                  where bcDoc.ErpDocumentID == erpDocumentId && bcDoc.Barcode == barcode
                                  select bcDoc);

                if (barcodeRow != null && barcodeRow.Any())
                    erpDocBC = (DMS_ErpDocBarcode)barcodeRow.Single();
            }

            catch (SqlException)
            {
                erpDocBC = null;
            }
            catch (Exception)
            {
                erpDocBC = null;
            }

            return erpDocBC;
        }

        ///<summary>
        /// Elimina una riga dalla tabella DMS_ErpDocBarcodes, corrispondente al barcode e all'erpDocumentId
        ///</summary>
        //--------------------------------------------------------------------------------
        internal bool DeleteBarcodeForErpDocument(string barcode, int erpDocumentId)
        {
            bool success = false;

            try
            {
                // carico la riga interessata
                var barcodeRow = (from bcDoc in dc.DMS_ErpDocBarcodes
                                  where bcDoc.ErpDocumentID == erpDocumentId && bcDoc.Barcode == barcode
                                  select bcDoc);

                if (barcodeRow != null && barcodeRow.Any())
                {
                    DMS_ErpDocBarcode erpDocBC = (DMS_ErpDocBarcode)barcodeRow.Single();
                    dc.DMS_ErpDocBarcodes.DeleteOnSubmit(erpDocBC);
                    dc.SubmitChanges();
                    success = true;
                }
            }
            catch (SqlException sqlEx)
            {
                SetMessage(Strings.ErrorDeletingErpDocBarcodes, sqlEx, "DeleteBarcodeForErpDocument");
                success = false;
            }
            catch (Exception ex)
            {
                SetMessage(Strings.ErrorDeletingErpDocBarcodes, ex, "DeleteBarcodeForErpDocument");
                success = false;
            }

            return success;
        }

        ///<summary>
        /// Dato un barcode e un erpDocumentId ritorna il record specifico
        ///</summary>
        //--------------------------------------------------------------------------------
        internal DMS_ErpDocBarcode GetErpDocBarcode(string barcode, int erpDocumentId)
        {
            try
            {
                var id = from bc in dc.DMS_ErpDocBarcodes
                         where bc.Barcode == barcode && bc.ErpDocumentID == erpDocumentId
                         select bc;

                if (id != null && id.Any())
                    return (DMS_ErpDocBarcode)id.Single();
            }
            catch (SqlException sqlEx)
            {
                SetMessage("SqlException", sqlEx, "GetErpDocBarcode");
            }
            catch (Exception ex)
            {
                SetMessage("Exception", ex, "GetErpDocBarcode");
            }

            return null;
        }

        ///<summary>
        /// Dato il valore di un barcode ritorna l'elenco dei documenti di ERP correlati
        /// ordinandoli per namespace
        ///</summary>
        //--------------------------------------------------------------------------------
        internal List<DMS_ErpDocument> GetPaperyListFromBarcode(string barcode)
        {
            List<DMS_ErpDocument> paperyList = new List<DMS_ErpDocument>();

            try
            {
                var docs = from bc in dc.DMS_ErpDocBarcodes
                           where bc.Barcode == barcode
                           select bc.DMS_ErpDocument;

                if (docs != null && docs.Any())
                    paperyList = docs.AsEnumerable().OrderBy(erpDoc => erpDoc.DocNamespace).ToList();
            }
            catch (SqlException sqlEx)
            {
                SetMessage("SqlException", sqlEx, "GetPaperyListFromBarcode");
            }
            catch (Exception ex)
            {
                SetMessage("Exception", ex, "GetPaperyListFromBarcode");
            }

            return paperyList;
        }

        ///<summary>
        /// Esegue la load di un documento da un AttachmentInfo
        /// A seconda dell'estensione vengono richiamate funzionalita' diverse:
        /// - .pdf: devo necessariamente salvare un file temporaneo sul file system
        /// - .*: tento prima di caricare le info dal database, dalla colonna DocContent.
        ///		se fallisco e sono compatibili con file di testo, allora provo a seguire la stessa procedura per i pdf
        ///		(salvo un temporaneo e lo analizzo)
        ///</summary>
        //---------------------------------------------------------------------
        private int LoadFromAttachmentInfo(AttachmentInfo currentAttach)
        {
            int imageId = 0;

            if (currentAttach.TempPath == null)
                return imageId;

            string ext = Path.GetExtension(currentAttach.TempPath);

            if (!CoreUtils.IsOCRCompatible(DMSOrchestrator.TextExtensions, ext))
                return imageId;

            try
            {
                switch (ext.ToLowerInvariant())
                {
                    case FileExtensions.DotPdf:
                        {
                            if (DMSOrchestrator.SaveAttachmentFile(currentAttach))
                                imageId = LoadFromFile(currentAttach.TempPath);
                            break;
                        }
                    case FileExtensions.TxtCompatible:
                        {
                            string pdfFileName = DMSOrchestrator.TransformToPdfA(currentAttach);
                            if (File.Exists(pdfFileName))
                                imageId = LoadFromFile(pdfFileName);
                            break;
                        }
                    default:
                        {
                            byte[] tempContent = null;

                            if (!currentAttach.IsAFile && currentAttach.DocContent != null && currentAttach.DocContent.Length > 0)
                            {
                                try
                                {
                                    tempContent = currentAttach.DocContent;
                                    imageId = LoadFromByteArray(tempContent);
                                }
                                catch (OutOfMemoryException)
                                {
                                    // se il GdViewer non riesce a caricare i byte
                                    // provo a caricare partendo dal file temporaneo
                                    if (DMSOrchestrator.SaveAttachmentFile(currentAttach))
                                        imageId = LoadFromFile(currentAttach.TempPath);
                                }
                            }
                            else
                                if (currentAttach.VeryLargeFile || currentAttach.IsAFile)
									imageId = LoadFromFile(currentAttach.TempPath);

							if (imageId == 0 && CoreUtils.IsTextCompatible(DMSOrchestrator.TextExtensions, ext))
							{
								// faccio cmq la Dispose
								tempContent = null;
								currentAttach.DisposeDocContent();
								
								goto case FileExtensions.TxtCompatible;
							}

                            tempContent = null;
                            currentAttach.DisposeDocContent();

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorGettingImageId, currentAttach.TempPath), e, "LoadFromAttachmentInfo");
            }

            return imageId;
        }

        ///<summary>
        /// Esegue la load di un documento da file che necessariamente deve essere presente su file system
        /// e ritorna l'imageId corrispondente
        ///</summary>
        //---------------------------------------------------------------------
        internal int LoadFromFile(string path)
        {
            int imageId = 0;

            if (string.IsNullOrEmpty(path))
                return imageId;

            string ext = Path.GetExtension(path);

            if (!CoreUtils.IsOCRCompatible(dc.TextExtensions, ext))
                return imageId;

            try
            {
                switch (ext.ToLowerInvariant())
                {
                    case FileExtensions.DotPdf:
                        {
                            if (gdPDF.LoadFromFile(path, false) == TBPictureStatus.OK)
                                imageId = gdPDF.RenderPageToGdPictureImage(DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.DpiQualityImage, false);
                            break;
                        }

                    case FileExtensions.TxtCompatible:
                        {
                            string pdfFileName = CoreUtils.TransformToPdfA(path, DMSOrchestrator.EasyAttachmentTempPath);
                            if (File.Exists(pdfFileName))
                            {
                                if (gdPDF.LoadFromFile(pdfFileName, false) == TBPictureStatus.OK)
                                    imageId = gdPDF.RenderPageToGdPictureImage(DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.DpiQualityImage, false);
                            }
                            break;
                        }
                    default:
                        {
                            imageId = gdPicture.CreateGdPictureImageFromFile(path);

                            if (gdPicture.GetStat() != TBPictureStatus.OK && CoreUtils.IsTextCompatible(dc.TextExtensions, ext))
                                goto case FileExtensions.TxtCompatible;

                            break;
                        }
                }
            }
            catch (Exception)
            {
                //SetMessage(string.Format(Strings.ErrorGettingImageId, path), e, "LoadFromFile");
            }

            return imageId;
        }

        ///<summary>
        /// Esegue la load di un documento leggendo il docContent dal database
        /// e ritorna l'imageId corrispondente
        ///</summary>
        //---------------------------------------------------------------------
        private int LoadFromByteArray(byte[] content)
        {
            int imageId = 0;

            try
            {
                imageId = gdPicture.CreateGdPictureImageFromByteArray(content);
            }
            catch (OutOfMemoryException e)
            {
                throw (e);
            }

            return imageId;
        }

        ///<summary>
        /// Metodo che ritorna un AttachmentInfo completo partendo dal valore di un barcode,
        /// effettuando tutti i vari controlli del caso
        ///</summary>
        //---------------------------------------------------------------------
        public AttachmentInfo GetAttachmentInfoFromBarcode(string barcodeValue, string notes, int erpDocumentID)
        {
            // controllo se esiste una riga nella DMS_ArchivedDocument con quel barcode
            DMS_ArchivedDocument archDoc = GetArchivedDocFromBarcodeValue(barcodeValue);

            // controllo se esiste una riga nella DMS_ErpDocBarcodes
            DMS_ErpDocBarcode erpDocBarcode = GetErpDocBarcode(barcodeValue, erpDocumentID); // togliere questo parametro

            if (erpDocBarcode != null)
                // unico caso che mi deve far comparire il pulsante Attach now sull'attachment			
                return new AttachmentInfo(erpDocBarcode, this.DMSOrchestrator); // creo un AttachmentInfo ad-hoc di tipo Papery

            if (archDoc != null)
            {
                // controllo se esiste un attachment per il documento di ERP con il barcode
                DMS_Attachment attachment = GetAttachment(archDoc.ArchivedDocID, erpDocumentID);
                if (attachment != null)
                {
                    // creo un AttachmentInfo ad-hoc di tipo Attachment esistente e mi posiziono direttamente su quello
                    return new AttachmentInfo(attachment, this.DMSOrchestrator);
                }
                else
                {
                    if (!DMSOrchestrator.InUnattendedMode)
                    {
                        // Mostra la form per scegliere quale azione effettuare
                        AttachmentInfo ai = new AttachmentInfo(archDoc, this.DMSOrchestrator);
                        DuplicateBarcodeForm paf = new DuplicateBarcodeForm();

                        if (paf.CanCreateAttachment(ai, DMSOrchestrator.CurrentTabPage))
                        {
                            AttachmentInfo attInfo = new AttachmentInfo(archDoc, this.DMSOrchestrator);
                            attInfo.IsMainDoc = true;
                            return attInfo;
                        }
                        else
                            return null;
                    }
                }
            }

            // non esiste il documento archiviato e non esiste il papery
            return new AttachmentInfo(barcodeValue, notes, this.DMSOrchestrator);
        }

        ///<summary>
        /// Metodo richiamato dall'UpdateCurrentAttachment (e nell'UpdateCurrentDocument)
        /// per effettuare i relativi controlli ed aggiornare (se possibile) il nuovo valore
        /// del barcode sul database
        ///</summary>
        //---------------------------------------------------------------------
        public bool UpdateBarcodeValue(AttachmentInfo attachmentInfo, string barcodeValue)
        {
            if (attachmentInfo == null)
                return false;

            if (string.Compare(attachmentInfo.TBarcode.Value, barcodeValue, StringComparison.InvariantCultureIgnoreCase) == 0)
                return true;

            // se non e' un papery e l'ArchivedDocId e' minore di zero non va bene e non procedo
            if (!attachmentInfo.IsAPapery && attachmentInfo.ArchivedDocId < 0)
                return false;

            DMS_ArchivedDocument archiveDoc = null;

            try
            {
                // controllo se esiste un documento archiviato con quell'id
                var var = (from aDoc in dc.DMS_ArchivedDocuments
                           where aDoc.ArchivedDocID == attachmentInfo.ArchivedDocId
                           select aDoc);

                // se esiste il documento archiviato allora procedo al suo update
                archiveDoc = (var != null && var.Any()) ? (DMS_ArchivedDocument)var.Single() : null;

                if (archiveDoc == null)
                    return false;

                // se il barcode e' diverso da empty controllo se esiste gia' un documento archiviato 
                // con quel valore. se esiste non posso cambiarlo
                if (!string.IsNullOrWhiteSpace(barcodeValue))
                {
                    // controllo se esiste un record diverso dal corrente con lo stesso barcode
                    var = (from aDoc in dc.DMS_ArchivedDocuments
                           where aDoc.ArchivedDocID != archiveDoc.ArchivedDocID &&
                           aDoc.Barcode == barcodeValue
                           select aDoc);

                    DMS_ArchivedDocument archiveDocWithSameBarcode = (var != null && var.Any()) ? (DMS_ArchivedDocument)var.Single() : null;

                    if (archiveDocWithSameBarcode != null)
                    {
                        // visualizzo un msg e non procedo
                        using (SafeThreadCallContext context = new SafeThreadCallContext())
                        {
                            InvalidBarcode ibForm = new InvalidBarcode();
                            if (DMSOrchestrator.CurrentTabPage != null)
                                ibForm.Owner = DMSOrchestrator.CurrentTabPage.FindForm();
                            ibForm.SetMessageText = string.Format(Strings.BarcodeValueAlreadyExists, barcodeValue);
                            ibForm.ShowDialog();
                        }

                        return false;
                    }
                }

                string lockMsg = string.Empty;
                // eseguo il lock del record
                if (DMSOrchestrator.LockManager.LockRecord(archiveDoc, DMSOrchestrator.LockContext, ref lockMsg))
                {
                    // aggiorno l'indice
                    DMSOrchestrator.SearchManager.UpdateBarcodeIndex(attachmentInfo, barcodeValue);
                    // aggiorno il valore del barcode nel record DMS_ArchivedDocument
                    archiveDoc.Barcode = barcodeValue;

                    archiveDoc.BarcodeType = DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeTypeValue;
                    dc.SubmitChanges();
                    attachmentInfo.TBarcode = new TypedBarcode(barcodeValue, BarcodeMapping.GetBarCodeType(archiveDoc.BarcodeType));
                    // unlock
                    DMSOrchestrator.LockManager.UnlockRecord(archiveDoc, DMSOrchestrator.LockContext);
                }
                else
                {
                    SetMessage(string.Format(Strings.ErrorUpdatingArchivedDoc, attachmentInfo.ArchivedDocId.ToString()), new Exception(lockMsg), "UpdateBarcodeValue");
                    return false;
                }
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorUpdatingArchivedDoc, attachmentInfo.ArchivedDocId.ToString()), e, "UpdateBarcodeValue");
                return false;
            }

            return true;
        }


        ///<summary>
        /// Metodo richiamato in fase di salvataggio del documento archiviato
        /// dal repository explorer. 
        /// Si occupa di ricercare tutti i documenti di ERP che hanno dei papery con 
        /// lo stesso barcode, istanzia il documento ed esegue l'attachment
        ///</summary>
        //---------------------------------------------------------------------
        public bool CheckAndAttachMultiplePapery(AttachmentInfo ai)
        {
            bool result = false;

            try
            {
                // carico le info del documento archiviato
                DMS_ArchivedDocument archDoc = this.DMSOrchestrator.SearchManager.GetArchivedDocument(ai.ArchivedDocId);
                if (archDoc == null)
                    return true;
                // controllo gli eventuali papery associati a quel barcode
                // la lista e' ordinata per namespace, in modo da ottimizzare 
                List<DMS_ErpDocument> paperyList = GetPaperyListFromBarcode(archDoc.Barcode);

                if (paperyList.Count == 0)
                    return true;

                if (DMSOrchestrator.InUnattendedMode)
                    result = AttachMultiplePendingPapery(archDoc, paperyList);
                else
                {
                    // se ne esiste almeno uno
                    // mostro la form per scegliere quale azione effettuare
                    using (SafeThreadCallContext context = new SafeThreadCallContext())
                    {
                        DuplicateBarcodeForm paf = new DuplicateBarcodeForm();
                        paf.InitForm(DuplicateBarcodeForm.DuplicateBarcodeType.BarcodeExistsInPapery);

                        // se l'utente ha scelto di creare direttamente gli allegati procedo
                        if (paf.CanCreateAttachment(ai, DMSOrchestrator.CurrentTabPage))
                            result = AttachMultiplePendingPapery(archDoc, paperyList);
                        else
                            result = true;
                    }
                }
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorAttachingMultiplePapery, ai.ArchivedDocId.ToString()), e, "CheckAndAttachMultiplePapery");
            }

            return result;
        }


        //---------------------------------------------------------------------
        private bool AttachMultiplePendingPapery(DMS_ArchivedDocument archiveDoc, List<DMS_ErpDocument> paperyList)
        {
            bool result = true;

            ERPDocumentManager edm = null;
            string currentNs = string.Empty;

            try
            {
                // i documenti nella lista sono gia' ordinato per namespace, in modo da ottimizzare
                // l'istanziazione dei documenti
                foreach (DMS_ErpDocument erpDoc in paperyList)
                {
                    // istanzio il documento di ERP per rottura di codice
                    if (string.IsNullOrWhiteSpace(currentNs) ||
                        string.Compare(currentNs, erpDoc.DocNamespace, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        if (edm != null)
                            edm.CloseDocument();

                        edm = new ERPDocumentManager(erpDoc.DocNamespace);
                        edm.DMSOrchestrator = this.DMSOrchestrator;
                        currentNs = erpDoc.DocNamespace;

                        // se non riesco ad aprire il documento continuo
                        if (!edm.OpenDocument())
                            continue;
                    }

                    int attachmentId = -1;
                    if (edm != null)
                        edm.Attach(archiveDoc.ArchivedDocID, erpDoc.PrimaryKeyValue, ref attachmentId);
                }

                if (edm != null)
                    edm.CloseDocument(); // lo fa solo per l'ultimo documento!
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorAttachingMultiplePapery, archiveDoc.ArchivedDocID.ToString()), e, "AttachMultiplePendingPapery");
            }

            return result;
        }

        //Massiva
        public event EventHandler<MassiveEventArgs> MassiveObjectAdded;
        public event EventHandler<MassiveEventArgs> MassiveRowProcessed;
        public event EventHandler<EventArgs> MassiveOperationCompleted;

        //--------------------------------------------------------------------------------
        void aiod_ProcessedAllDocuments(object sender, EventArgs args)
        {
            AttachmentInfoOtherData att = (AttachmentInfoOtherData)sender;

            if (att.FailedDocuments == 0)
                att.Result = MassiveResult.Done;
            else
                att.Result = (att.FailedDocuments == att.ProcessedDocuments) ? MassiveResult.Failed : MassiveResult.WithError;

            if (MassiveRowProcessed != null)
                MassiveRowProcessed(this, new MassiveEventArgs(att));
        }

		// per fare in modo che la OpenDocument venga fatta sul thread del documento gestionale
		System.Windows.Threading.Dispatcher myDispatcher;

		//--------------------------------------------------------------------------------
		public void MassiveProcessThread(List<AttachmentInfoOtherData> list)
        {
			// mi tengo da parte il thread del documento gestionale
			myDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;

			Thread myThread = new Thread(() => MassiveProcess(list));
            myThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            myThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            myThread.Start();
        }

        //--------------------------------------------------------------------------------
        internal void MassiveProcess(List<AttachmentInfoOtherData> list)
        {
            if (list == null || list.Count == 0)
                return;

            SortedDictionary<string, List<NamespaceDetails>> ht = new SortedDictionary<string, List<NamespaceDetails>>(StringComparer.CurrentCultureIgnoreCase);
            
			//per ogni riga
            foreach (AttachmentInfoOtherData aiod in list)
            {
                MassiveEventArgs args = new MassiveEventArgs(aiod);
                //niente da fare
                if (aiod.ActionToDo == MassiveAction.None)
                {
                    aiod.Result = MassiveResult.Ignored;

                    if (MassiveRowProcessed != null)
                        MassiveRowProcessed(this, args);
                    continue;
                }

                //riga invalida o già fatta
                if (aiod.Result != MassiveResult.Todo || aiod.ActionToDo == MassiveAction.None)
                    continue;
                int id = -1;

                //se devo archiviare procedo
                bool onlyarchive = (aiod.ActionToDo == MassiveAction.Archive || aiod.ActionToDo == MassiveAction.Substitute);
                if (aiod.Attachment.ArchivedDocId < 0 || onlyarchive)
                {
                    ArchiveResult res = (aiod.ActionToDo == MassiveAction.Substitute)
                        ? DMSOrchestrator.ArchiveManager.SubstituteDocumentUsingBarcode(Path.Combine(aiod.Attachment.OriginalPath, aiod.Attachment.Name), string.Empty, aiod.Attachment.TBarcode.Value, out id)
                        : DMSOrchestrator.ArchiveManager.ArchiveFile(Path.Combine(aiod.Attachment.OriginalPath, aiod.Attachment.Name), string.Empty, out id, false, true, aiod.Attachment.TBarcode.Value);

                    aiod.Diagnostic.Set(DMSOrchestrator.DMSDiagnostic);
                    DMSOrchestrator.DMSDiagnostic.Clear();
                    if (id > 0)
                        aiod.Attachment.ArchivedDocId = id;

                    //se dovevo solo archiviare ho finito oppure c'è stato un errore in fase di archiviaizone non devo proseguire
                    if (onlyarchive || res == ArchiveResult.TerminatedWithError || res == ArchiveResult.Cancel)
                    {
                        aiod.Result = (res == ArchiveResult.TerminatedWithError) ? MassiveResult.Failed : (res == ArchiveResult.Cancel) ? MassiveResult.WithError : MassiveResult.Done;
                        if (MassiveRowProcessed != null)
                            MassiveRowProcessed(this, args);
                        continue;
                    }
                }
                //se dovevo attachare invece
                //per ogni barcode-doc procedo con la creazione di una lista ordinata
                foreach (ERPDocumentBarcode bcd in aiod.ERPDocumentsBarcode)
                {
                    string primaryKey = bcd.PK;
                    string docNamespace = bcd.Namespace;
                    //dal docid recupero pk e ns
                    if (string.IsNullOrEmpty(docNamespace) || string.IsNullOrEmpty(primaryKey))
                        continue;

                    NamespaceDetails nsd = new NamespaceDetails();
                    nsd.ErpDocId = bcd.ErpDocID;
                    nsd.PrimaryKey = primaryKey;
                    nsd.ArchivedDocId = aiod.Attachment.ArchivedDocId;
                    nsd.AttInfoOtherData = aiod;
                    nsd.ErpDocumentBarcode = bcd;

                    if (!ht.Keys.Contains(docNamespace))
                        ht.Add(docNamespace, new List<NamespaceDetails> { nsd });
                    else
                        ht[docNamespace].Add(nsd);
                }
            }

			//per ottimizzare devo produrre una lista ordinata per namespace e per pk in modo da aprire il doc il minimo di volte necessarie
			//procedo con l'attach
			foreach (KeyValuePair<string, List<NamespaceDetails>> pair in ht)
            {
                ERPDocumentManager erpDocManager = new ERPDocumentManager(pair.Key);
                erpDocManager.DMSOrchestrator = DMSOrchestrator;

				// utilizzo il thread del documento gestionale
				myDispatcher.Invoke(() =>
				{
					if (!erpDocManager.OpenDocument())
						return;

					foreach (NamespaceDetails nsdett in pair.Value)
					{
						int attachmentId = -1;
						if (!erpDocManager.Attach(nsdett.ArchivedDocId, nsdett.PrimaryKey, ref attachmentId))
							nsdett.AttInfoOtherData.FailedDocuments++;
						else
							nsdett.AttInfoOtherData.Attachment.AttachmentId = attachmentId;

						nsdett.ErpDocumentBarcode.ErpDocDiagnostic.Set(erpDocManager.DocumentDiagnostic);
						erpDocManager.DocumentDiagnostic.Clear();
						nsdett.AttInfoOtherData.ProcessedDocuments++;
					}
					erpDocManager.CloseDocument();
				});
            }

            if (MassiveOperationCompleted != null)
                MassiveOperationCompleted(this, new EventArgs());
        }

        //--------------------------------------------------------------------------------
        internal Dictionary<string, Barcode> SplitFileUsingBarcode(string filePathName)
        {
            Dictionary<Barcode, int> splitData = null;
            //List<string> files = new List<string>();

            Dictionary<string, Barcode> files = null;

            if (FileExtensions.IsTifPath(filePathName))
                files = SplitTiffOnBarcode(filePathName);
            else
                if (FileExtensions.IsPdfPath(filePathName))
            {
                splitData = GetSplitDataOnBarcodeInPDFFile(filePathName);
                files = SplitPdfOnBarcode(splitData, filePathName);
            }

            if (files == null)
                files = new Dictionary<string, Barcode>();

            if (files.Count == 0 && File.Exists(filePathName))
                files.Add(filePathName, new Barcode());

            return files;
        }

        //--------------------------------------------------------------------------------
        private void DeepAttach(DirectoryInfo dir, bool splitFile, ref List<AttachmentInfoOtherData> targetList)
        {
            if (dir == null || !dir.Exists)
                return;

            if (dir.GetDirectories().Length > 0)
                foreach (DirectoryInfo f in dir.GetDirectories())
                    DeepAttach(f, splitFile, ref targetList);
            int archivedDocId = 0;

            Dictionary<string, Barcode> files = new Dictionary<string, Barcode>();

            foreach (FileInfo f in dir.GetFiles())
            {
                if (splitFile && (FileExtensions.IsTifPath(f.FullName) || FileExtensions.IsPdfPath(f.FullName)))
                {
                    files = SplitFileUsingBarcode(f.FullName);
                    if (files != null)
                        foreach (KeyValuePair<string, Barcode> fb in files)
                            if (!string.IsNullOrWhiteSpace(fb.Key))
                            {
                                AttachmentInfoOtherData aiod = PrepareAttachment(new AttachmentInfo(--archivedDocId, new FileInfo(fb.Key), DMSOrchestrator), fb.Value);
                                if (aiod != null)
                                    targetList.Add(aiod);
                            }
                }
                else
                {
                    AttachmentInfoOtherData aiod = PrepareAttachment(new AttachmentInfo(--archivedDocId, f, DMSOrchestrator), new Barcode());
                    if (aiod != null)
                        targetList.Add(aiod);
                }
            }
        }


        //questo metodo viene chiamato via WEBMethod per consentire di eseguire la massiva anche in modalità unattended mediante procedure personalizzate e schedulabile (x cui fatte con TaskBuilder Framework)
        //dato un folder (o in rete o locale) per ogni file 
        //--------------------------------------------------------------------------------
        internal MassiveAttachInfo MassiveAttachUnattendedMode(string folder, bool splitFile)
        {
            List<AttachmentInfoOtherData> list = new List<AttachmentInfoOtherData>();
            DeepAttach(new DirectoryInfo(folder), splitFile, ref list);
            MassiveProcess(list);

            MassiveAttachInfo massiveAttachInfo = new MassiveAttachInfo();
            foreach (AttachmentInfoOtherData aiod in list)
                massiveAttachInfo.AddAttachmentInfo(aiod);

            return massiveAttachInfo;
        }

        //--------------------------------------------------------------------------------
        internal AttachmentInfoOtherData PrepareAttachment(AttachmentInfo anAttachmentInfo, Barcode barcode)
        {
            if (anAttachmentInfo == null)
                return null;

            AttachmentInfoOtherData aiod = new AttachmentInfoOtherData(anAttachmentInfo);
            aiod.ProcessedAllDocuments += new AttachmentInfoOtherData.ProcessedEventHandler(aiod_ProcessedAllDocuments);

            List<ERPDocumentBarcode> list = new List<ERPDocumentBarcode>();
            MassiveStatus stat = MassiveStatus.NoBC;

            //dal bc del doc recupero l'erpdocid
            string bc = (barcode != null && !barcode.Value.IsNullOrEmpty()) ? barcode.Value : DetectBarcodeValueInFile(anAttachmentInfo).Value;
            if (String.IsNullOrWhiteSpace(bc))
                stat = MassiveStatus.NoBC;
            else
            {
                anAttachmentInfo.TBarcode.Value = bc;

                bool existBC = BarcodeAlreadyExists(bc, anAttachmentInfo.ArchivedDocId); // se esiste nel repository

                List<DMS_ErpDocument> doclist = GetErpDocumentFromBarCode(bc);
                if (doclist == null || doclist.Count == 0)
                    stat = (existBC) ? MassiveStatus.BCDuplicated : MassiveStatus.OnlyBC;
                else
                {
                    foreach (DMS_ErpDocument doc in doclist)
                    {
                        if (doc == null || doc.ErpDocumentID < 0)
                            continue;

                        ERPDocumentBarcode erpDoc = new ERPDocumentBarcode();
                        erpDoc.PK = doc.PrimaryKeyValue;
                        erpDoc.Namespace = doc.DocNamespace;
                        erpDoc.ErpDocID = doc.ErpDocumentID;

                        list.Add(erpDoc);
                    }

                    stat = (list.Count == 0) ? ((existBC) ? MassiveStatus.BCDuplicated : MassiveStatus.OnlyBC) : MassiveStatus.Papery;
                }
            }

            aiod.BarCodeStatus = stat;
            aiod.ERPDocumentsBarcode = list;

            return aiod;
        }

        //--------------------------------------------------------------------------------
        internal void MassivePreProcess(AttachmentInfo anAttachmentInfo, Barcode barcode)
        {

        }

        //New MassiveAttach with C++ user interface
        //@@BAUZI
        //--------------------------------------------------------------------------------
        public void MassivePreProcess(List<string> filesToAttach, bool splitFileOnBC)
        {
            Dictionary<string, Barcode> files = new Dictionary<string, Barcode>();
            int archivedDocId = 0;
            foreach (string path in filesToAttach)
            {
                //se devo cerco in ogni file il barcode e mi segno la pagina
                //ogni file pdf e tiff verra'scannato per trovare i barcode presenti e quindi verranno separati in piú file, prendendo come prima pagina quella col barcode

                if (splitFileOnBC)
                    files = SplitFileUsingBarcode(path);
                else
                    files.Add(path, new Barcode()); //se non devo eseguiro lo split considero il file originario
            }

            List<AttachmentInfoOtherData> aioList = new List<AttachmentInfoOtherData>();
            Dictionary<string, int> existBarcodes = new Dictionary<string, int>();

            foreach (KeyValuePair<string, Barcode> f in files)
                if (!string.IsNullOrWhiteSpace(f.Key))
                {

                    AttachmentInfoOtherData aiod = PrepareAttachment(new AttachmentInfo(--archivedDocId, new FileInfo(f.Key), DMSOrchestrator), f.Value); //vado in negativo per non andare in conflitto con codici esistenti. 
                    if (
                            (aiod.Attachment.ArchivedDocId < 0 && aioList.Exists(a => string.Compare(a.Attachment.TempPath, aiod.Attachment.TempPath, true) == 0)) ||
                            (aiod.Attachment.ArchivedDocId > 0 && aioList.Exists(a => a.Attachment.ArchivedDocId == aiod.Attachment.ArchivedDocId))
                        )
                    {
                        aiod.Result = MassiveResult.PreFailed;
                        aiod.BarCodeStatus = MassiveStatus.ItemDuplicated;
                    }

                    if (!string.IsNullOrWhiteSpace(aiod.Attachment.TBarcode.Value) && aioList.Exists(a => string.Compare(a.Attachment.TBarcode.Value, aiod.Attachment.TBarcode.Value, true) == 0))
                        aiod.BarCodeStatus = MassiveStatus.BCDuplicated;

                    //aiod.PossibleActions.Add(MassiveAction.None);

                    if (aiod.BarCodeStatus == MassiveStatus.BCDuplicated)
                    {
                        switch (DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BCActionForBatch)
                        {
                            case DuplicateDocumentAction.ReplaceExistingDoc:
                                aiod.ActionToDo = MassiveAction.Substitute; break;//se i setting dicono di sostituire i doc con stesso barcode uso la stessa impostazione anche qua
                            case DuplicateDocumentAction.ArchiveAndKeepBothDocs:
                                aiod.ActionToDo = MassiveAction.Archive; break;
                        }
                    }

                    //if (aiod.ActionToDo != MassiveAction.None && aiod.Result != MassiveResult.PreFailed)//se none o fallito non aggiungo altre opzioni
                    //    aiod.PossibleActions.Add(aiod.ActionToDo);
                    
                    aioList.Add(aiod);

                    if (MassiveObjectAdded != null)
                        MassiveObjectAdded(this, new MassiveEventArgs(aiod));
                }
        }
    }   
}
