using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.TaskBuilderNet.UI.WinControls;
namespace Microarea.EasyAttachment.UI.Controls
{
    //---------------------------------------------------------------------
    public partial class DocumentDetails : UserControl
    {

        string notShrinkedPath = null;
        AttachmentInfo attachment = null;

        //---------------------------------------------------------------------
        public DocumentDetails()
        {
            InitializeComponent();
        }

        //---------------------------------------------------------------------
		public void Populate(DMS_ArchivedDocument archDoc, DMSOrchestrator dmsOrchestrator)
        {
			//PathShrinker ps = new PathShrinker();
			//notShrinkedPath = archDoc.Path;
			//LblFileName.Text = Path.GetFileName(archDoc.Name);
			//LblFilePath.Text = ps.Shrink(archDoc.Path, LblFilePath);
			//this.toolTip1.SetToolTip(this.LblFilePath, archDoc.Path);
			//long size = (archDoc.Size > 1024) ? archDoc.Size / 1024 : 1;
			//LblSize.Text = String.Format(Strings.KBytes, size.ToString());
			//LblModifiedDate.Text = archDoc.LastWriteTimeUtc.ToLocalTime().ToString();
			//LblCreationDate.Text = archDoc.CreationTimeUtc.ToLocalTime().ToString();
			//LblCreatedBy.Text = archDoc.TBCreatedID;
			//PbFile.Image = Utils.GetMediumImage(archDoc.ExtensionType);
        }

        //---------------------------------------------------------------------
        public void Populate(AttachmentInfo attach)
        {
            attachment = attach;
            PathShrinker ps = new PathShrinker();
            notShrinkedPath = attach.OriginalPath;
            LblFileName.Text = Path.GetFileName(attach.Name);
            LblFilePath.Text = ps.Shrink(attach.OriginalPath, LblFilePath);
            this.toolTip1.SetToolTip(this.LblFilePath, attach.OriginalPath);
			LblSizeValue.Text = String.Format(Strings.KBytes, attach.KBSize.ToString());
			LblArchivedDateValue.Text = attach.ArchivedDate.ToString();
            LblStorage.Visible = (attach.StorageType == StorageTypeEnum.FileSystem);
            LblStorageValue.Visible = (attach.StorageType == StorageTypeEnum.FileSystem);
            LblStorageValue.Text = attach.StorageFile;

            if (attach.AttachmentId < 0)
            {
                LblAttachedDateValue.Visible = false;
                LblAttachedDate.Visible = false;
                LblAttachmentIDValue.Visible = false;
                LblArchivedDocIDValue.Visible = false;
                LblAttachmentID.Visible = false;
                LblArchivedDocID.Visible = false;
            }
            else
            {
                LblAttachedDateValue.Text = attach.AttachedDate.ToString();
                LblArchivedDocIDValue.Text = attach.ArchivedDocId.ToString();
                LblAttachmentIDValue.Text = attach.AttachmentId.ToString();
            }

			LblCreatedByValue.Text = attach.CreatedBy;
			LblModifiedByValue.Text = attach.ModifiedBy;
            PbFile.Image = Utils.GetMediumImage(attach.ExtensionType);
        }

        //---------------------------------------------------------------------
        internal void Clear()
        {
            LblFileName.Text = Strings.Notavailable;
            LblFilePath.Text = Strings.Notavailable;
            this.toolTip1.SetToolTip(this.LblFilePath, Strings.Notavailable);
            LblSizeValue.Text = String.Format(Strings.KBytes, Strings.Notavailable);
			LblArchivedDateValue.Text = Strings.Notavailable;
			LblAttachedDateValue.Text = Strings.Notavailable;
            PbFile.Image = null;
        }

        //---------------------------------------------------------------------
        private void LblFilePath_SizeChanged(object sender, EventArgs e)
        {
            PathShrinker ps = new PathShrinker();
            LblFilePath.Text = ps.Shrink(notShrinkedPath, LblFilePath);

		}

        //---------------------------------------------------------------------
        private void PbFile_Click(object sender, EventArgs e)
        {

            if (attachment == null)
                return;

            try
            {
                if (attachment.SaveAttachmentFile())
                    Process.Start(attachment.TempPath);

                else// dovrei forse verificare che non sia archiviato o cose del genere?
                {
                    string originalfile = Path.Combine(attachment.OriginalPath, attachment.Name);
                    if (File.Exists(originalfile)) 
                        Process.Start(originalfile);
                }
                
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
            }
        }
    }
}

