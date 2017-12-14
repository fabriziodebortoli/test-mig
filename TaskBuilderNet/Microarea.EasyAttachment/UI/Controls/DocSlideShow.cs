using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.EasyAttachment.Properties;
using Microarea.TBPicComponents;

namespace Microarea.EasyAttachment.UI.Controls
{
	//================================================================================
	public partial class DocSlideShow : UserControl
	{
		public enum ViewMode { SlideShow, Grid }; // tipo di visualizzazione
		public enum SelectionType { Single, Multiple }; //tipo di selezione fatta dall'utente.S erve per pilotare lo stato dei control

		private SelectionType userSelectionType = SelectionType.Single;
		private string title = String.Empty;
		private int panelBusyCount = 0;
		private ViewMode docViewMode = ViewMode.SlideShow;
		private SlideShowPanel currentPanel = null;
		private IList<SlideShowPanel> panels = new List<SlideShowPanel>();
		private List<AttachmentInfo> attachments = new List<AttachmentInfo>();
        private TBPicPDF gdPDF = new TBPicPDF();
		// cache dei documenti
		private Dictionary<int, Control> previewControls = new Dictionary<int, Control>();

		public bool drag = false;

		// Properties
		//---------------------------------------------------------------------
		public AttachmentInfo CurrentDoc { get; set; }
		public ViewMode DocViewMode { get { return docViewMode; } set { docViewMode = value; ChangeViewMode(); } }

		public SelectionType UserSelectionType
		{
			get { return userSelectionType; }
			set { userSelectionType = value; if (SelectionTypeChanged != null) SelectionTypeChanged(this, EventArgs.Empty); }
		}

		public bool DeleteVisible { set { BtnRemoveAttach.Visible = value; } }
		public bool RefreshVisible { set { BtnRefresh.Visible = value; } }
		public bool BtnPaperyDocsVisible { set { BtnPaperyDocs.Visible = value; } }
		public bool BtnSwitchViewVisible { set { BtnSwitcView.Visible = value; } }
		public bool BrowseBtnsVisible { set { BtnRight.Visible = value; BtnLeft.Visible = value; } }

		public string NoAttachLabelText { set { LblNoAttach.Text = value; } }
		public bool ShowOnlyPaperyDocsChecked { get { return BtnPaperyDocs.CheckState == CheckState.Checked; } }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<AttachmentInfo> AttachmentInfoList { set { attachments = value; Populate(); } get { return attachments; } }

		public List<AttachmentInfo> SelectedDocuments
		{
			get
			{
				List<AttachmentInfo> selectedDocuments = new List<AttachmentInfo>();
				if (docViewMode == ViewMode.SlideShow)
					selectedDocuments.Add(CurrentDoc);
				else
				{
					for (int j = 0; j < DocListView.SelectedItems.Count; j++)
						selectedDocuments.Add((AttachmentInfo)DocListView.SelectedItems[j].Tag);
				}
				return selectedDocuments;
			}
		}

		// per personalizzare il titolo della groupbox e' necessario utilizzare questa property
		//---------------------------------------------------------------------
		public string Title
		{
			get { return title; }
			set { title = value; if (!String.IsNullOrWhiteSpace(value)) GBRecent.Text = "      " + value; }
		}

		//---------------------------------------------------------------------
		public int Count { get { return (docViewMode == ViewMode.SlideShow) ? panels.Count : DocListView.Items.Count; } }

		// Events
		//---------------------------------------------------------------------
		public event EventHandler OpenAttach;
		public event EventHandler DeleteAttach;
		public event EventHandler OpenDocument;
		public event EventHandler CurrentDocChanging;
		public event EventHandler CountChanged;
		public event EventHandler RefreshAttachments;
		public event EventHandler ShowPaperyDocs;
		public event EventHandler SelectionTypeChanged;

		///<summary>
		/// Constructor
		///</summary>
		//---------------------------------------------------------------------
		public DocSlideShow()
		{
			InitializeComponent();
			CurrentDoc = null;

			DocListView.ItemDrag += new ItemDragEventHandler(DocListView_ItemDrag);
			DocListView.SizeChanged += new EventHandler(DocListView_SizeChanged);
			DocListView.MouseDoubleClick += new MouseEventHandler(DocListView_MouseDoubleClick);
		}

		//---------------------------------------------------------------------
		void DocListView_SizeChanged(object sender, EventArgs e)
		{
			//l'ultima colonna si ridimensiona in base alla dimensione del controllo, 
			//esiste questo magic number -2 ma talvolta fa casino e fa comparire la scroll
			//allora tolgo 2  per essere sicuri che la scroll non compaia
			//DocListView.Columns[DocListView.Columns.Count - 1].Width = -2;//MAGIC NUMBER

			DocListView.Columns[DocListView.Columns.Count - 1].Width -= 2;
		}

