using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.EasyAttachment.UI.Controls;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.UI.WinControls;
using Microarea.TaskBuilderNet.UI.WinControls.Generic;

namespace Microarea.EasyAttachment.UI.Forms
{
    //=========================================================================
    public partial class MassiveAttach : MenuTabForm
    {
        private DMSOrchestrator dmsOrchestrator = null;
        private int archivedDocId = 0;
        private List<AttachmentInfoOtherData> ailist = new List<AttachmentInfoOtherData>();
        private List<AttachmentInfoOtherData> asynchList = new List<AttachmentInfoOtherData>();
        private List<string> deviceFiles = new List<string>();
        private bool splitFileOnBC;

        //--------------------------------------------------------------------------------
        public MassiveAttach(DMSOrchestrator dmsOrch)
        {
            InitializeComponent();

            LblNoBC.Text = Strings.NoBarcode;
            LblInfo .Text = Strings.SomeError;
            LblOK.Text = Strings.OneOrMoreBC;
            LblNoPending.Text = Strings.OnlyBarcode;
            LblDuplicateBC.Text = Strings.Success;
            LblError.Text = Strings.MassiveError;
			LblDuplicateBC.Text = Strings.DuplicatedBC;
			LblSuccess.Text = Strings.Success;

			PbFailed.Image = MassiveAttachImageList.GetResultImage(MassiveResult.Failed);
			PbSuccess.Image = MassiveAttachImageList.GetResultImage(MassiveResult.Done);
			PbWithError.Image = MassiveAttachImageList.GetResultImage(MassiveResult.WithError);

			PbNoBc.Image = MassiveAttachImageList.GetStatusImage(MassiveStatus.NoBC);
			PbOnlyBarcode.Image = MassiveAttachImageList.GetStatusImage(MassiveStatus.OnlyBC);
			PbPapery.Image = MassiveAttachImageList.GetStatusImage(MassiveStatus.Papery);
			PbDuplicateBC.Image = MassiveAttachImageList.GetStatusImage(MassiveStatus.BCDuplicated);

            dmsOrchestrator = new DMSOrchestrator();
            dmsOrchestrator.InitializeManager(dmsOrch);
			dmsOrchestrator.InUnattendedMode = true;
            massiveAttachResult1.Orchestrator = dmsOrch;
            massiveAttachResult1.Rows.CollectionChanged += new CollectionChangeEventHandler(Rows_CollectionChanged);
			massiveAttachResult1.RenderingBarcode += new MassiveAttachResult.RenderingBarcodeDelegate(massiveAttachResult1_RenderingBarcode);
         
            dmsOrchestrator.BarcodeManager.MassiveObjectAdded += new EventHandler<MassiveEventArgs>(BarcodeManager_MassiveObjectAdded);
            dmsOrchestrator.BarcodeManager.MassiveRowProcessed += new EventHandler<MassiveEventArgs>(BarcodeManager_MassiveRowProcessed);
			//dmsOrchestrator.BarcodeManager.MassiveProcessTerminated += new EventHandler(BarcodeManager_MassiveProcessTerminated);

			
        }
		

		//--------------------------------------------------------------------------------
        private Barcode massiveAttachResult1_RenderingBarcode(TypedBarcode barcode)
		{
			return dmsOrchestrator.BarcodeManager.GetBarcodeImageFromValue(barcode);
		}

		//--------------------------------------------------------------------------------
		void BarcodeManager_MassiveObjectAdded(object sender, MassiveEventArgs args)
		{
			massiveAttachResult1.Add(args.aiod);

			asynchList.Add(args.aiod);
			ailist.Add(args.aiod);
		}

        //--------------------------------------------------------------------------------
		void BarcodeManager_MassiveRowProcessed(object sender, MassiveEventArgs args)
        {
            progressBar1.Increment(1);
            if (args != null && args.aiod != null)
            {
				massiveAttachResult1.RowProcessing(args.aiod.RowIndex);
				dmsOrchestrator.FireMassiveRowProcessed(args);
			}
        }

		//--------------------------------------------------------------------------------
		void BarcodeManager_MassiveProcessTerminated(object sender, EventArgs e)
		{
			dmsOrchestrator.FireMassiveProcessTerminated();
		}
        
        //---------------------------------------------------------------------
        public void Wait(bool wait)
        {
            if (wait)
            {
                LblProcEnded.Visible = false;
                Cursor.Current = Cursors.WaitCursor;   
            }
            else
            {
                progressBar1.Visible = false;
                Cursor.Current = Cursors.Default;
            }
        }

        //--------------------------------------------------------------------------------
        void Rows_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            BtnOK.Enabled = TsbDelete.Enabled = TsbRefresh.Enabled = (massiveAttachResult1.Rows.Count > 0);
        }

        //--------------------------------------------------------------------------------
        private void tsBntChooseFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.AutoUpgradeEnabled = true;
            
            //openFileDialog
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = String.Format("{0} (*.*)|*.*", Strings.AllFiles);

