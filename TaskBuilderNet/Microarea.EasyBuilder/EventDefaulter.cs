
using System.ComponentModel;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;

namespace Microarea.EasyBuilder
{
	sealed class EventDefaulter
	{
		//--------------------------------------------------------------------------------
		private EventDefaulter()
		{}

		//--------------------------------------------------------------------------------
		public static IList<Statement> GetDefaultEventCode(EasyBuilderComponent component, EventDescriptor e)
		{
			IList<Statement> coll = new List<Statement>();
			MDBTSlaveBuffered dbt = component as MDBTSlaveBuffered;
			if (dbt != null)
			{
				if ( 
						e.Name == "AddingRow" ||
						e.Name == "RowAdded" ||
						e.Name == "InsertingRow" ||
						e.Name == "RowInserted" ||
						e.Name == "DeletingRow" ||
						e.Name == "AuxColumnsPrepared" ||
						e.Name == "PrimaryKeyPrepared" ||
						e.Name == "RowPrepared"
					)
				{
					coll.Add(
							new VariableDeclarationStatement(
								new SimpleType(((MSqlRecord)dbt.Record).SerializedType),
								"record",
								new CastExpression(
									new SimpleType(((MSqlRecord)dbt.Record).SerializedType),
									new MemberReferenceExpression(new IdentifierExpression("e"), RecordSerializer.RecordPropertyName)
								)
							)
						);
				}
			}
			return coll;
		}
	}
}