		//---------------------------------------------------------------------
		void DocListView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			DoDragDrop();
		}

		//---------------------------------------------------------------------
		public void Clear()
		{
			Clear(true);
		}

		//---------------------------------------------------------------------
		public void Clear(bool all)
		{
			foreach (SlideShowPanel p in panels)
				PanelSlideShow.Controls.Remove(p);

			previewControls.Clear();
			panels.Clear();
			panelBusyCount = 0;

			//griglia
			DocListView.Items.Clear();

			CurrentDoc = null;
			ClearControls();
			WriteDocCount();
		}

		//---------------------------------------------------------------------
		public void Prepare()
		{
			if (docViewMode == ViewMode.SlideShow)
			{
				if (panels == null)
					return;

				if (panels.Count == 0)
					ClearControls();
				else
					PrepareControls();
			}
			else/*griglia*/
			{
				if (DocListView.Items.Count == 0)
					ClearControls();
				else
					PrepareControls();
			}

			WriteDocCount();
			//if (Populated != null)
			//    Populated(Count, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		public void ClearControls()
		{
			LblNoAttach.Visible = true;
			LblNoAttach.BringToFront();
			PBNoAttach.Visible = true;
			PBNoAttach.BringToFront();
			BtnRight.Enabled = false;
			BtnLeft.Enabled = false;
			BtnRemoveAttach.Enabled = false;
			// anche se non ci sono doc posso voler refreshare per verificare che non siano stati allegati docs (per esempio da una massiva)
			BtnRefresh.Enabled = true;
			BtnPaperyDocs.Enabled = false;
			BtnSwitcView.Enabled = false;
			GetDocument(null);
			LblDocName.Visible = false;
			LblDocName.Text = string.Empty;
			CurrentDoc = null;
		}

		//---------------------------------------------------------------------
		public void PrepareControls()
		{
			LblDocName.Visible = true;
			LblNoAttach.Visible = false;
			PBNoAttach.Visible = false;
			BtnRight.Enabled = true;
			BtnLeft.Enabled = true;
			BtnRemoveAttach.Enabled = true;
			BtnRefresh.Enabled = true;
			BtnPaperyDocs.Enabled = true;
			BtnSwitcView.Enabled = true;
		}

		//------------------------------------------------------------------------------------------------------
		private void WriteDocCount()
		{
			string val = GBRecent.Text;
			if (String.IsNullOrWhiteSpace(val)) return;
			int i = val.IndexOf("(");
			if (i != -1)
				val = val.Substring(0, i - 1);

			GBRecent.Text = val + String.Format(" ({0})", Count);
		}

		//------------------------------------------------------------------------------------------------------
		private Control GetControl(AttachmentInfo attachInfo)
		{
			//SE ESTENSIONE VUOTA procedo se no poi il doc non compare mai!
			//if (String.IsNullOrWhiteSpace(attachInfo.ExtensionType))
			//    return null;

			//cache dei documenti (sarebbe da disabilitare quando un doc viene archiviato sostituito con uno più nuovo)
			if (previewControls.ContainsKey(attachInfo.ArchivedDocId) && previewControls[attachInfo.ArchivedDocId] != null)
				return (Control)previewControls[attachInfo.ArchivedDocId];

			Control controlToAdd = null;

			try
			{
				string extension = /*Path.GetExtension*/(attachInfo.ExtensionType);

				switch (extension.ToLowerInvariant())
				{
					case FileExtensions.DotPdf:
						{
                            if (!attachInfo.IsAFile && !attachInfo.SaveAttachmentFile())
                                break;

							if (gdPDF.LoadFromFile(attachInfo.TempPath, false) == TBPictureStatus.OK)
							{
								int imageId = gdPDF.RenderPageToGdPictureImage(200, false);
								if (imageId != 0)
								{
                                    TBPicViewer GdViewer1 = GetGdViewer();

                                    TBPicImaging oGdPictureImaging = new TBPicImaging();
									int ii = oGdPictureImaging.CreateThumbnail(imageId, GdViewer1.Size.Width, GdViewer1.Size.Height);
									GdViewer1.DisplayFromGdPictureImage(ii);
									controlToAdd = GdViewer1;
                                    //anomalia 20161
                                    oGdPictureImaging.ReleaseGdPictureImage(imageId);
								}
								gdPDF.CloseDocument(); //devo chiamare la close, se no il file rimane aperto (quindi lockato) 
							}

							//ThumbnailEx ex = new ThumbnailEx();
							//ex.AddItemFromFile(attachInfo.TempPath);
							//ex.SetItemText(0, String.Empty); 
							//ex.BackColor = Color.Aquamarine;//??
							//controlToAdd = ex;
							break;
						}

					default:
						{
							PictureBox pb = new PictureBox();
							pb.SizeMode = PictureBoxSizeMode.CenterImage;
							pb.Image = GetThumbnail(attachInfo);
							if (pb.Image == null)
								pb.Image = GetBitmapIconOfFile(attachInfo.ExtensionType);//pesca l'icona generica per quell'estensione.
							controlToAdd = pb;
							break;
						}
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.Fail(e.Message);
			}
			finally
			{
				if (controlToAdd == null)
				{
					PictureBox pb = new PictureBox();
					pb.Image = GetBitmapIconOfFile(attachInfo.ExtensionType);//pesca l'icona generica per quell'estensione.
					controlToAdd = pb;
				}

				attachInfo.DisposeDocContent();
			}

			previewControls.Add(attachInfo.ArchivedDocId, controlToAdd);
			//controlToAdd.MouseDown += new MouseEventHandler(_MouseDown);    
			//controlToAdd.MouseUp += new MouseEventHandler(_MouseUp);
			return controlToAdd;
		}



		//--------------------------------------------------------------------------------
		//void controlToAdd_MouseDown(object sender, MouseEventArgs e)
		//{
		//    drag = true;  //sul mouse down gestisco l'eventualità del drag& drop.
		//    //lo slide show non ha un item_drag, come treeview e list view, 
		//    //quindi in attesa di scoprire nuovi orizzonti gestisco il dodragdrop sul mouse down, 
		//    //questo comporta che ad ogni click viene salvato il file temporaneo... 
		//    //e che devo rimbalzare a mano l'evento al parent perchè se no il dodragdrop se lo mangia 
		//    //Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<controlToAdd_MouseDown"); long l = (DateTime.Now.Ticks);
		//    if (e.Button == MouseButtons.Left && e.Clicks==1 && drag)//così non si mangia il doppio click
		//    {
		//       // if (e.X >= this.Location.X && e.X <= this.Location.X + this.Size.Width && e.Y >= this.Location.Y && e.Y <= this.Location.X + this.Size.Height) return;
		//        if (Utils.SaveAttachmentFile(currentPanel.AttachInfo))
		//        {
		//            //rilancio il click al parent che se no viene mangiato da dodragdrop
		//            panel_MouseClick(((SlideShowPanel)((Control)sender).Parent), e);// così non si perde il click
		//            DoDragDrop();
		//        }
		//        // Debug.WriteLine(DateTime.Now.ToString());
		//        //long ll = (DateTime.Now.Ticks);

		//        // Debug.WriteLine(ll - l); 
		//    } 
		//}

		//--------------------------------------------------------------------------------
		private void _MouseDown(object sender, MouseEventArgs e)
		{
			drag = true;  //sul mouse down gestisco l'eventualità del drag& drop.
		}

		//--------------------------------------------------------------------------------
		private void _MouseUp(object sender, MouseEventArgs e)
		{
			drag = false;
		}

		//------------------------------------------------------------------------------------------------------
		private void DoDragDrop()
		{
			try
			{
				List<string> list = new List<string>();
				foreach (AttachmentInfo att in SelectedDocuments)
				{
                    if (!att.SaveAttachmentFile())
                        continue;

					if (!File.Exists(att.TempPath))
						return;

					//per fare in modo che il drag&drop avvenga senza portarsi dietro il postfisso 
					//che viene inserito ai file archiviati rinomino temporaneamente il file 
					//perchè non trovo altro metodo per  esplicitare un nome di file diverso
					//rallenta un po'...
					string oldname = att.TempPath;
					int ind = oldname.LastIndexOf(Utils.Postfix);
					string newName = oldname;
					if (ind > -1)
						newName = oldname.Substring(0, ind) + Path.GetExtension(oldname);

					FileInfo fi = new FileInfo(oldname);

					if (!File.Exists(newName))
						fi.CopyTo(newName, true);
                    list.Add(newName);
                    FileInfo fiNew = new FileInfo(newName);

                    fiNew.Attributes = fiNew.Attributes & ~FileAttributes.Hidden;
                    fiNew.Attributes = fiNew.Attributes & ~FileAttributes.ReadOnly;
                    fiNew.Attributes = fiNew.Attributes & ~FileAttributes.System;
                    fiNew.Refresh();
				}

                if (list.Count == 0) return;//se ricadi qua si schianta explorer( esempio se selezioni solo papery )
				this.DoDragDrop(new DataObject(DataFormats.FileDrop, list.ToArray()), DragDropEffects.Copy);

				System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(DeleteFiles));
				t.Start(list);
			}
			catch (Exception exc)
			{
				Debug.Write(exc.ToString());
			}
		}

      

		//------------------------------------------------------------------------------------------------------
		private void DeleteFiles(object list)
		{
			List<string> files = list as List<string>;
			if (files == null) return;
			try
			{
				foreach (string s in files)
                    Utils.DeleteFile(new FileInfo(s));
			}
			catch (Exception exc)
			{
				Debug.Write(exc.ToString());
			}
		}

		//------------------------------------------------------------------------------------------------------
		private void AddControl(AttachmentInfo doc)
		{
			if (docViewMode == ViewMode.SlideShow)
			{
				Control controlToAdd = GetControl(doc);

				if (controlToAdd != null)
				{
					SlideShowPanel panel = GetFreePanel();
					if (panel != null)
					{
						panel.AttachInfo = doc;
						panel.AddControl(controlToAdd);
					}
				}
			}
			else //griglia
			{
				int imageIdx = GetImageIndex(doc.IsAPapery ? doc.ExtensionType : Path.GetExtension(doc.TempPath));
				ListViewItem i = new ListViewItem(doc.Name, imageIdx);
                i.Name = (doc.IsAPapery) ? doc.TBarcode.Value : doc.AttachmentId.ToString();
				i.Tag = doc;
                string ttt = doc.Description;
                if (!String.IsNullOrWhiteSpace(ttt) && ttt != doc.Name)
                    i.ToolTipText = ttt;    
				DocListView.Items.Add(i);
			}
		}

		/// <param name="small">ritorna immagine 16x16 oppure 64x64</param>
		//---------------------------------------------------------------------
		public static int GetImageIndex(string extType)
		{
			string extension = extType.Trim(new char[] { '.' });

			switch (extension.ToLowerInvariant())
			{
				case FileExtensions.Pdf:
					return 11;
				case FileExtensions.Bmp:
					return 21;
				case FileExtensions.Doc:
				case FileExtensions.Docx:
					return 19;
                case FileExtensions.Ppt:
                case FileExtensions.Pptx:
                    return 9;
				case FileExtensions.Gif:
					return 18;
				case FileExtensions.Gzip:
					return 17;
				case FileExtensions.Html:
				case FileExtensions.Htm:
					return 16;
				case FileExtensions.Jpg:
				case FileExtensions.Jpeg:
					return 15;
				case FileExtensions.Tif:
				case FileExtensions.Tiff:
					return 6;
				case FileExtensions.Txt:
				case FileExtensions.Config:
					return 5;
				case FileExtensions.Png:
					return 10;
				case FileExtensions.Rar:
					return 8;
				case FileExtensions.Xml:
					return 1;
				case FileExtensions.Xls:
				case FileExtensions.Xlsx:
					return 2;
				case FileExtensions.Zip:
				case FileExtensions.Zip7z:
					return 0;
				case FileExtensions.Wmv:
					return 3;
				case FileExtensions.Mpeg:
					return 12;
				case FileExtensions.Avi:
					return 22;
				case FileExtensions.Wav:
					return 4;
				case FileExtensions.Mp3:
					return 13;
				case FileExtensions.Msg:
					return 14;
				case FileExtensions.Rtf:
					return 7;
				case FileExtensions.Papery:
					return 23;
				default:
					return 20;
			}
		}

		//---------------------------------------------------------------------
		public void Populate()
		{
			LoadAttachment();
			SetSelection(0);

			if (CountChanged != null)
				CountChanged(this, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		public void LoadAttachment()
		{
			bool btnChecked = BtnPaperyDocs.Checked;

			Clear();

			bool noAttachments = (attachments == null || attachments.Count <= 0);
			LblNoAttach.Visible = noAttachments;
			PBNoAttach.Visible = noAttachments;

			if (noAttachments)
			{
				BtnPaperyDocs.Checked = btnChecked;
				BtnPaperyDocs.Enabled = BtnPaperyDocs.Checked;
				return;
			}

			foreach (AttachmentInfo doc in attachments)
				AddControl(doc);

			Prepare();
		}

		//---------------------------------------------------------------------
		public void ClearAllAttachment()
		{
			Clear();
			attachments.Clear();
		}

		//--------------------------------------------------------------------------------
		public void AddNewAttachment(AttachmentInfo doc)
		{
			if (IsDisposed)
				return;

			if (docViewMode == ViewMode.SlideShow)
			{
				//per prima cosa controllo se l'attachment è già presente tra quelli gestiti 
				for (int i = 0; i < panels.Count; i++)
				{
					SlideShowPanel panel = panels[i];

					if (panel.AttachInfo != null)
					{
						if (panel.AttachInfo.IsAPapery)
						{
                            if (panel.AttachInfo.TBarcode.Value == doc.TBarcode.Value)
							{
								UpdateDoc(panel, doc);
								SetSelection(i);
								return;
							}
						}
						else
						{
							if (//panel.AttachInfo.AttachmentId == doc.AttachmentId &&
								panel.AttachInfo.ArchivedDocId == doc.ArchivedDocId)
							{
								UpdateDoc(panel, doc);
								SetSelection(i);
								return;
							}
						}
					}
				}
			}
			else
			{
				DocListView.MultiSelect = false; // tolgo la multiselezione per poter selezione solo il nuovo attachment inserito
				int selIdx = -1;

				//per prima cosa controllo se l'attachment è già presente tra quelli gestiti 
				foreach (ListViewItem i in DocListView.Items)
				{
					if (i.Tag == null)
						continue;

					AttachmentInfo ai = (AttachmentInfo)i.Tag;
					if (ai.IsAPapery)
					{
                        if (ai.TBarcode.Value == doc.TBarcode.Value)
							selIdx = i.Index;
					}
					else
						if (ai.AttachmentId == doc.AttachmentId)
							selIdx = i.Index;

					if (selIdx > -1)
					{
						SetSelection(selIdx);
						DocListView.MultiSelect = true; // ripristino la possibilità di effettuare la multiselezione
						return;
					}
				}
			}

			//se non presente allora lo aggiungo
			attachments.Add(doc);
			AddControl(doc);

			SetSelection(Count - 1);
			Prepare();

			if (docViewMode == ViewMode.Grid)
				DocListView.MultiSelect = true; // ripristino la possibilità di effettuare la multiselezione

			if (CountChanged != null)
				CountChanged(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		public void DeleteCurrentAttachment()
		{
			DeleteSelection();
			Prepare();

			if (CountChanged != null)
				CountChanged(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		TBPicViewer GetGdViewer()
		{
            TBPicViewer GdViewer1 = new TBPicViewer();
			GdViewer1.Size = new System.Drawing.Size(SlideShowPanel.standardWidth, SlideShowPanel.standardHeight);
			GdViewer1.BackColor = BackColor;
			GdViewer1.Location = new Point(1, 1);
			GdViewer1.AutoSize = true;
			GdViewer1.ScrollBars = false;
			GdViewer1.EnableMenu = false;
            GdViewer1.MouseWheelMode = TBPicViewerMouseWheelMode.MouseWheelModeVerticalScroll;
			GdViewer1.ContextMenuStrip = null;
			GdViewer1.Dock = DockStyle.Fill;
            GdViewer1.MouseMode = TBPicViewerMouseMode.MouseModeDefault;
			return GdViewer1;
		}

		//--------------------------------------------------------------------------------
		void GetDocument(object sender)
		{
			if (CurrentDoc != null && CurrentDocChanging != null)
				CurrentDocChanging(this, new EventArgs());

			if (sender == null)
			{
				currentPanel = null;
				CurrentDoc = null;
				return;
			}

			if (docViewMode == ViewMode.SlideShow)
			{
				currentPanel = sender as SlideShowPanel;
				if (currentPanel == null) return;
				CurrentDoc = currentPanel.AttachInfo;
			}
			else
				if (DocListView.SelectedItems != null && DocListView.SelectedItems.Count > 0)
					CurrentDoc = DocListView.SelectedItems[0].Tag as AttachmentInfo;
		}

		//--------------------------------------------------------------------------------
		private void UpdateDoc(SlideShowPanel panel, AttachmentInfo doc, bool checkData = true)
		{
            if (panel == null) return;
			if (panel.AttachInfo.LastWriteTimeUtc != doc.LastWriteTimeUtc || !checkData)
			{
				//invalido il controllo con l'id come il doc perchè è cambiato il file
				previewControls.Remove(doc.ArchivedDocId);
				Control controlToAdd = GetControl(doc);
				panel.AddControl(controlToAdd);
				panel.AttachInfo = doc;
			}
		}

		//--------------------------------------------------------------------------------
		internal void UpdateCurrentDoc()
        {
            if (currentPanel == null) return;
			UpdateDoc(currentPanel, CurrentDoc, false);
			currentPanel.Select();
		}

        //tooltip che mostra la descrizione se presente. devo fare un po' di manovre 
        //per il posizionamento dei controlli uno dentro l'altro se non non viene mostrato.
		//--------------------------------------------------------------------------------
		void panel_MouseHover(object sender, EventArgs e)
		{
			if (sender == null) return;

            SlideShowPanel p = sender as SlideShowPanel;
            if (p == null || p.Controls == null || p.Controls.Count == 0) return;


            Control control = p.Controls[0];
            if (control == null)
                return;

            if (control is TBPicViewer)
                control = control.Controls[0];

            if (control == null)
                return;

            string text = p.AttachInfo.Description;
            
            if (string.IsNullOrWhiteSpace(text) || text == p.AttachInfo.Name ) return;

            toolTip.SetToolTip(control, text);
		}

		//--------------------------------------------------------------------------------
		void panel_MouseClick(object sender, MouseEventArgs e)
		{
			GetDocument(sender);
			ToggleSelection();
			OpenAttachment();
		}

		//--------------------------------------------------------------------------------
		void ToggleSelection()
		{
			foreach (SlideShowPanel p in panels)
				p.ToggleSelection(p == currentPanel);
			//se il doc selezionato è fuori dalla vista anche parzialmente allora scrollo per renderlo visibile.
			EnsureVisible();
			LblDocName.Text = CurrentDoc.Name;
		}

		//--------------------------------------------------------------------------------
		internal void EnsureVisible()
		{
			if (currentPanel == null)
				return;

			//se il doc selezionato è fuori dalla vista anche parzialmente allora scrollo per renderlo visibile.
			if (currentPanel.Location.X + currentPanel.Size.Width > PanelSlideShow.Size.Width)//esce a destra
				PanelSlideShow.AutoScrollPosition = new Point(
					(currentPanel.Location.X + currentPanel.Size.Width - PanelSlideShow.AutoScrollPosition.X) - PanelSlideShow.Size.Width, PanelSlideShow.AutoScrollPosition.Y
					);

			if (currentPanel.Location.X < 0)//esce a sinistra
				PanelSlideShow.AutoScrollPosition = new Point(currentPanel.Location.X - PanelSlideShow.AutoScrollPosition.X, PanelSlideShow.AutoScrollPosition.Y);
		}


		//---------------------------------------------------------------------
		void DocListView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			GetDocument(sender);
			DocumentClicked();
		}

		//--------------------------------------------------------------------------------
		void panel_DoubleClick(object sender, EventArgs e)
		{
			GetDocument(sender);
			DocumentClicked();
		}

		//--------------------------------------------------------------------------------
		void OpenAttachment()
		{
			if (OpenAttach != null)
				OpenAttach(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		void DeleteAttachment()
		{
			if (DeleteAttach != null)
				DeleteAttach(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		void DocumentClicked()
		{
			if (OpenDocument != null)
				OpenDocument(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		private SlideShowPanel GetFreePanel()
		{
			SlideShowPanel p = new SlideShowPanel();
			PanelSlideShow.Controls.Add(p);

			//devo posizionare il pannello verificando se il controllo è scrollato perchè viene considerata la location nel rettangolo visibile.
			if (PanelSlideShow.AutoScrollPosition.X != 0)
				p.Location = new Point(4 + panelBusyCount + PanelSlideShow.AutoScrollPosition.X, 9);
			else
				p.Location = new Point(4 + panelBusyCount, 9);
			panelBusyCount += 77;

			p.DoubleClick += new EventHandler(panel_DoubleClick);
			//p.MouseClick += new MouseEventHandler(panel_MouseClick);
			p.Click += new EventHandler(panel_Click);
			p.MouseHover += new EventHandler(panel_MouseHover);
			p.BackColor = BackColor;
			p.Index = panels.Count;
			panels.Add(p);
			return p;
		}

		//--------------------------------------------------------------------------------
		void panel_Click(object sender, EventArgs e)
		{
			GetDocument(sender);
			ToggleSelection();
			OpenAttachment();
		}

		// METODI DI APPOGGIO
		// crea la thumbnail dell'immagine data, mantenendo la ratio e impostando uno sfondo bianco 
		// anzichè nero se lo sfondo dell'icona è trasparente.
		//--------------------------------------------------------------------------------
		public Image GetThumbnailFromImage(Image image, int width, int height)
		{
			Bitmap bmpOut = null;
			try
			{
				Bitmap bmp = new Bitmap(image);
				ImageFormat loFormat = bmp.RawFormat;
				decimal ratio;
				int newWidth = 0;
				int newHeight = 0;

				// If the image is smaller than a thumbnail just return it
				if (bmp.Width < width && bmp.Height < height)
					return bmp;

				if (bmp.Width > bmp.Height)
				{
					ratio = (decimal)width / bmp.Width;
					newWidth = width;
					decimal lnTemp = bmp.Height * ratio;
					newHeight = (int)lnTemp;
				}
				else
				{
					ratio = (decimal)height / bmp.Height;
					newHeight = height;
					decimal lnTemp = bmp.Width * ratio;
					newWidth = (int)lnTemp;
				}

				// System.Drawing.Image imgOut = 
				//      loBMP.GetThumbnailImage(lnNewWidth,lnNewHeight,
				//                              null,IntPtr.Zero);

				// This code creates cleaner (though bigger) thumbnails and properly
				// and handles GIF files better by generating a white background for
				// transparent images (as opposed to black)

				bmpOut = new Bitmap(newWidth, newHeight);
				Graphics g = Graphics.FromImage(bmpOut);
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
				g.DrawImage(bmp, 0, 0, newWidth, newHeight);

				bmp.Dispose();
			}
			catch
			{
				return null;
			}
			return bmpOut;
		}

		//--------------------------------------------------------------------------------
		private Image GetThumbnail(AttachmentInfo attachInfo)
		{
			if (attachInfo == null)
				return null;

			Image myImage = null;

			try
			{
				if (attachInfo.VeryLargeFile || attachInfo.IsAFile)
					myImage = Image.FromFile(attachInfo.TempPath);
				else
					if (attachInfo.DocContent != null && attachInfo.DocContent.Length > 0)
					{
						using (MemoryStream msImage = new MemoryStream(attachInfo.DocContent, 0, attachInfo.DocContent.Length))
						{
							msImage.Write(attachInfo.DocContent, 0, attachInfo.DocContent.Length);

							if (CoreUtils.HasImageFormat(attachInfo.DocContent))
								myImage = Image.FromStream(msImage, true);
						}
					}

				if (myImage != null)
					return GetThumbnailFromImage(myImage, SlideShowPanel.standardWidth, SlideShowPanel.standardHeight);
			}
			catch (ArgumentException argEx)
			{
				Debug.WriteLine("ArgumentException getting thumbnail of " + attachInfo.TempPath + Environment.NewLine + argEx.ToString());
				return null;
			}
			catch (Exception exc)
			{
				Debug.WriteLine("Exception getting thumbnail of " + attachInfo.TempPath + Environment.NewLine + exc.ToString());
				return null;
			}

			return null;
		}

		//---------------------------------------------------------------------
		private Bitmap GetBitmapIconOfFile(string extType)
		{
			if (String.IsNullOrWhiteSpace(extType))
				return Microarea.EasyAttachment.Properties.Resources.Ext_Default64x64;
			return Utils.GetMediumImage(extType);
		}

		//---------------------------------------------------------------------
		private void openAttachmentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenAttachment();
		}

		//---------------------------------------------------------------------
		private void openArchivedDocumentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DocumentClicked();
		}

		//--------------------------------------------------------------------------------
		private void BtnRemoveAttach_Click(object sender, EventArgs e)
		{
			DeleteAttachment();
		}

		//funzioni per scorrere le immagini dello slide show, cambiando l'attachement selezionato
		//--------------------------------------------------------------------------------
		private void BtnRight_Click(object sender, EventArgs e)
		{
			SetNextSelection();
		}

		//--------------------------------------------------------------------------------
		private void BtnLeft_Click(object sender, EventArgs e)
		{
			SetPrevSelection();
		}

		//--------------------------------------------------------------------------------
		private void BtnRefresh_Click(object sender, EventArgs e)
		{
			if (RefreshAttachments != null)
				RefreshAttachments(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		private void ChangeViewMode()
		{
			if (docViewMode == ViewMode.Grid)
			{
				DocListView.Visible = true;
				DocListView.BringToFront();
				LblDocName.SendToBack();
				BtnSwitcView.Image = Resources.slideshow;
			}
			else
			{
				BtnSwitcView.Image = Resources.gridIcon;
				DocListView.Visible = false;
				LblDocName.BringToFront();
			}
		}

		//--------------------------------------------------------------------------------
		private void BtnSwitcView_Click(object sender, EventArgs e)
		{
			if (docViewMode == ViewMode.SlideShow)
				docViewMode = ViewMode.Grid;
			else
				docViewMode = ViewMode.SlideShow;

			ChangeViewMode();

			if (CurrentDoc == null)
			{
				ClearControls();
				return;
			}

			int oldcurrdocid = CurrentDoc.AttachmentId;
            string oldBarcode = CurrentDoc.TBarcode.Value;

			LoadAttachment();

			if (docViewMode == ViewMode.SlideShow)
			{
				foreach (SlideShowPanel p in panels)
				{
					if (p.AttachInfo.AttachmentId == oldcurrdocid)
					{
						SetSelection(p.Index);
						break;
					}
				}
			}
			else
			{
				DocListView.MultiSelect = false; // tolgo la multiselezione
				int selIdx = -1;
				foreach (ListViewItem i in DocListView.Items)
				{
					AttachmentInfo ai = (AttachmentInfo)i.Tag;
					if (ai.IsAPapery)
					{
                        if (ai.TBarcode.Value == oldBarcode)
							selIdx = i.Index;
					}
					else
						if (ai.AttachmentId == oldcurrdocid)
							selIdx = i.Index;

					if (selIdx > -1)
					{
						SetSelection(selIdx);
						DocListView.MultiSelect = true; // ripristino la possibilità di effettuare la multiselezione
						break;
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void SetSelection(int index)
		{
			if (docViewMode == ViewMode.SlideShow)
			{
				//Seleziono pannello
				if (panels != null && panels.Count > 0 && index >= 0 && index <= panels.Count - 1)
					panel_MouseClick(panels[index], null);
			}
			else
			{
				if (index >= 0 && index <= DocListView.Items.Count - 1)
				{
					DocListView.Items[index].Selected = true;
					DocListView.EnsureVisible(index);
					DocListView.Select();
					GetDocument(DocListView.SelectedItems);
					OpenAttachment();
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void SetNextSelection()
		{
			if (docViewMode == ViewMode.SlideShow)
			{
				if (panels != null && panels.Count > 0)
				{
					int newIndex = currentPanel.Index + 1;
					if (newIndex < panels.Count)
						panel_MouseClick(panels[newIndex], null);
				}
			}
			else
			{
				if (DocListView.SelectedItems != null && DocListView.SelectedItems.Count > 0)
				{
					int o = DocListView.SelectedItems[0].Index;
					DocListView.MultiSelect = false;
					SetSelection(++o);
					DocListView.MultiSelect = true;
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void SetPrevSelection()
		{
			if (docViewMode == ViewMode.SlideShow)
			{
				if (panels != null && panels.Count > 0)
				{
					int newIndex = currentPanel.Index - 1;
					if (newIndex >= 0)
						panel_MouseClick(panels[newIndex], null);
				}
			}
			else
			{
				if (DocListView.SelectedItems != null && DocListView.SelectedItems.Count > 0)
				{
					int o = DocListView.SelectedItems[0].Index;
					DocListView.MultiSelect = false;
					SetSelection(--o);
					DocListView.MultiSelect = true;
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void DeleteSelection()
		{
			if (CurrentDoc != null)
				attachments.Remove(CurrentDoc);

			if (docViewMode == ViewMode.SlideShow)
			{
				if (panels == null || panels.Count == 0)
					return;

				int currIndex = currentPanel.Index;
				PanelSlideShow.Controls.Remove(currentPanel);

				for (int i = currIndex + 1; i < panels.Count; i++)
				{
					SlideShowPanel p = panels[i];
					p.Location = new Point(p.Location.X - 81, 9);
					p.Index = i - 1;
				}
				this.panels.Remove(currentPanel);

				if (panels.Count > 0)
				{
					if (currIndex > 0)
						panel_MouseClick(panels[currIndex - 1], null);
					else
						panel_MouseClick(panels[0], null);
				}

				if (panelBusyCount > 0)
					panelBusyCount -= 77;
			}
			else
			{
                ListViewItem item = DocListView.Items[(CurrentDoc.IsAPapery) ? CurrentDoc.TBarcode.Value : CurrentDoc.AttachmentId.ToString()];
				if (item != null)
				{
					int o = item.Index;

					DocListView.Items.Remove(item);

					if (o >= DocListView.Items.Count)
						o = DocListView.Items.Count - 1;
					SetSelection(o);
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void DocListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (DocListView.SelectedItems.Count > 1)
			{
				//se l'utente sta passando dalla selezione singola a quella multipla
				if (UserSelectionType == SelectionType.Single)
				{
					BtnRight.Enabled = false;
					BtnLeft.Enabled = false;
					BtnPaperyDocs.Enabled = false;
					UserSelectionType = SelectionType.Multiple;
				}
				return;
			}
			else
				//se l'utente sta passando dalla selezione multipla a quella singola
				if (UserSelectionType == SelectionType.Multiple)
				{
					BtnRight.Enabled = true;
					BtnLeft.Enabled = true;
					BtnPaperyDocs.Enabled = true;
					UserSelectionType = SelectionType.Single;
				}

			GetDocument(sender);
			OpenAttachment();
		}

		///<summary>
		/// Sparo un evento se clicco sul pulsante per filtrare o meno i documenti nello slideshow 
		/// caricando solo i pending papery
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnPaperyDocs_Click(object sender, EventArgs e)
		{
			if (ShowPaperyDocs != null)
				ShowPaperyDocs(sender, EventArgs.Empty);
		}

        private void _MouseHover(object sender, EventArgs e)
        {

        }

	}
}