using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using Microarea.EasyAttachment.Components;

namespace Microarea.EasyAttachment.UI.Forms
{
	///<summary>
	/// Form per la modifica multipla dei valori
	///</summary>
	//================================================================================
	public partial class ModifySelectedRows : Form
    {
		public ModificationSelections ModificationSelections { get; private set; }

		///<summary>
		/// Private constructor
		///</summary>
		//---------------------------------------------------------------------
		private ModifySelectedRows()
        {
            InitializeComponent();
        }

		///<summary>
		/// Metodo statico da richiamare per l'apertura della form
		/// Serve per poter valorizzare le selezioni e non perderle con la chiusura della form
		///</summary>
		//--------------------------------------------------------------------------------
		public static ModificationSelections OpenForm(DTCategories dtEnabledCategories)
		{
			ModifySelectedRows msrForm = new ModifySelectedRows();
			msrForm.InitForm(dtEnabledCategories);
			msrForm.ShowDialog();

			// chiudo la form e ritorno la struttura con le selezioni
			return msrForm.ModificationSelections;
		}

		///<summary>
		/// Metodo richiamato esternamente per aggiungere dinamicamente n-label e n-combobox
		/// quante sono le categorie NON disabilitate
		///</summary>
		//---------------------------------------------------------------------
		private void InitForm(DTCategories dtEnabledCategories)
		{
			int y = 30;
			int lastComboBoxY = 0;

			// aggiungo dinamicamente le combobox e relative label contenenti le categorie censite
			foreach (DataRow dr in dtEnabledCategories.Rows)
			{
				Label LblCategoryName = new Label();
				LblCategoryName.Location = new Point(LblYear.Location.X, LblYear.Location.Y + y);

				LblCategoryName.Name = "Lbl" + dr[CommonStrings.Name].ToString();
				LblCategoryName.Text = dr[CommonStrings.Name].ToString();
				this.SplitContainerForm.Panel1.Controls.Add(LblCategoryName);

				ComboBox categoryComboBox = new ComboBox();
				categoryComboBox.Location = new Point(TBYear.Location.X, TBYear.Location.Y + y);
				categoryComboBox.Name = "CBox" + dr[CommonStrings.Name].ToString();
				categoryComboBox.FlatStyle = FlatStyle.Flat;
				categoryComboBox.Size = new Size(categoryComboBox.Size.Width + 100, categoryComboBox.Size.Height);

				DTCategoriesValues categoryValues = dr[CommonStrings.ValueSet] as DTCategoriesValues;
				if (categoryValues != null)
					foreach (DataRow val in categoryValues.Rows)
						categoryComboBox.Items.Add(val[CommonStrings.Value].ToString());

				this.SplitContainerForm.Panel2.Controls.Add(categoryComboBox);

				y += 30;
				lastComboBoxY = categoryComboBox.Location.Y + categoryComboBox.Height;
			}

			// solo se ho delle categorie sposto i pulsanti Ok-Cancel sotto a tutto
			if (dtEnabledCategories.Rows.Count > 0)
			{
				BtnOk.Location = new Point(BtnOk.Location.X, lastComboBoxY + 10);
				BtnCancel.Location = new Point(BtnCancel.Location.X, lastComboBoxY + 10);

				if (this.Size.Height < BtnOk.Location.Y)
				{
					this.Size = new Size(this.Size.Width, this.Size.Height + (BtnOk.Location.Y - this.Size.Height) + BtnOk.Size.Height + 60);
					this.Refresh();
				}
			}
		}

		///<summary>
		/// Intercetto il pulsante OK e riempio la struttura in memoria con le selezioni
		///</summary>
		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			ModificationSelections = new ModificationSelections();
			ModificationSelections.Description = TBDescription.Text;
			ModificationSelections.FreeTags = TBFreeTags.Text;
			ModificationSelections.Year = TBYear.Text;

			foreach (Control ctrl in this.SplitContainerForm.Panel2.Controls)
			{
				if (ctrl is ComboBox)
				{
					string selText = ((ComboBox)ctrl).Text;
					ModificationSelections.CategoryValues.Add(selText); // devo aggiungere anche le stringhe vuote perche' vado per position!
				}
			}

			// alla fine forzo la chiusura della form
			this.Close();
		}
    }

	///<summary>
	/// Classe di appoggio per rendere visibili alla form padre le selezioni dell'utente
	///</summary>
	//================================================================================
	public class ModificationSelections
	{
		public string Description = string.Empty;
		public string FreeTags = string.Empty;
		public string Year = string.Empty;

		public List<string> CategoryValues = new List<string>();
	}
}
