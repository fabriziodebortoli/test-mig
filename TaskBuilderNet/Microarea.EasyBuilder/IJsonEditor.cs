using Microarea.EasyBuilder.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.EasyBuilder
{
	/// <summary>
	/// Internal Use
	/// </summary>
	public interface IJsonEditor
	{
		/// <summary>
		/// Internal Use
		/// </summary>
		string Json { get; set; }
		/// <summary>
		/// Internal Use
		/// </summary>
		string InitialJsonFile { get; }
		/// <summary>
		/// Internal Use
		/// </summary>
		void AttachCodeEditor(JsonCodeControl c);
		/// <summary>
		/// Internal Use
		/// </summary>
		bool SaveJson();
		/// <summary>
		/// Internal Use
		/// </summary>
		bool OpenJson(string file);
		/// <summary>
		/// Internal Use
		/// </summary>
		bool CloseJson(string file);
		/// <summary>
		/// Internal Use
		/// </summary>
		bool UpdateWindow(IntPtr hwnd);
		/// <summary>
		/// Internal Use
		/// </summary>
		bool UpdateWindow(string v);
		/// <summary>
		/// Internal Use
		/// </summary>
		bool UpdateTabOrder(IntPtr hwnd);
		/// <summary>
		/// Internal Use
		/// </summary>
		void AddWindow(IntPtr hwnd, IntPtr hwndParent);
		/// <summary>
		/// Internal Use
		/// </summary>
		void DeleteWindow(IntPtr hwnd);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		void OnFormEditorDirtyChanged(Object sender, DirtyChangedEventArgs args);
		/// <summary>
		/// Internal Use
		/// </summary>
		string Undo();
		/// <summary>
		/// Internal Use
		/// </summary>
		string Redo();
		/// <summary>
		/// Internal Use
		/// </summary>
		void UpdateFromSourceCode(string newText);
	}
}
