using System;
using System.Runtime.InteropServices;

namespace Microarea.TaskBuilderNet.Interfaces.Model
{
	//=========================================================================
	public interface IDataType
	{
		bool IsATime();
		bool IsFullDate();
		void SetAsTime();
		void SetAsTime(bool isTime);
		void SetFullDate(bool fullDate);
		void SetFullDate();
		int Tag { get; set; }
		int Type { get; set; }
		bool IsEnum { get; }
	}
	//=========================================================================
	[ComVisible(false)]
	[System.ComponentModel.TypeConverter(typeof(System.ComponentModel.TypeConverter))]
	public interface IDataObj
	{
		//---------------------------------------------------------------------
		IDataType DataType { get; }
		//---------------------------------------------------------------------
		object Value { get; set; }
        //---------------------------------------------------------------------
        bool Modified { get; set; }
        //---------------------------------------------------------------------      
        void Clear();
        //---------------------------------------------------------------------      
        bool Equals(object obj);
		//---------------------------------------------------------------------
		object Clone();
		//---------------------------------------------------------------------
		bool BelongsToPrototypeRecord { get; }
	}

	//=========================================================================
	/// <summary>
	/// Available stati for a DataObj instance.
	/// </summary>
	[Flags]
	public enum DataStati
	{
		// Basato sulla definizione di DataStatus in DataObj.h
		DBCaseCompliant = 0x0001,
		Uppercase = 0x0002,
		FullDate = 0x0004,	// usato solo per DataDate per indicare l'uso del Time
		Time = 0x0008,	// usato per DataDate per indicare che e` un Ora
		// e per DataLng per indicare che e` un tempo
		ReadOnly = 0x00010,	// per gestire il readonly nei controls dipendentemente allo stato del documento
		Hide = 0x00020,	// per gestire il hide/show dei controls
		Findable = 0x00040,	// abilita la ricerca nei documenti
		ValueChanged = 0x00080,	// riservato ed utilizzabile dal programmatore
		Valid = 0x00100,	// usato dal report engine
		Modified = 0x00200,	// riservato dalla gestione interna del documento
		Dirty = 0x00400,	// usato per ottimizzare i/o su database
		UpdateView = 0x00800,	// usato per forzare la rivisualizzazione del dato
		OslReadOnly = 0x01000,	// OSL: per gestire il readonly nei controls
		OslHide = 0x02000,	// OSL: per gestire il hide/show dei controls
		AlwaysReadOnly = 0x04000,	// per gestire il readonly nei controls indipendentemente dallo stato del documento
		ValueLocked = 0x08000,	// per impedire l'assegnazione di un nuovo valore al DataObj
		CollateCultureSensitive = 0x10000,	// indica che il contenuto è collate culture-sensitive
		TbHandle = 0x0004,	// usato come attributo del DATA_LONG_TYPE (DataType::Long) per indicare che il contenuto è un handle (DataType::Object)
		TbVoid = 0x0008		// usato come attributo del DATA_NULL_TYPE (DataType::Null) per indicare un valore di ritorno void (DataType::Void)
	}
}
