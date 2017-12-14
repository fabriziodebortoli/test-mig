using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder.UI
{
	//===================================================================================
	/// <remarks/>
	public partial class Localization : UserControl
	{
		string currentCulture = string.Empty;
		/// <remarks/>
		private FormEditor formEditor;
		private static CultureInfo[] availableCultures = null;
		private static CultureInfo[] AvailableCultures
		{
			get
			{
				if (availableCultures == null)
				{
					List<CultureInfo> cultures = new List<CultureInfo>();
					cultures.AddRange(CultureInfo.GetCultures(CultureTypes.AllCultures));
					cultures.Sort(
						(CultureInfo a, CultureInfo b) =>
						{
							return a.DisplayName.CompareTo(b.DisplayName);
						}
						);
					availableCultures = cultures.ToArray();
				}
				return availableCultures;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp();
			hevent.Handled = true;
		}
		//--------------------------------------------------------------------------------
		/// <remarks/>
		public Localization(FormEditor formEditor, string culture)
		{
			this.formEditor = formEditor;
			this.currentCulture = culture;
			InitializeComponent();
			this.Text = Resources.LocalizationFormTitle;
			//l'event handler va impostato solo dopo che è stato assegnato il data source e l'elemento corrente
		}

		//--------------------------------------------------------------------------------
		private void Localization_Load(object sender, EventArgs e)
		{
			PopulateStrings(currentCulture);

			this.localizableStringBindingSource.ListChanged += new System.ComponentModel.ListChangedEventHandler(this.localizableStringBindingSource_ListChanged);
			this.localizableControlBindingSource.ListChanged += new System.ComponentModel.ListChangedEventHandler(this.localizableStringBindingSource_ListChanged);
		}

		//--------------------------------------------------------------------------------
		internal void PopulateStrings(string culture)
		{
			List<LocalizableString> ls = formEditor.Sources.Localization.GetLocalizableStrings(culture);

			List<LocalizableString> strings = new List<LocalizableString>();
			List<LocalizableString> controls = new List<LocalizableString>();
			foreach (LocalizableString item in ls)
			{
				if (item.IsControl)
					controls.Add(item);
				else
					strings.Add(item);
			}

			strings.Sort();
			controls.Sort();

			localizableStringBindingSource.DataSource = strings;
			localizableControlBindingSource.DataSource = controls;
		}
		
		//--------------------------------------------------------------------------------
		internal void SaveStrings()
		{
			try
			{
				Cursor = Cursors.WaitCursor;
				List<LocalizableString> strings = localizableStringBindingSource.DataSource as List<LocalizableString>;
				if (strings != null)
                    formEditor.Sources.Localization.UpdateResourceManagerStrings(strings, formEditor.Sources.CustomizationInfos);

				strings = localizableControlBindingSource.DataSource as List<LocalizableString>;
				if (strings != null)
                    formEditor.Sources.Localization.UpdateResourceManagerControls(strings);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		//--------------------------------------------------------------------------------
		private void localizableStringBindingSource_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
		{
			SaveStrings();
		}

		//--------------------------------------------------------------------------------
		private void dgStrings_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			// Clear the row error in case the user presses ESC.   
			dgStrings.Rows[e.RowIndex].ErrorText = String.Empty;
		}

		//--------------------------------------------------------------------------------
		private void dgStrings_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			DataGridViewRow row = dgStrings.Rows[e.RowIndex];
			
			string name = row.Cells[nameDataGridViewTextBoxColumn.Name].FormattedValue.ToString();
			if (!BaseCustomizationContext.CustomizationContextInstance.IsValidName(name))
			{
				row.ErrorText = Resources.InvalidName;
				e.Cancel = true;
				return;
			}
			foreach (DataGridViewRow r in dgStrings.Rows)
			{
				if (
					name.CompareNoCase(r.Cells[nameDataGridViewTextBoxColumn.Name].FormattedValue.ToString()) &&
					r.Index != e.RowIndex
					)
				{
					row.ErrorText = Resources.DuplicateResourceName;
					e.Cancel = true;
					return;
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void dgStrings_RowEnter(object sender, DataGridViewCellEventArgs e)
		{
			DataGridViewCell cell = dgStrings.Rows[e.RowIndex].Cells[nameDataGridViewTextBoxColumn.Name];
			string newName = cell.FormattedValue.ToString();
			if (newName.IsNullOrEmpty())
				cell.Value = GenerateNewLocalizedString();
		}

		//--------------------------------------------------------------------------------
		private string GenerateNewLocalizedString()
		{
			int i = 0;
			string newName = string.Empty;
			do
			{
				i++;
				newName = string.Format("String{0}", i);
			} while (ExistName(newName));

			return newName;
		}

		//--------------------------------------------------------------------------------
		private bool ExistName(string name)
		{
			foreach (DataGridViewRow r in dgStrings.Rows)
			{
				if (name.Equals(r.Cells[nameDataGridViewTextBoxColumn.Name].FormattedValue))
					return true;
			}
			return false;
		}
	}

	//===================================================================================
	/// <remarks/>
	public class LocalizableString : IComparable
	{
		/// <remarks/>
		public LocalizableString()
		{
			Name = Text = "";
			IsControl = false;
		}
		/// <remarks/>
		public string Name { get; set; }
		/// <remarks/>
		public string Text { get; set; }
		/// <remarks/>
		public bool IsControl { get; set; }

		/// <summary>
		/// Comparer
		/// </summary>
		//--------------------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			if (obj is LocalizableString)
				return this.Name.CompareTo(((LocalizableString)obj).Name);
			else
				return 0;
		}
	}
}
