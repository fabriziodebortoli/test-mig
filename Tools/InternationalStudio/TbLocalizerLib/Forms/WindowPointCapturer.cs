using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for WindowPointCapturer.
	/// </summary>
	//================================================================================
	public class WindowPointCapturer : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnFind;
		private Point hitPoint = Point.Empty;
		private Point centerPoint = Point.Empty;
		private System.Windows.Forms.TextBox txtCenter;
		private DictionaryCreator dictionaryCreator;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//--------------------------------------------------------------------------------
		public Point HitPoint { get { return hitPoint; } }

		//--------------------------------------------------------------------------------
		public WindowPointCapturer(DictionaryCreator dictionaryCreator)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.TransparencyKey = Color.Red;
			this.dictionaryCreator = dictionaryCreator;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		//--------------------------------------------------------------------------------
		private void SetCurrentRow(Translator t, string s)
		{
			FindAndReplaceInfos infos = new FindAndReplaceInfos();
			infos.TargetString = s;
			infos.BaseString = null;
									
			if (!t.SetSelectedRow(infos))
			{
				infos.TargetString = null;
				infos.BaseString = s;
				t.SetSelectedRow(infos);
			}
		}

		//--------------------------------------------------------------------------------
		public void OpenDictionaryTranslator(bool isTbLoader, LocalizerConnector.FormInfo info)
		{
			Microarea.TaskBuilderNet.Core.Generic.NameSpace ns = null;
			
			if (info.Namespace.Length != 0)
				ns = new Microarea.TaskBuilderNet.Core.Generic.NameSpace(info.Namespace);
			
			foreach (DictionaryTreeNode n in dictionaryCreator.GetDictionaryNodes())
			{
				if (string.Compare(n.Name, info.Culture, true) != 0)
					continue;
				
				if (!isTbLoader)
				{
					ArrayList childs = n.GetTypedChildNodes(NodeType.LASTCHILD, true, info.ID.ToString(), true);
					if (childs.Count == 1)
					{
						Translator t = dictionaryCreator.ShowTranslatorOnNode(childs[0] as LocalizerTreeNode);
						if (t != null)
						{
							SetCurrentRow(t, info.Text);
							return;
						}
					}
				}
				else if (ns == null) //Dialog
				{
					ResourceIndexDocument indexDoc = new ResourceIndexDocument(CommonFunctions.GetCorrespondingBaseLanguagePath(n.FileSystemPath));
					if (!indexDoc.LoadIndex())
						continue;

					string name = indexDoc.GetResourceNameById(AllStrings.dialog, info.ID.ToString());
					if (!string.IsNullOrEmpty(name))
					{
						ArrayList childs = n.GetTypedChildNodes(NodeType.LASTCHILD, true, name, true);
						foreach(DictionaryTreeNode child in childs)
						{
							string s = String.Concat(CommonFunctions.GetApplicationName(child), '.', CommonFunctions.GetModuleName(child), '.');
							foreach (string path in info.DictionaryPaths)
							{
								if (path.StartsWith(s, StringComparison.InvariantCultureIgnoreCase))
								{
									Translator t = dictionaryCreator.ShowTranslatorOnNode(childs[0] as LocalizerTreeNode);
									if (t != null)
									{
										SetCurrentRow(t, info.Text);
										return;
									}
								}
							}
						}
					}
				}
				else	//report
				{
					LocalizerTreeNode prjNode = Functions.GetTypedParentNode(n, NodeType.PROJECT);
					if (prjNode == null) continue;
					if (string.Compare(prjNode.Name, ns.Module, true) != 0) continue;

					string reportFileName = Path.GetFileNameWithoutExtension(ns.Report);

					ArrayList nodes = n.GetTypedChildNodes(NodeType.RESOURCE, true);
					foreach (LocalizerTreeNode child in nodes)
					{
						if (child.Name != AllStrings.report) continue;
						foreach (LocalizerTreeNode reportNode in child.Nodes)
						{
							if (string.Compare
								(
								reportNode.Name,
								reportFileName,
								true
								) != 0
								)
								continue;
								
							foreach (DictionaryTreeNode reportChildNode in reportNode.Nodes)
								dictionaryCreator.ShowTranslatorOnNode(reportChildNode);
								
							return;
						}
					}
				}
			}

			MessageBox.Show(this, Strings.ResourceNotFound);
		}

		//--------------------------------------------------------------------------------
		private bool FindFromOtherProcess(Process proc, IntPtr hwnd)
		{
			LocalizerConnector.FormInfo fi; 
			if (!LocalizerConnector.GetFormInfo(proc, hwnd, out fi))
				return false;
			OpenDictionaryTranslator (false, fi);

			return true;
		}

		//--------------------------------------------------------------------------------
		private bool FindFromTbLoader(Process proc, IntPtr windowHandle)
		{			
			if (HitPoint == Point.Empty)
				return false;		
			
			try
			{
				LocalizerConnector.FormInfo fi = LocalizerConnector.GetWindowInfosFromPoint(proc, windowHandle, HitPoint);
				if (fi == null)
					return false;
				OpenDictionaryTranslator(true, fi);
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message);
				return false;
			}

			
		}
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WindowPointCapturer));
			this.btnFind = new System.Windows.Forms.Button();
			this.txtCenter = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnFind
			// 
			this.btnFind.AccessibleDescription = resources.GetString("btnFind.AccessibleDescription");
			this.btnFind.AccessibleName = resources.GetString("btnFind.AccessibleName");
			this.btnFind.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnFind.Anchor")));
			this.btnFind.BackColor = System.Drawing.SystemColors.Control;
			this.btnFind.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFind.BackgroundImage")));
			this.btnFind.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnFind.Dock")));
			this.btnFind.Enabled = ((bool)(resources.GetObject("btnFind.Enabled")));
			this.btnFind.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnFind.FlatStyle")));
			this.btnFind.Font = ((System.Drawing.Font)(resources.GetObject("btnFind.Font")));
			this.btnFind.Image = ((System.Drawing.Image)(resources.GetObject("btnFind.Image")));
			this.btnFind.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnFind.ImageAlign")));
			this.btnFind.ImageIndex = ((int)(resources.GetObject("btnFind.ImageIndex")));
			this.btnFind.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnFind.ImeMode")));
			this.btnFind.Location = ((System.Drawing.Point)(resources.GetObject("btnFind.Location")));
			this.btnFind.Name = "btnFind";
			this.btnFind.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnFind.RightToLeft")));
			this.btnFind.Size = ((System.Drawing.Size)(resources.GetObject("btnFind.Size")));
			this.btnFind.TabIndex = ((int)(resources.GetObject("btnFind.TabIndex")));
			this.btnFind.Text = resources.GetString("btnFind.Text");
			this.btnFind.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnFind.TextAlign")));
			this.btnFind.Visible = ((bool)(resources.GetObject("btnFind.Visible")));
			this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
			// 
			// txtCenter
			// 
			this.txtCenter.AccessibleDescription = resources.GetString("txtCenter.AccessibleDescription");
			this.txtCenter.AccessibleName = resources.GetString("txtCenter.AccessibleName");
			this.txtCenter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtCenter.Anchor")));
			this.txtCenter.AutoSize = ((bool)(resources.GetObject("txtCenter.AutoSize")));
			this.txtCenter.BackColor = System.Drawing.Color.Red;
			this.txtCenter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtCenter.BackgroundImage")));
			this.txtCenter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtCenter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtCenter.Dock")));
			this.txtCenter.Enabled = ((bool)(resources.GetObject("txtCenter.Enabled")));
			this.txtCenter.Font = ((System.Drawing.Font)(resources.GetObject("txtCenter.Font")));
			this.txtCenter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtCenter.ImeMode")));
			this.txtCenter.Location = ((System.Drawing.Point)(resources.GetObject("txtCenter.Location")));
			this.txtCenter.MaxLength = ((int)(resources.GetObject("txtCenter.MaxLength")));
			this.txtCenter.Multiline = ((bool)(resources.GetObject("txtCenter.Multiline")));
			this.txtCenter.Name = "txtCenter";
			this.txtCenter.PasswordChar = ((char)(resources.GetObject("txtCenter.PasswordChar")));
			this.txtCenter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtCenter.RightToLeft")));
			this.txtCenter.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtCenter.ScrollBars")));
			this.txtCenter.Size = ((System.Drawing.Size)(resources.GetObject("txtCenter.Size")));
			this.txtCenter.TabIndex = ((int)(resources.GetObject("txtCenter.TabIndex")));
			this.txtCenter.Text = resources.GetString("txtCenter.Text");
			this.txtCenter.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtCenter.TextAlign")));
			this.txtCenter.Visible = ((bool)(resources.GetObject("txtCenter.Visible")));
			this.txtCenter.WordWrap = ((bool)(resources.GetObject("txtCenter.WordWrap")));
			// 
			// WindowPointCapturer
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.Color.Red;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.txtCenter);
			this.Controls.Add(this.btnFind);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "WindowPointCapturer";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.TopMost = true;
			this.Load += new System.EventHandler(this.WindowPointCapturer_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.WindowPointCapturer_Paint);
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void WindowPointCapturer_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Pen p = new Pen(Color.Blue);
			RectangleF r = e.Graphics.VisibleClipBounds;
			r.Height = r.Height - btnFind.Height;
			e.Graphics.DrawEllipse(p, r.Left + 20, r.Top + 20, r.Width - 40, r.Height - 40);
			e.Graphics.DrawLine(p, 20, r.Height / 2, r.Width - 20, r.Height / 2);
			e.Graphics.DrawLine(p, r.Width / 2, 20, r.Width / 2, r.Height - 20);

			centerPoint = new Point((int)r.Width / 2, (int)r.Height / 2);
			
		}

		//--------------------------------------------------------------------------------
		private void btnFind_Click(object sender, System.EventArgs e)
		{
			hitPoint = PointToScreen(centerPoint);
			Visible = false;
			try
			{
				Process proc;
				IntPtr hwnd;
				if (!CommonFunctions.GetWindowInfoFromPoint(hitPoint, out proc, out hwnd))
				{
					MessageBox.Show(this, Strings.ProcessNotFound);
					return;
				}

				if (FindFromTbLoader(proc, hwnd))
					return;
				if (FindFromOtherProcess(proc, hwnd))
					return;

				MessageBox.Show(this, Strings.ProcessNotFound);
			}
			finally
			{
				Visible = true;
			}
		}
			
		//--------------------------------------------------------------------------------
		private void WindowPointCapturer_Load(object sender, System.EventArgs e)
		{
			Rectangle r = ClientRectangle;
			r.Height = r.Height - btnFind.Height;
			Point p = new Point(r.Width / 2 - txtCenter.Width / 2, r.Height / 2 - txtCenter.Height / 2);
			txtCenter.Location = p;
		
		}
	
	}
}
