using System;
using Microarea.Framework.TBApplicationWrapper;

namespace Microarea.TaskBuilderNet.Model
{
	//=========================================================================
	/// <summary>
	/// DbtExtensionMethods
	/// </summary>
	public static class DbtExtensionMethods
	{
		//---------------------------------------------------------------------
		/// <summary>
		/// Initialize a new instance of DBTSlave 
		/// </summary>
		public static void InitializeRecord<TRecord>(this MDBTObject @this)
			where TRecord : MSqlRecord
		{
			//spostati qui e non più lazy: quando vengo istanziato dal dbtslavebuffered perché io ne sono lo slave,
			//nessuno chiamava la Record, e nessuno forza la creazione del record tipizzato
			//la Add assegna anche il field record

			//la Add assegna anche il field record
			@this.Add((TRecord)Activator.CreateInstance(typeof(TRecord), @this.GetRecordPtr()));
			System.Diagnostics.Debug.Assert(@this.Record != null);
			//la Add assegna anche il field oldRecord
			@this.Add((TRecord)Activator.CreateInstance(typeof(TRecord), @this.GetOldRecordPtr()));
			System.Diagnostics.Debug.Assert(@this.OldRecord != null);

			@this.CreateComponents();
		}
	}
}
