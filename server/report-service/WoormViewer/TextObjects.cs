
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.RSWeb.WoormViewer;

namespace Microarea.RSWeb.Objects
{
	/// <summary>
	/// Summary description for GenericText.
	/// </summary>
	/// ================================================================================
	//[Serializable]
	public class Label : BasicText
	{
		//------------------------------------------------------------------------------
		public Label(WoormDocument doc)  : base(doc)
		{
			FontStyleName = DefaultFont.Descrizione;
			Align = AlignType.DT_LEFT | AlignType.DT_TOP;
		}

        //------------------------------------------------------------------------------
        public Label(Label s)
            : base (s)
        {  
        }

        //------------------------------------------------------------------------------
        public Label Clone()
        {
            return new Label(this);
        }
    }
    /// <summary>
    /// Summary description for GenericText.
    /// </summary>
    /// ================================================================================
    //[Serializable]
    public class WoormValue : BasicText
	{
		private object  rdeData = null;
		private string  formattedData = string.Empty;
		private bool    cellTail = false;		

		//const string FORMATTEDDATA = "FormattedData";
		//--------------------------------------------------------------------------------
		public object RDEData { get { return rdeData; } }
		//--------------------------------------------------------------------------------
        public string FormattedData { get { return formattedData; } set { formattedData = value; } }

		//per i valori di cella di colonna che continuano il valore di una cella della riga precedente 	
		//--------------------------------------------------------------------------------
		public bool CellTail	{	get { return cellTail; } }

        public bool IsValid     {   get { return rdeData != null; } } 
        //------------------------------------------------------------------------------
		public WoormValue(WoormDocument doc) : base(doc)
		{
			FontStyleName = DefaultFont.Testo;
			Align = Defaults.DefaultAlign;
		}

       //------------------------------------------------------------------------------
        public WoormValue(WoormValue s)
            : base (s)
        {
            rdeData = s.RDEData;
            formattedData = s.FormattedData;
            cellTail = s.CellTail;
        }

        //------------------------------------------------------------------------------
        public WoormValue Clone()
        {
            return new WoormValue(this);
        }

		//------------------------------------------------------------------------------
		public void Clear()
		{
			formattedData = "";
			rdeData = null;
            cellTail = false;
        }

		//------------------------------------------------------------------------------
		public void AssignData(object rdeData, string formattedData, bool cellTail)
		{
			this.rdeData = rdeData;
			this.formattedData = formattedData;
			this.cellTail = cellTail;
		}
	}
}
