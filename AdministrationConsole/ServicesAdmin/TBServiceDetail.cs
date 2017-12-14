using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.Console.Plugin.ServicesAdmin
{
	/// </summary>
	//=========================================================================
	public partial class TBServiceDetail : System.Windows.Forms.Form
	{
		private Diagnostic diagnostic = new Diagnostic("TBServiceDetail");
		private TbServices tbServices;

		// evento per chiedere alla Console l'authentication token
		public delegate string GetAuthenticationToken();
		public event GetAuthenticationToken OnGetAuthenticationToken;


		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public TBServiceDetail(TbServices tbServices)
		{
			InitializeComponent();
			this.tbServices = tbServices;
		}

		/// <summary>
		/// TBServiceDetail_Load
		/// </summary>
		//---------------------------------------------------------------------
		private void TBServiceDetail_Load(object sender, System.EventArgs e)
		{
			RefreshTree();
		}

		//---------------------------------------------------------------------
		private void RefreshTree()
		{
			Cursor.Current = Cursors.WaitCursor;
			FillProcesses();
			Cursor.Current = Cursors.Default;
		}

		//---------------------------------------------------------------------
		private void FillProcesses()
		{
			TwThreads.Nodes.Clear();

			string xml = GetTbLoaderInstantiatedList();//@"<Processes><Process id='1234'><Threads><Thread name='sa - Company30DbXml' id='5076' loginthreadname='sa - Company30DbXml' loginthreadid='5076' documentthread='false' modalstate='true'><Threads><Thread name='Document.erp.company.documents.Company' id='7084' loginthreadname='sa - Company30DbXml' loginthreadid='5076' documentthread='true' modalstate='false'><Threads></Threads></Thread><Thread name='Document.erp.company.documents.Company' id='7820' loginthreadname='sa - Company30DbXml' loginthreadid='5076' documentthread='true' modalstate='false'><Threads></Threads></Thread></Threads></Thread></Threads></Process></Processes>";
			if (String.IsNullOrEmpty(xml))
			{
				TwThreads.Nodes.Add(new EmptyTreenode(Strings.NoProcesses, 0, 0));
				return;
			}
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.LoadXml(xml);
			}
			catch 
			{
				TwThreads.Nodes.Add(new EmptyTreenode(Strings.InvalidData, 0, 0));
				return;

			}
			XmlNodeList procList = xmlDoc.SelectNodes("Processes/Process");

			if (procList == null || procList.Count == 0)
			{
				TwThreads.Nodes.Add(new EmptyTreenode(Strings.NoProcesses, 0, 0));
				return;
			}

			foreach (XmlElement proc in procList)
			{
				string procID = proc.GetAttribute("id");
				TreeNode procNode = new TreeNode(Strings.TbLoaderProcess + " (" + procID + ")", 0, 0);
				procNode.Tag = new ThreadNodeTag(procID, procID); ;
				procNode.ToolTipText = String.Format(Strings.ThreadId, procID);
				TwThreads.Nodes.Add(procNode);

				XmlNodeList loginListGroup = proc.ChildNodes;
				if (loginListGroup == null || loginListGroup.Count==0)//nodo Threads
					continue;
				XmlNodeList loginList = loginListGroup[0].ChildNodes;

				foreach (XmlElement login in loginList)
				{
					TreeNode loginNode = new TreeNode(login.GetAttribute("name"), 1, 1);
					string loginID = login.GetAttribute("id");
					loginNode.Tag = new ThreadNodeTag(loginID, procID);
					bool loginModal = (String.Compare(bool.TrueString, login.GetAttribute("modalstate"), true, CultureInfo.InvariantCulture) == 0);
					loginNode.ToolTipText = String.Format(Strings.ThreadId, loginID) + (loginModal ? (Environment.NewLine + Strings.Modal) : String.Empty); ;
					loginNode.ContextMenuStrip = ThreadContextMenuStrip;
					procNode.Nodes.Add(loginNode);

					XmlNodeList docListGroup = login.ChildNodes;
					if (docListGroup == null || docListGroup.Count == 0) //nodo Threads
						continue;
					XmlNodeList docList = docListGroup[0].ChildNodes;
					
					foreach (XmlElement doc in docList)
					{
						TreeNode docNode = new TreeNode(doc.GetAttribute("name"), 2, 2);
						string docID = doc.GetAttribute("id");
						//se è "true" vale true in tutti gli altri casi false
						bool docModal = (String.Compare(bool.TrueString, doc.GetAttribute("modalstate"), true, CultureInfo.InvariantCulture) == 0);
						docNode.Tag = new ThreadNodeTag(docID, procID);
						docNode.ToolTipText = String.Format(Strings.ThreadId, docID) + (docModal ? (Environment.NewLine + Strings.Modal) : String.Empty);
						docNode.ContextMenuStrip = ThreadContextMenuStrip;
						loginNode.Nodes.Add(docNode);
					}
				}
			}
			TwThreads.ExpandAll();
		}

		/// <summary>
		/// Fornisce una stringa in formato xml che contiene informazioni legate ai TbLoader 
		/// istanziati da Login Manager per EasyLook. Utilizza Web Services Attachment
		/// </summary>
		/// <returns>Una string xml</returns>
		//-----------------------------------------------------------------------
		public string GetTbLoaderInstantiatedList()
		{
			try
			{
				return tbServices.GetTbLoaderInstantiatedListXML(GetAuthToken());
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				return string.Empty;
			}
		}

		//---------------------------------------------------------------------
		private string GetAuthToken()
		{
			string token = string.Empty;
			if (OnGetAuthenticationToken != null)
				token = OnGetAuthenticationToken();

			if (string.IsNullOrEmpty(token))
				throw new Exception(Strings.AuthenticationTokenNotValid);
			return token;

		}

		//---------------------------------------------------------------------
		private int GetCurrentID()
		{
			int id = -1;
			if (TwThreads.SelectedNode == null) 
				return id;
			return ((ThreadNodeTag)TwThreads.SelectedNode.Tag).ThreadID;
		}

		//---------------------------------------------------------------------
		private int GetCurrentProcessID()
		{
			int id = -1;
			if (TwThreads.SelectedNode == null)
				return id;
			return ((ThreadNodeTag)TwThreads.SelectedNode.Tag).ProcessID;
		}

		//---------------------------------------------------------------------
		private void CloseStripMenuItem_Click(object sender, EventArgs e)
		{
			bool stopped = true;
			int procID = GetCurrentProcessID();
			int threadID = GetCurrentID();
			bool isProcess = procID == threadID;
			try
			{
				if (isProcess)
					stopped = tbServices.StopProcess(procID, GetAuthToken());
				else
					stopped = tbServices.StopThread(threadID, procID, GetAuthToken());
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				MessageBox.Show(this, exc.Message, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error); ;
				return;
			}

			if (!stopped)
			{

				string val = isProcess ? Strings.Process : Strings.Thread;
				DialogResult res = MessageBox.Show(this, String.Format(Strings.KillUnresponsive, val ,threadID), Strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
				if (res == DialogResult.Yes)
				{
					if (procID == threadID)//è un processo
						tbServices.KillProcess(procID, GetAuthToken());
					else
					tbServices.KillThread(threadID, procID, GetAuthToken());
				}
			}
			RefreshTree();
		}

		//---------------------------------------------------------------------
		private void Tree_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right) return;

			TreeView localTree = sender as TreeView;

			if (localTree == null || localTree.Nodes == null || localTree.Nodes.Count == 0)
				return;

			localTree.SelectedNode = localTree.GetNodeAt(e.X, e.Y);

			if (localTree.SelectedNode != null && !(localTree.SelectedNode is EmptyTreenode))
				localTree.SelectedNode.ContextMenuStrip = ThreadContextMenuStrip;
		}

		//---------------------------------------------------------------------
		private void refreshAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RefreshTree();
		}
	}

	//=========================================================================
	internal class EmptyTreenode : TreeNode
	{
		public EmptyTreenode(string text, int imageIndex, int SelectedImageIndex)
			: base(text, imageIndex, SelectedImageIndex)
		{}
	}

	//=========================================================================
	internal struct ThreadNodeTag 
	{
		public int ThreadID;
		public int ProcessID;


		//---------------------------------------------------------------------
		public ThreadNodeTag(string threadID, string processID)
		{
			Int32.TryParse(threadID, out ThreadID);
			Int32.TryParse(processID, out ProcessID);
		}
	}
}