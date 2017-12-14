using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microarea.Library.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.MenuManager
{
	public partial class DocOrganizerPanel : DockPanel
	{
		public enum CloseButtonType { Normal, MouseOver, Pressed }
        
        /*private bool mouseOverCloseButton = false;
		private bool mousePressed = false;
		private bool mouseOverOpenFormsList = false;
		private bool mouseOverOpenFormsIcon = false;
		private int mouseTabIndex = -1;*/
        private List<WindowItem> windowsList = null;
		private bool addingWindow = false;
		//private OpenFormsDockableForm openForms = null;

		private TbApplicationClientInterface tbAppClientInterface;
		private MenuMngForm ownerForm;
		//private DocumentToolTip toolTipTabDocument;
		private DocumentStates documentStates;

		public event EventHandler WindowListAdded;
		public event EventHandler WindowListRemoved;
        public event EventHandler WindowListZero;

        //------------------------------------------------------------------------------------------------------------------------------
        public DocumentStates DocumentStates { get { return documentStates; } }

        //-----------------------------------------------------------------------------------------
        public DocOrganizerPanel()
        {
            InitializeComponent();
            // 
            // MenuStripOpenDocs
            // 
            this.components = new System.ComponentModel.Container();

            this.windowsList = new List<WindowItem>();
            this.ActiveDocumentChanged += new EventHandler(DocOrganizerPanel_FocusActiveDocumentFrame);
            this.ActivePaneChanged += new EventHandler(DocOrganizerPanel_FocusActiveDocumentFrame);

            ITheme theme = DefaultTheme.GetTheme();
            this.BackgroundImageLayout = ImageLayout.Center;
            BackgroundImage = global::Microarea.MenuManager.MenuManagerStrings.watermark2;
            BackColor = DockBackColor = theme.GetThemeElementColor("LoginBackGroundColor");  //TODOLUCA
        }

        //-----------------------------------------------------------------------------------------
        internal void LoadDocumentStates(string file)
		{
			documentStates = DocumentStates.Load(file);
		}

		//-----------------------------------------------------------------------------------------
		protected virtual void OnWindowListAdded()
		{
			if (WindowListAdded != null)
				WindowListAdded(this, EventArgs.Empty);
		}

        //-----------------------------------------------------------------------------------------
        protected virtual void OnWindowListRemoved()
        {
            if (WindowListRemoved != null)
                WindowListRemoved(this, EventArgs.Empty);
        }


        //-----------------------------------------------------------------------------------------
        protected virtual void OnWindowListZero()
        {   
            if (WindowListZero != null)
                   WindowListZero(this, EventArgs.Empty);
        }

        //-----------------------------------------------------------------------------------------
        private WindowItem GetWindowItem(IntPtr handle)
		{
			return WindowItem.Find(windowsList, handle);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private WindowItem GetWindowItem(GenericDockableForm form)
		{
			if (form == null)
				return null;
			return WindowItem.Find(windowsList, form);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void OnContextMenuClick()
		{

		}

		//-----------------------------------------------------------------------------------------
		private void ClickLayout(int x, int y)
		{

		}

		//-----------------------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == ExternalAPI.WM_LBUTTONDOWN)
			{
				int x = (m.LParam.ToInt32() << 16) >> 16;
				int y = m.LParam.ToInt32() >> 16;
				ClickLayout(x, y);
			}
			if (
					m.Msg == ExternalAPI.WM_MOUSEACTIVATE
					&&
					ownerForm != null
					&&
					(IsActiveMainMenuStrip(ownerForm.MainMenuStrip))
				)
			{	//fictitious click to hide menuStrip(s), because click in document clientArea doesn't hide menu
				int coord = 0;
				ExternalAPI.PostMessage(FindForm().Handle, ExternalAPI.WM_LBUTTONDOWN, (IntPtr)coord, (IntPtr)coord);
			}
			base.WndProc(ref m);
		}

		//--------------------------------------------------
		bool IsActiveMainMenuStrip(MenuStrip menu)
		{
			if (menu == null)
				return false;
			foreach (ToolStripMenuItem tsmi in menu.Items)
			{
				foreach (ToolStripItem tsi in tsmi.DropDownItems)
					if (tsi.Visible)
						return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------------------
		public bool HandleMessage(ref Message m)
		{
			try
			{
				//Management message sent by windows (document, menumanager, ecc.)
				switch (m.Msg)
				{
					case ExternalAPI.UM_DOCUMENT_CREATED:
						{
							m.Result = IntPtr.Zero;
							AddWindow((IntPtr)m.WParam, (IntPtr)m.LParam);
							OnWindowListAdded();
							return true;
						}

					case ExternalAPI.UM_DOCUMENT_DESTROYED:
						{
                            m.Result = IntPtr.Zero;
							RemoveWindow((IntPtr)m.WParam);
							OnWindowListRemoved();

                            if (Contents.Count == 0)
                                OnWindowListZero();
                            return true;
						}
					case ExternalAPI.UM_SWITCH_ACTIVE_TAB:
						{
							ActivatePrevious();
							return true;
						}
					case ExternalAPI.UM_FRAME_ACTIVATE:
						{
							IntPtr hwnd = m.WParam;
							//do not propagate activation message to clients to avoid activation loops
							ActivateWindow(hwnd);
							return true;
						}

					case ExternalAPI.UM_MENU_CREATED:
						{
							m.Result = IntPtr.Zero;

							WindowItem item = GetWindowItem((IntPtr)m.WParam);
							if (item != null)
							{
								item.Form.CloseButton = false;
								item.Form.CloseButtonVisible = false;
							}
							return true;
						}

					//Management message sent by windows (document, menumanager, ecc.)
					case ExternalAPI.UM_FRAME_TITLE_UPDATED:
						{
							IntPtr handle = (IntPtr)m.WParam;
							WindowItem item = GetWindowItem(handle);
							if (item != null)
							{
								string title, tooltip;
								item.UpdateFormTitle(out title, out tooltip);
							}
							return true;
						}

					case ExternalAPI.WM_ACTIVATE:
						{
							//TODO
							DocumentDockableForm active = ActiveDocument as DocumentDockableForm;
							if (active == null)
								return true;

							foreach (var item in windowsList)
							{
								if (item.Form == active)
									ExternalAPI.SendMessage(item.Handle, m.Msg, m.WParam, m.LParam);
							}
							return true;

						}

					case ExternalAPI.WM_ENTERSIZEMOVE:
					case ExternalAPI.WM_EXITSIZEMOVE:
						{
							foreach (var item in windowsList)
							{
								ExternalAPI.SendMessage(item.Handle, m.Msg, m.WParam, m.LParam);
							}
							return true;
						}
					default:
						return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public void Init(TbApplicationClientInterface tbApp, MenuMngForm form)
		{
			tbAppClientInterface = tbApp;
			ownerForm = form;
			if (this.ownerForm.TabbedDocuments)
				Functions.DoParallelProcedure(() => { tbAppClientInterface.SetDocked(true); });
		}

		//------------------------------------------------------------------------------------------------------------------------------
		//forza il fuoco alla frame del documento attivo, che a volte si perde (inserimento al volo)
		void DocOrganizerPanel_FocusActiveDocumentFrame(object sender, EventArgs e)
		{
			DocumentDockableForm page = ActiveContent as DocumentDockableForm;
			if (page != null)
				ExternalAPI.SetFocus(page.WindowHandle);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public void ActivateWindow(IntPtr windowHandle)
		{
			WindowItem wi = WindowItem.Find(windowsList, windowHandle);
			if (wi != null)
			{
				DocumentDockableForm page = ActiveContent as DocumentDockableForm;
				if (page == null || page.WindowHandle != wi.Handle)
				{
					wi.Form.Activate();
				}
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void AddWindow(IntPtr windowHandle, IntPtr menuHandle)
		{
			if (!ExternalAPI.IsWindow(windowHandle))
				return;
			try
			{
				addingWindow = true;
				DocumentDockableForm newForm = new DocumentDockableForm(windowHandle, DocumentStates);
				WindowItem item = new WindowItem(windowHandle, menuHandle, newForm, tbAppClientInterface);
				windowsList.Add(item);

				newForm.MergeMenu(item);

				//blocco il layout per evitare rallentamenti
				this.SuspendLayout(true);

				if (DocumentStates != null)
				{
					DocumentState state = DocumentStates.Get(newForm.DocumentNamespace);
					if (state.DockState == DockState.Float)
					{
						FormWindowState fws = state.WindowState;//me lo metto da parte perche' la show lo cambia
						newForm.Show(this, state.Rectangle);
						newForm.FloatPane.FloatWindow.WindowState = fws;
					}
					else
					{
						DockState ds = state.DockState;
						if (ds == DockState.Hidden || ds == DockState.Unknown)
							ds = DockState.Document;
						newForm.Show(this, ds);
					}
				}
				else
				{
					newForm.Show(this, DockState.Document);
				}

				newForm.AlignParent();
				NotifyWindow(windowHandle, newForm.HostingHandle, true);

				//faccio una postmessage che ripristini il layout e imposti il fuoco (post perché altrimenti
				//mi rallenta l'applicativo che sta aspettando che questo codice rientri)
				BeginInvoke((ThreadStart)delegate
				{
                    newForm.Attached = true;//libero la gestione del resize (per evitare inutili chiamate lo faccio solo da questo momento)
                    this.ResumeLayout(true, true);
                    ExternalAPI.SetFocus(windowHandle);
				});
			}
			finally
			{
				addingWindow = false;
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void NotifyWindow(IntPtr windowHandle, IntPtr parentWindowHandle, bool docked)
		{
			ExternalAPI.SendMessage
					(
					windowHandle,
					ExternalAPI.UM_UPDATE_FRAME_STATUS,
					(IntPtr)Convert.ToInt16(docked),
					parentWindowHandle
					);	 //notify to c++ windows
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void RemoveWindow(IntPtr windowHandle)
		{
			this.Invoke( new MethodInvoker( () => 
			{
				WindowItem item = WindowItem.Find(windowsList, windowHandle);
				if (item == null || item.Form == null)
					return;

				windowsList.Remove(item);
				item.Dispose();
            }));
           
		}

		//------------------------------------------------------------------------------------------------------------------------------
		internal void ActivateFormWindow(bool activate, bool posted)
		{
			if (!addingWindow && this.ActiveDocument != null && !this.ActiveDocument.DockHandler.IsFloat)
			{
				DocumentDockableForm page = this.ActiveDocument as DocumentDockableForm;
				if (page != null)
					page.ActivateFormWindow(activate, posted);
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		internal void ActivateAppFormWindow(bool activate, bool posted)
		{
			if (!addingWindow && this.ActiveDocument != null && !this.ActiveDocument.DockHandler.IsFloat)
			{
				DocumentDockableForm page = this.ActiveDocument as DocumentDockableForm;
				if (page != null)
					page.ActivateAppFormWindow(activate, posted);
			}
		}
	}

}
