using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;

namespace Microarea.EasyAttachment.UI.Controls
{
	///<summary>
	/// Combobox derivata che consente di elencare le classi documentali
	///</summary>
	//================================================================================
	public class SOSClassesComboBox : ComboBox
	{
		private IContainer components;

		// eventi da ruotare all'esterno
        public delegate void SOSClassesComboBoxDelegate(Object sender, EventArgs e);
        public event SOSClassesComboBoxDelegate SOSClassesComboBoxSelectedIndexChanged;

		[Browsable(false)]
		public List<int> CollectionIDs { get { return ((SOSClassComboItem)SelectedItem).CollectionIDs; } }

		public DMSOrchestrator Orchestrator { get; set; }

		//--------------------------------------------------------------------------------
        public SOSClassesComboBox()
		{
			InitializeComponent();
            DropDownStyle = ComboBoxStyle.DropDownList;

			this.DrawMode = DrawMode.OwnerDrawFixed;
		}

		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ComponentResourceManager resources = new ComponentResourceManager(typeof(ExtensionImageComboBox));
		}

		//---------------------------------------------------------------------
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);
            if (SOSClassesComboBoxSelectedIndexChanged != null)
                SOSClassesComboBoxSelectedIndexChanged(this, e);
		}

		/// <summary>
		/// Override OnDrawItem, To Be able To Draw Text, And Font Formatting
		/// </summary>
		//--------------------------------------------------------------------------------
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground(); //Draw Background Of Item
            e.DrawFocusRectangle(); //Draw Its rectangle

            SolidBrush foreBrush = new SolidBrush(e.ForeColor);
            SolidBrush currForeBrush = new SolidBrush(e.ForeColor);

            Font f = new Font(e.Font, FontStyle.Bold);

            try
            {
                if (e.Index < 0) //Do We Have A Valid List Just Draw Indented Text
                    e.Graphics.DrawString(this.Text, e.Font, foreBrush, e.Bounds.Left, e.Bounds.Top);
                else //We Have A List
                {
                    if (this.Items[e.Index].GetType() == typeof(SOSClassComboItem))  //Is It A ImageCombo Item 
                    {
                        SOSClassComboItem ICCurrItem = (SOSClassComboItem)this.Items[e.Index]; //Get Current Item

                        //Obtain Current Item's ForeColour
                        Color ICCurrForeColour = e.ForeColor;
                        currForeBrush = new SolidBrush(ICCurrForeColour);
                        //Obtain Current Item's Font
                        Font ICCurrFont = e.Font;

                        //Then, Draw Text In Specified Bounds
                        e.Graphics.DrawString(ICCurrItem.Text, ICCurrFont, currForeBrush, e.Bounds.Left, e.Bounds.Top);
                    }
                    else //Not An ImageCombo Box Item, Just Draw The Text
                        e.Graphics.DrawString(this.Items[e.Index].ToString(), e.Font, foreBrush, e.Bounds.Left, e.Bounds.Top);
                }
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                foreBrush.Dispose();
                currForeBrush.Dispose();
                f.Dispose();
            }

            base.OnDrawItem(e);
        }
    }

    ///<summary>
    /// Generico item da aggiungere all'ImageComboBox
    /// Consente di avere un testo affiancato da un'immagine (estrapolata tramite il numero di indice
    /// in cui e' stata inserita nell'imagelist associata alla combo)
    ///</summary>
    //================================================================================
    public class SOSClassComboItem
    {
		public string Code = null;
		public string Name = null;
		public List<int> CollectionIDs = null;
        public List<string> SosDocumentTypes = null;


		//--------------------------------------------------------------------------------
		public string Text { get { return Name; } }
		public string Value { get { return Code; } }

		//--------------------------------------------------------------------------------
        public SOSClassComboItem(string itemText, string itemValue, List<int> coll, List<string> sosDocumentTypes)
		{
			Name = itemText;
			Code = itemValue;
			CollectionIDs = coll;
            SosDocumentTypes = sosDocumentTypes;
		}

        /// <summary>
        /// Override ToString To Return Item Text
        /// </summary>
        //--------------------------------------------------------------------------------
        public override string ToString()
        {
            return Text;
        }

        //--------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            SOSClassComboItem val = obj as SOSClassComboItem;
            if (val == null) return false;
            return String.Compare(val.Text, Text, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

		//--------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }
}
