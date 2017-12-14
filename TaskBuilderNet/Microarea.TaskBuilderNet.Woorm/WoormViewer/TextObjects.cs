
using System;
using System.Runtime.Serialization;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

namespace RSjson
{
	/// <summary>
	/// Summary description for GenericText.
	/// </summary>
	/// ================================================================================
	[Serializable]
	public class Label : BasicText
	{
		//------------------------------------------------------------------------------
		public Label(WoormDocument doc)  : base(doc)
		{
			FontStyleName = DefaultFont.Descrizione;
			Align = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_TOP;
		}

		//------------------------------------------------------------------------------
		public Label(SerializationInfo info, StreamingContext context)
		: base (info, context)
		{
		}
	}
	/// <summary>
	/// Summary description for GenericText.
	/// </summary>
	/// ================================================================================
	[Serializable]
	public class WoormValue : BasicText
	{
		private object  rdeData = null;
		private string  formattedData = string.Empty;
		private bool    cellTail = false;		

		const string FORMATTEDDATA = "FormattedData";
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
		public WoormValue(SerializationInfo info, StreamingContext context)
		{
			formattedData = info.GetString(FORMATTEDDATA);
		}

        //------------------------------------------------------------------------------
        public WoormValue(WoormValue s)
            : base (s)
        {
        }

		//-------------------------------------------------------------------------------
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(FORMATTEDDATA, formattedData);
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