            if (openFileDialog.ShowDialog(this) != DialogResult.OK ||
                string.IsNullOrWhiteSpace(openFileDialog.FileName))
                return;

            Wait(true);
            PrepareProgress(openFileDialog.FileNames.Count());
            Attach(openFileDialog.FileNames);
            Wait(false);
        }

        //--------------------------------------------------------------------------------
        private void PrepareProgress(int max)
        { 
            progressBar1.Visible = true;
            progressBar1.Maximum = max+1;
            progressBar1.Step = 1;
            progressBar1.Value = 1;//incomincia
        }

        //--------------------------------------------------------------------------------
        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        //--------------------------------------------------------------------------------
        private void repositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //using (SafeThreadCallContext context = new SafeThreadCallContext())
            //{
            //    Wait(true);
            //    RepositoryBrowser rep = new RepositoryBrowser(this, dmsOrchestrator);
            //    rep.StartingFilter = RepositoryBrowser.Filter.Papery;
            //    List<AttachmentInfo> attachmentInfos = rep.GetArchivedDocs(this);
            //    if (attachmentInfos != null && attachmentInfos.Count > 0)
            //    {
            //        PrepareProgress(attachmentInfos.Count);
            //        foreach (AttachmentInfo attachmentInfo in attachmentInfos)
            //            Attach(attachmentInfo, null);

            //    }
            //    Wait(false);
            //}
        }

        //--------------------------------------------------------------------------------
        private void Attach(AttachmentInfo ai, Barcode barcode)
        {
            //possibilità di splittare doc nel repository eliminata per questioni di conflitto di barcode nella prima pagina del doc
            //if (IsToSplit(ai.TempPath) && ai.ArchivedDocId >= 0) //se minore di zero vuol dire che era già partito da file
            //{
            //    //se devo splittare i doc sul barcode lo faccio anche da quelli trascinati da repository
            //    Attach(ai.TempPath);
            //    return;
            //}
            progressBar1.Increment(1);
			dmsOrchestrator.BarcodeManager.MassivePreProcess(ai, barcode);
        }

        //--------------------------------------------------------------------------------
        private void Attach(string[] paths)
        {
            if (paths == null || paths.Length == 0) return;
            foreach (string path in paths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                if (File.Exists(path))
                    Attach(path);
                else if (Directory.Exists(path))
                    DeepAttach(new DirectoryInfo(path), false);
            }
        }

        //--------------------------------------------------------------------------------
        private void Attach(string path)
        {
			Dictionary<string, Barcode> files = new Dictionary<string, Barcode>();

            //se devo cerco in ogni file il barcode e mi segno la pagina
            //ogni file pdf e tiff verra'scannato per trovare i barcode presenti e quindi verranno separati in piú file, prendendo come prima pagina quella col barcode

            if (splitFileOnBC)
				files = dmsOrchestrator.BarcodeManager.SplitFileUsingBarcode(path);
			else
				files.Add(path, new Barcode()); //se non devo eseguiro lo split considero il file originario

			foreach (KeyValuePair<string, Barcode> f in files)
				if (!string.IsNullOrWhiteSpace(f.Key))				
					Attach(new AttachmentInfo(--archivedDocId, new FileInfo(f.Key), dmsOrchestrator), f.Value); //vado in negativo per non andare in conflitto con codici esistenti.
        }

        //--------------------------------------------------------------------------------
        private bool IsToSplit(string path)
        {
            return (splitFileOnBC && (FileExtensions.IsTifPath(path) || FileExtensions.IsPdfPath(path)));
        }

        //--------------------------------------------------------------------------------
        private void deviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // bisogna fare cosi', altrimenti c'e' un bell'errore di ThreadSafeCall!
            using (SafeThreadCallContext context = new SafeThreadCallContext())
            {
                Wait(true);

                // demando al codice della form gli opportuni controlli prima di aprirla
                List<string> acquiredFileList = Acquisition.OpenForm(dmsOrchestrator.BarcodeManager, splitFileOnBC);

                if (acquiredFileList != null && acquiredFileList.Count > 0)
                {
                    PrepareProgress(acquiredFileList.Count);
                    this.Update();

                    foreach (string file in acquiredFileList)
                    {
                        if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                            continue;

                        Attach(file);
                        // alla fine poi  eliminerò il file
                        deviceFiles.Add(file);
                    }
                }
                Wait(false);
            }
        }

        //--------------------------------------------------------------------------------
        private void DeepAttach(DirectoryInfo dir, bool deep)
        {
            if (dir == null || !dir.Exists) 
                return;

            if (dir.GetDirectories().Length > 0 &&
                (deep ||
                MessageBox.Show(this, Strings.DeepAttach, Strings.ArchivedDocument, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)) 
                foreach (DirectoryInfo f in dir.GetDirectories())
                    DeepAttach(f, true);

            foreach (FileInfo f in dir.GetFiles())
                Attach(f.FullName);
        }
      
        //--------------------------------------------------------------------------------
        private void MassiveAttach_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Wait(true);
                // faccio direttamente l'Attach perche' non ho effettuato il drag sul GdViewer
                // quindi non devo caricare il documento
                string[] dropping = (string[])(e.Data.GetData(DataFormats.FileDrop));

                PrepareProgress(dropping.Length);
                Attach(dropping);
                Wait(false);
            }
            else if (e.Data.GetDataPresent("FileGroupDescriptor"))
            {
                try
                {
                    Wait(true);
                    //wrap standard IDataObject in MailDataObject
                    MailDataObject dataObject = new MailDataObject(e.Data);

                    //get the names and data streams of the files dropped
                    string[] filenames = (string[])dataObject.GetData("FileGroupDescriptor");
                    MemoryStream[] filestreams = (MemoryStream[])dataObject.GetData("FileContents");
                    PrepareProgress(filenames.Length);
                    for (int fileIndex = 0; fileIndex < filenames.Length; fileIndex++)
                    {
                        //use the fileindex to get the name and data stream
                        string filename = this.dmsOrchestrator.GetArchiveDocTempFileName(filenames[fileIndex]);
                        MemoryStream filestream = filestreams[fileIndex];

                        //save the file stream using its name to the application path
                        FileStream outputStream = File.Create(filename);
                        filestream.WriteTo(outputStream);
                        outputStream.Close();
                        Attach(filename);
                    }

                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.ToString());
                }
                finally { Wait(false); }
            }
        }

        //--------------------------------------------------------------------------------
        private void MassiveAttach_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if ((e.Data.GetDataPresent(DataFormats.FileDrop)) || (e.Data.GetDataPresent("FileGroupDescriptor")))
                e.Effect = DragDropEffects.Copy;
        }
        
        //--------------------------------------------------------------------------------
        private void Go()
        {  
            List<AttachmentInfoOtherData> list = new List<AttachmentInfoOtherData>();
            foreach (DataGridViewRow row in massiveAttachResult1.Rows)
            {
                AttachmentInfoOtherData aiod = row.Cells[1].Tag as AttachmentInfoOtherData;
                aiod.RowIndex = row.Index;
                list.Add(aiod);
            }
            dmsOrchestrator.BarcodeManager.MassiveProcess(list);
        }

        //--------------------------------------------------------------------------------
        private void BtnOK_Click(object sender, EventArgs e)
        {
            LblProcEnded.Visible = false;
            PrepareProgress(massiveAttachResult1.Rows.Count);
            Wait(true);

            Go();

            BtnOK.Enabled = false;
            TsbRefresh.Enabled = false;
            TsbDelete.Enabled = false;

            LblProcEnded.Visible = true;
            LblProcEnded.Text = Strings.ProcedureTerminated;

            DeleteDeviceFiles();
            Wait(false);
            
        }

         //--------------------------------------------------------------------------------
        private void DeleteDeviceFiles()
        {
            try
            {
                if (deviceFiles != null && deviceFiles.Count > 0)
                    foreach (string file in deviceFiles)
                        Utils.DeleteFile(new FileInfo(file));
            }
            catch { }
        }

        //--------------------------------------------------------------------------------
        private void TsbDelete_Click(object sender, EventArgs e)
        {
            LblProcEnded.Visible = false;
            Cursor.Current = Cursors.WaitCursor;
            massiveAttachResult1.DeleteSelectedRows();
            Cursor.Current = Cursors.Default;
        }

        //--------------------------------------------------------------------------------
        private void TsbRefresh_Click(object sender, EventArgs e)
        {
            Wait(true);
            PrepareProgress(massiveAttachResult1.Rows.Count * 2);
            LblProcEnded.Visible = false;
            //per evitare casini coi duplicati devo prima togliere tutto e poi aggiungere
            List<AttachmentInfoOtherData> list = new List<AttachmentInfoOtherData>();

            foreach (DataGridViewRow row in massiveAttachResult1.Rows)
            {
                AttachmentInfoOtherData aiod = row.Cells[1].Tag as AttachmentInfoOtherData;
                progressBar1.Increment(1);
                if (aiod == null || aiod.Attachment == null || aiod.Result != MassiveResult.Todo || aiod.ActionToDo == MassiveAction.None)
                {
                    progressBar1.Increment(1);// perchè poi non viene aggiunto il relativo attachemnt, viene tralasciato, non terminerebbe la progress
                    continue;//tralascio i falliti e i fatti
                }
                list.Add(aiod);

            }
            massiveAttachResult1.Clear();
            foreach (AttachmentInfoOtherData aiod in list)
                Attach(aiod.Attachment, null);

            Wait(false);
        }

        //---------------------------------------------------------------------
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            splitFileOnBC = toolStripButton1.Checked;
            toolStripButton1.Image = 
                splitFileOnBC ? 
                Microarea.EasyAttachment.Properties.Resources.check_selected : 
                Microarea.EasyAttachment.Properties.Resources.check_unselected;
            repositoryToolStripMenuItem.Enabled = !splitFileOnBC;

           if (splitFileOnBC) 
			   MessageWindow.ShowDialog(this, Strings.WarningSplitOnBarcode,Strings.Warning);
        }
    }
}
