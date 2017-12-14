using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.MenuManager
{
	//================================================================================
	[Serializable]
	public class DocumentStates
	{
		private Dictionary<string, DocumentState> states = null;
		private List<string> openDocs = null;
		private string file;

		//--------------------------------------------------------------------------------
		public bool RestoreOpenDocuments { get; set; }
		//--------------------------------------------------------------------------------
		public int OpenDocumentCount { get { return openDocs.Count; } }

		//---------------------------------------------------------------------------
		private DocumentStates(string file)
		{
			this.file = file;
		}

		//--------------------------------------------------------------------------------
		public DocumentState Get(string docNamespace)
		{
			DocumentState state;
			if (!states.TryGetValue(docNamespace, out state))
			{
				state = new DocumentState();
				state.DockState = DockState.Document;
				states[docNamespace] = state;
			}
			return state;
		}

		//--------------------------------------------------------------------------------
		public void AddNamespace(string docNamespace)
		{
			openDocs.Add(docNamespace);
		}
		//--------------------------------------------------------------------------------
		public void RemoveNamespace(string docNamespace)
		{
			openDocs.Remove(docNamespace);
		}
		//--------------------------------------------------------------------------------
		public static DocumentStates Load(string file)
		{
			DocumentStates states = new DocumentStates(file);
			if (File.Exists(file))
			{
				using (FileStream fs = new FileStream(file, FileMode.Open))
				{
					try
					{
						BinaryFormatter bf = new BinaryFormatter();
						states.RestoreOpenDocuments = (bool) bf.Deserialize(fs);
						states.states = bf.Deserialize(fs) as Dictionary<string, DocumentState>;
						states.openDocs = bf.Deserialize(fs) as List<string>;
						return states;
					}
					catch 
					{
					}
				}
			}
			states.states = new Dictionary<string, DocumentState>();
			states.openDocs = new List<string>();
			states.RestoreOpenDocuments = true;
			return states;
		}

		//--------------------------------------------------------------------------------
		public void Save()
		{
			try
			{
				string folder = Path.GetDirectoryName(file);
				if (!Directory.Exists(folder))
					Directory.CreateDirectory(folder);
				using (FileStream fs = new FileStream(file, FileMode.Create))
				{
					BinaryFormatter bf = new BinaryFormatter();
					bf.Serialize(fs, this.RestoreOpenDocuments);
					bf.Serialize(fs, this.states);
					bf.Serialize(fs, this.openDocs);
				}
			}
			catch
			{
			}
		}

		//--------------------------------------------------------------------------------
		internal List<string> GetListAndClear()
		{
			//clono la lista
			List<string> openDocs = new List<string>(this.openDocs);
			//pulisco la lista originaria
			this.openDocs.Clear();
			return openDocs;
		}

		
	}

	//================================================================================
	[Serializable]
	public class DocumentState
	{
		//--------------------------------------------------------------------------------
		public DockState DockState { get; set; }
		//--------------------------------------------------------------------------------
		public Rectangle Rectangle { get; set; }
		public FormWindowState WindowState { get; set; }
	}
}
