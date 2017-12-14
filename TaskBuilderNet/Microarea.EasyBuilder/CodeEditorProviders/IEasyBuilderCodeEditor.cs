using System;
using System.Windows.Forms;
using ICSharpCode.NRefactory.CSharp;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.EasyBuilder.CodeEditorProviders
{
	/// <summary>
	/// Internal Use
	/// </summary>
	//==================================================================================
	public interface IEasyBuilderCodeEditor : IDockContent, IDisposable
	{
		/// <remarks/>
		//--------------------------------------------------------------------------------
		event EventHandler Disposed;

		/// <remarks/>
		//--------------------------------------------------------------------------------
		event FormClosedEventHandler FormClosed;

		/// <remarks/>
		//--------------------------------------------------------------------------------
		event EventHandler RefreshPropertyGrid;

		/// <remarks/>
		//--------------------------------------------------------------------------------
		event EventHandler RefreshIntellisense;

		/// <remarks/>
		//--------------------------------------------------------------------------------
		void Activate();

		/// <remarks/>
		//--------------------------------------------------------------------------------
		void Show(DockPanel hostingPanel, DockState dockState);

		/// <remarks/>
		//--------------------------------------------------------------------------------
		bool CollectAndSaveCodeChanges();

		/// <remarks/>
		//--------------------------------------------------------------------------------
		void CollectOriginalMethodBody();

		/// <remarks/>
		//--------------------------------------------------------------------------------
		bool Save();

		/// <remarks/>
		//--------------------------------------------------------------------------------
		void Close();

		/// <remarks/>
		//--------------------------------------------------------------------------------
		bool IsDisposed { get; }

		/// <remarks/>
		//--------------------------------------------------------------------------------
		bool IgnoreChangesOnFormClosing { get; set; }

		/// <remarks/>
		//--------------------------------------------------------------------------------
		void PopulateErrors();

		/// <remarks/>
		//--------------------------------------------------------------------------------
		void Initialize(Editor editor, MethodDeclaration method = null);

		/// <remarks/>
		//--------------------------------------------------------------------------------
		void ReinitializeForm(MethodDeclaration method);

		/// <remarks/>
		//--------------------------------------------------------------------------------
		void SetDirty(bool v);
	}
}
