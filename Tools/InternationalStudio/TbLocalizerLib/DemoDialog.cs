using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// DemoDialog.
	/// </summary>
	//=========================================================================
	public class DemoDialog
	{
		public enum		Type { Dialog, Woorm, WoormAskDialog };
		private ArrayList	resourcePaths;
		private uint		IDD;
		private Form		parent;
		private Type		demoType;
		private	HtmlDialog	htmlDialog;
		private XmlDocument xmlDocument;
		private string		dialogName;
		private bool		valid = true;
		private string		woormFile;
		private static bool	stringLoaderLoaded = false;

		//--------------------------------------------------------------------------------
		[DllImport("Kernel32.dll", EntryPoint="LoadLibraryW", CharSet=CharSet.Unicode)]
		private static extern IntPtr LoadLibrary(string path);

		//--------------------------------------------------------------------------------
		[DllImport("TBStringLoader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool ExistDialog(string modulePath, uint IDD);
		
		//--------------------------------------------------------------------------------
		[DllImport("TBStringLoader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool ShowDemoDialog(IntPtr hParent, string xml, string modulePath, uint IDD);
 
		//--------------------------------------------------------------------------------
		[DllImport("TBStringLoader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool CloseDemoDialog(uint IDD);
 
		//--------------------------------------------------------------------------------
		[DllImport("TBStringLoader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SetFormFont();
 
		//--------------------------------------------------------------------------------
		[DllImport("TBStringLoader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern string CheckDialog(string modulePath, uint IDD, float ratio, string dictionaryPath);
 
		//--------------------------------------------------------------------------------
		[DllImport("TBStringLoader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FreeCache();
		
		//--------------------------------------------------------------------------------
		public string WoormFile { get { return woormFile; } set { woormFile = value; } } 

		//--------------------------------------------------------------------------------
		public DemoDialog(string[] modulePaths, uint IDD, Form parentWindow)
		{
			resourcePaths = new ArrayList();
			resourcePaths.AddRange(modulePaths);
			
			this.IDD		= IDD;
			this.parent		= parentWindow;
			this.demoType	= Type.Dialog;

			LoadLibraries();
		}

		//---------------------------------------------------------------------
		public DemoDialog()
		{
			this.demoType	= Type.Dialog;

			LoadLibraries();
		}

		//---------------------------------------------------------------------
		public DemoDialog(string filePath, Type demoType, Form parentWindow)
		{
			resourcePaths = new ArrayList ();
			resourcePaths.Add(filePath);
			
			this.IDD		= 0;
			this.parent		= parentWindow;	
			this.demoType	= demoType;
		}


		//---------------------------------------------------------------------
		public DemoDialog(string filePath, Form parentWindow)
		{
			resourcePaths = new ArrayList ();
			resourcePaths.Add(filePath);
			
			this.IDD		= 0;
			this.parent		= parentWindow;	
			this.demoType	= Type.Woorm;
		}

		//---------------------------------------------------------------------
		public DemoDialog(string filePath, string dialogName, Form parentWindow)
		{
			resourcePaths = new ArrayList ();
			resourcePaths.Add(filePath);
			
			this.IDD		= 0;
			this.dialogName = dialogName;
			this.parent		= parentWindow;	
			this.demoType	= Type.WoormAskDialog;
		}

		//---------------------------------------------------------------------
		private void LoadLibraries()
		{
			valid = false;

			string path = Path.GetDirectoryName(Application.ExecutablePath);
		
			string TBStringLoaderPath = Path.Combine(path, "TBStringLoader.dll");

			if (LoadLibrary(TBStringLoaderPath) == IntPtr.Zero)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, String.Format(Strings.CantFindModule, TBStringLoaderPath));
				return;
			}

			valid = true;
			stringLoaderLoaded = true;
		}

		//---------------------------------------------------------------------
		public static void FreeStringLoaderResources()
		{
			if (stringLoaderLoaded)
				FreeCache();
		}

		//---------------------------------------------------------------------
		public void SetFont()
		{
			if (!valid) return;

			SetFormFont();
		}
		
		//---------------------------------------------------------------------
		public bool Check(float ratio, string dictionaryPath, out LocalizerDocument doc)
		{
			doc = null;

			if (!valid) return false;
			
			string xmlResult = string.Empty;
			foreach (string resourcePath in resourcePaths)
			{
				xmlResult = CheckDialog(resourcePath, IDD, ratio, dictionaryPath);
				
				FreeCache();
			
				if (xmlResult != string.Empty) break;
			}

			if (xmlResult == string.Empty) return false;

			doc = new LocalizerDocument();
			doc.LoadXml(xmlResult);
			
			return true;
		}
		
		//---------------------------------------------------------------------
		public bool Show(string xml)
		{
			if (!valid) return false;

			try
			{
				switch (demoType)
				{
					case Type.Dialog:
					{
						foreach (string resourcePath in resourcePaths)
						{
							if (!ExistDialog(resourcePath, IDD))
								continue;

							if (ShowDemoDialog(parent.Handle, xml, resourcePath, IDD))
								return true;
						}
						
						return false;
					}
					
					case Type.Woorm:
					{
						xmlDocument = new XmlDocument();
						xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration(AllStrings.version, AllStrings.encoding, null));
						XmlDocumentFragment df = xmlDocument.CreateDocumentFragment();
						df.InnerXml = xml;
						xmlDocument.AppendChild(df);
						

						return ShowWoorm(null);	
					}
						
					case Type.WoormAskDialog:
					{
						xmlDocument = new XmlDocument();
						xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration(AllStrings.version, AllStrings.encoding, null));
						XmlDocumentFragment df = xmlDocument.CreateDocumentFragment();
						df.InnerXml = xml;
						xmlDocument.AppendChild(df);
						
						return ShowWoorm(dialogName);
					}

				}

				return false;
			}
			catch (Exception ex)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, ex.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		public bool Close()
		{
			if (!valid) return false;

			try
			{
				switch (demoType)
				{
					case Type.Dialog:
						return CloseDemoDialog(IDD);
					
					case Type.Woorm:
					case Type.WoormAskDialog :	
						return CloseHtmlDialog();
				}
				return false;				
			}
			catch (Exception ex)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, ex.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		public bool ShowWoorm(string askDialogName)
		{
			if (!valid) return false;

			try
			{
				if (!File.Exists(WoormFile)) 
				{
					MessageBox.Show(this.parent, string.Format(Strings.FileNotFound, WoormFile));
					return false;	
				}

				if (htmlDialog == null || htmlDialog.IsDisposed)
				{
					htmlDialog = new HtmlDialog();
					htmlDialog.Owner = parent;
					htmlDialog.Closed += new EventHandler(HtmlDialogClosed);
				}

				string resourcePath = resourcePaths[0] as string;
				if (resourcePath == null) return false;
				string dictionaryFilePath = Path.GetDirectoryName(resourcePath);		

				string tmpFile = Path.ChangeExtension(Path.GetTempFileName(), "xml");	
				xmlDocument.Save(tmpFile);
				string url = string.Format	(
					"http://{0}:{1}/WoormViewer.aspx?Filename={2}&DictionaryFilePath={3}&DataFile={4}&Installation={5}", 
					Environment.MachineName,
					DictionaryCreator.MainContext.SocketPort, 
					HttpUtility.UrlEncode(WoormFile),
					HttpUtility.UrlEncode(dictionaryFilePath),
					HttpUtility.UrlEncode(tmpFile),
					HttpUtility.UrlEncode(CommonFunctions.CurrentEnvironmentSettings.Installation)
					);

				if (askDialogName != null)
					url += string.Format("&AskDialogName={0}", HttpUtility.UrlEncode(askDialogName));

				htmlDialog.Show(url);

			}
			catch (Exception exc)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, exc.Message);
				return false;
			}
			finally
			{
			}
			return true;
		}
	
		//---------------------------------------------------------------------
		public bool CloseHtmlDialog()
		{
			if (!valid) return false;

			if (htmlDialog != null)
			{
				htmlDialog.Close();
				return true;
			}
			return false;
		}

		//---------------------------------------------------------------------
		private void AdjustWindowPosition(Form aForm, Form oldForm)
		{
			aForm.StartPosition = FormStartPosition.Manual;
			aForm.AutoScaleMode = AutoScaleMode.Font;
			if (oldForm != null)
			{
				aForm.Location = oldForm.Location;
				aForm.Size = oldForm.Size;
				return;
			}
			
			aForm.Left	= parent.Left;
			aForm.Top	= parent.Bottom;
			Rectangle rect = Screen.FromControl(aForm).WorkingArea;
			if (!rect.Contains(new Point(aForm.Left, aForm.Bottom)))
				aForm.Top -= (aForm.Bottom - rect.Bottom);
		}

		//---------------------------------------------------------------------
		private void HtmlDialogClosed(object sender, EventArgs args)
		{
			htmlDialog = null;
		}

	}

}
