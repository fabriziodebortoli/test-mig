using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyAttachment.Core
{
	//================================================================================
	partial class DMSModelDataContext
	{
		//--------------------------------------------------------------------------------------------------
		partial void OnCreated()
		{
			this.CommandTimeout = 3600;
		}

		//--------------------------------------------------------------------------------------------------
		public DateTime GetDate()
		{
			//ExecuteQuery<System.DateTime>(
			foreach (DateTime time in (ExecuteQuery<System.DateTime>("SELECT GETDATE()", new object[]{})))
				return time;

			return DateTime.Now;
		}

		/// restituisce le estensioni text compatible lette dalla tabella DMS_TextExtensions		
		//--------------------------------------------------------------------------------------------------
		public List<string> TextExtensions
		{
			get
			{
				var exts = from e in DMS_TextExtensions
							select e.ExtensionType;

				return exts.ToList();
			}
		}

		//--------------------------------------------------------------------------------------------------
		public bool ExistsInDMS_ERPDocumentBarcodes(string barcode)
		{
			var barcodes = from b in DMS_ErpDocBarcodes
						   where  b.Barcode == barcode
						   select b.Barcode;

			return barcodes != null && barcodes.Any();
		}
	}

	//================================================================================
	public interface ILockable
	{
		string LockKey { get;}
		string TableName { get; }
	}

	//================================================================================
	public partial class DMS_ArchivedDocSearchIndex : ILockable
	{
		public string LockKey { get { return string.Format("{0}:{1}", ArchivedDocID.ToString(), SearchIndexID.ToString()); } }
		public string TableName { get { return "DMS_ArchivedDocSearchIndex"; } }

		// WHERE DMS_ArchivedDocSearchIndex.DMS_SearchFieldIndex.FieldName == fieldName AND 
		//(DMS_ArchivedDocSearchIndex.DMS_SearchFieldIndex.FieldValue == keyword1 ||DMS_ArchivedDocSearchIndex.DMS_SearchFieldIndex.FieldValue == keyword2 || DMS_ArchivedDocSearchIndex.DMS_SearchFieldIndex.FieldValue == keyword3...)
		//--------------------------------------------------------------------------------------
		public static Expression<Func<DMS_ArchivedDocSearchIndex, bool>> EqualsTo(string fieldName,  List<String> keywords)
		{
			var checkName = PredicateBuilder.True<DMS_ArchivedDocSearchIndex>();
			var checkValues = PredicateBuilder.False<DMS_ArchivedDocSearchIndex>();
			checkName = checkName.And(p => p.DMS_SearchFieldIndex.FieldName == fieldName);

			foreach (string keyword in keywords)
			{
				//if a value is empty or equal to % then I consider all fields value (is LIKE '%' in SqlAnsi)
				if (string.IsNullOrEmpty(keyword) || string.Compare("%", keyword) == 0)
					return checkName;
				string temp = keyword;
				checkValues = checkValues.Or(p => p.DMS_SearchFieldIndex.FieldValue.Equals(temp));
			}
			checkName = checkName.And(checkValues);
			return checkName;
		}		
	}

	//================================================================================
	public partial class DMS_Attachment : ILockable
	{
		public string LockKey { get { return AttachmentID.ToString(); } }
		public string TableName { get { return "DMS_Attachment"; } }

		//--------------------------------------------------------------------------------
		public static Func<DMSModelDataContext, int, IQueryable<DMS_Attachment>>
			AttachmentById = CompiledQuery.Compile((DMSModelDataContext dc, int id) =>
				from a in dc.DMS_Attachments where a.AttachmentID == id select a);
	}

	//================================================================================
	partial class DMS_AttachmentSearchIndex : ILockable
	{
		public string LockKey { get { return string.Format("{0}:{1}", AttachmentID.ToString(), SearchIndexID.ToString()); } }
		public string TableName { get { return "DMS_AttachmentSearchIndex"; } }

		// WHERE DMS_AttachmentSearchIndex.DMS_SearchFieldIndex.FieldName == fieldName AND 
		//(DMS_AttachmentSearchIndex.DMS_SearchFieldIndex.FieldValue == keyword1 ||DMS_AttachmentSearchIndex.DMS_SearchFieldIndex.FieldValue == keyword2 || DMS_AttachmentSearchIndex.DMS_SearchFieldIndex.FieldValue == keyword3...)
		//--------------------------------------------------------------------------------
		public static Expression<Func<DMS_AttachmentSearchIndex, bool>> EqualsTo(string fieldName, List<String> keywords)
		{
			var checkName = PredicateBuilder.True<DMS_AttachmentSearchIndex>();
			var checkValues = PredicateBuilder.False<DMS_AttachmentSearchIndex>();
			checkName = checkName.And(p => p.DMS_SearchFieldIndex.FieldName == fieldName);

			foreach (string keyword in keywords)
			{
				//if a value is empty or equal to % then I consider all fields value (is LIKE '%' in SqlAnsi)
				if (string.IsNullOrEmpty(keyword) || string.Compare("%", keyword) == 0)
					return checkName;
				string temp = keyword;
				checkValues = checkValues.Or(p => p.DMS_SearchFieldIndex.FieldValue.Equals(temp));
			}
			checkName = checkName.And(checkValues);
			return checkName;
		}
	}

	//================================================================================
	public partial class DMS_Collection : ILockable
	{
		public string LockKey { get { return CollectionID.ToString(); } }
		public string TableName { get { return "DMS_Collection"; } }
	}

	//================================================================================
	public partial class DMS_CollectionsField : ILockable
	{
		public string LockKey { get { return string.Format("{0}:{1}", FieldName, CollectionID.ToString()); } }
		public string TableName { get { return "DMS_CollectionsField"; } }
	}

	//================================================================================
	public partial class DMS_Collector : ILockable
	{
		public string LockKey { get { return CollectorID.ToString(); } }
		public string TableName { get { return "DMS_Collector"; } }
	}

	//================================================================================
	public partial class DMS_ErpDocument : ILockable
	{
		public string LockKey { get { return ErpDocumentID.ToString(); } }
		public string TableName { get { return "DMS_ErpDocument"; } }
	}

	//================================================================================
	public partial class DMS_Field : ILockable
	{
		public string LockKey { get { return FieldName; } }
		public string TableName { get { return "DMS_Field"; } }
	}

	//================================================================================
	public partial class DMS_IndexesSynchronization : ILockable
	{
		public string LockKey { get { return SynchID.ToString(); } }
		public string TableName { get { return "DMS_IndexesSynchronization"; } }
	}

	//================================================================================
	public partial class DMS_SearchFieldIndex : ILockable
	{
		public string LockKey { get { return SearchIndexID.ToString(); } }
		public string TableName { get { return "DMS_SearchFieldIndex"; } }
	}

	//================================================================================
	public partial class DMS_Setting : ILockable
	{
		public string LockKey { get { return string.Format("{0}:{1}", WorkerID.ToString(), SettingType.ToString()); } }
		public string TableName { get { return "DMS_Setting"; } }
	}

	//================================================================================
	public partial class DMS_ArchivedDocument : ILockable
	{
		public string LockKey { get { return ArchivedDocID.ToString(); } }
		public string TableName { get { return "DMS_ArchivedDocument"; } }

		//--------------------------------------------------------------------------------
		public static Func<DMSModelDataContext, int, IQueryable<DMS_ArchivedDocument>>
			ArchivedDocById = CompiledQuery.Compile((DMSModelDataContext dc, int id) =>
				from a in dc.DMS_ArchivedDocuments where a.ArchivedDocID == id select a);

		// WHERE DMS_ArchivedDocument.TBModifiedID == worker1 OR DMS_ArchivedDocument.TBModifiedID == worker2....
		//--------------------------------------------------------------------------------------
		public static bool ArchivedByWorker(int workerID, List<int> workers)
		{
			bool bok = false;
			foreach (int keyword in workers)
			{
				int temp = keyword;
				bok = workerID == temp || bok;
			}
			return bok;
		}
	}
	
	//================================================================================
	public partial class DMS_FieldProperty : ILockable
	{
		public string LockKey { get { return FieldName; } }
		public string TableName { get { return "DMS_FieldProperty"; } }
	}

	//================================================================================
	public partial class DMS_TextExtension : ILockable
	{
		public string LockKey { get { return ExtensionID.ToString(); } }
		public string TableName { get { return "DMS_TextExtension"; } }
	}

	//================================================================================
	public partial class DMS_SOSConfiguration : ILockable
	{
		public string LockKey { get { return ParamID.ToString(); } }
		public string TableName { get { return "DMS_SOSConfiguration"; } }
	}

	//================================================================================
	public class DistinctFieldNameCollectionsField : IEqualityComparer<DMS_CollectionsField>
	{
		//--------------------------------------------------------------------------------
		public bool Equals(DMS_CollectionsField x, DMS_CollectionsField y)
		{
			if (x == null || y == null)    //optional
				return false;

			else
				return x.FieldName == y.FieldName;
		}

		//--------------------------------------------------------------------------------
		public int GetHashCode(DMS_CollectionsField objAttachSearchIndex)
		{
			return objAttachSearchIndex.FieldName.GetHashCode();
		}
	}

	//================================================================================
	public class DistinctAttachmentIDAttachment : IEqualityComparer<DMS_Attachment>
	{
		//--------------------------------------------------------------------------------
		public bool Equals(DMS_Attachment x, DMS_Attachment y)
		{
			if (x == null || y == null)    //optional
				return false;
			else
				return x.AttachmentID == y.AttachmentID;
		}

		//--------------------------------------------------------------------------------
		public int GetHashCode(DMS_Attachment objAttachSearchIndex)
		{
			return objAttachSearchIndex.AttachmentID.GetHashCode();
		}
	}

	//================================================================================
	public class DistinctAttachmentIDAttachmentSearchIndex : IEqualityComparer<DMS_AttachmentSearchIndex>
	{
		//--------------------------------------------------------------------------------
		public bool Equals(DMS_AttachmentSearchIndex x, DMS_AttachmentSearchIndex y)
		{
			if (x == null || y == null)    //optional
				return false;
			else
				return x.AttachmentID == y.AttachmentID;
		}

		//--------------------------------------------------------------------------------
		public int GetHashCode(DMS_AttachmentSearchIndex objAttachSearchIndex)
		{
			return objAttachSearchIndex.AttachmentID.GetHashCode();
		}
	}

	//================================================================================
	public class DistinctArchivedDocIDArchivedDoc : IEqualityComparer<DMS_ArchivedDocument>
	{
		//--------------------------------------------------------------------------------
		public bool Equals(DMS_ArchivedDocument x, DMS_ArchivedDocument y)
		{
			if (x == null || y == null)    //optional
				return false;
			else
				return x.ArchivedDocID == y.ArchivedDocID;
		}

		//--------------------------------------------------------------------------------
		public int GetHashCode(DMS_ArchivedDocument objAttachSearchIndex)
		{
			return objAttachSearchIndex.ArchivedDocID.GetHashCode();
		}
	}
	
	//================================================================================
	public class DistinctArchivedDocIDArchivedDocSearchIndex : IEqualityComparer<DMS_ArchivedDocSearchIndex>
	{
		//--------------------------------------------------------------------------------
		public bool Equals(DMS_ArchivedDocSearchIndex x, DMS_ArchivedDocSearchIndex y)
		{
			if (x == null || y == null)    //optional
				return false;
			else
				return x.ArchivedDocID == y.ArchivedDocID;
		}

		//--------------------------------------------------------------------------------
		public int GetHashCode(DMS_ArchivedDocSearchIndex objAttachSearchIndex)
		{
			return objAttachSearchIndex.ArchivedDocID.GetHashCode();
		}
	}

	//================================================================================
	public class DistinctSearchFieldIndex : IEqualityComparer<DMS_SearchFieldIndex>
	{
		//--------------------------------------------------------------------------------
		public bool Equals(DMS_SearchFieldIndex x, DMS_SearchFieldIndex y)
		{
			if (x == null || y == null)    //optional
				return false;
			else
				return x.SearchIndexID == y.SearchIndexID;
		}

		//--------------------------------------------------------------------------------
		public int GetHashCode(DMS_SearchFieldIndex objAttachSearchIndex)
		{
			return objAttachSearchIndex.SearchIndexID.GetHashCode();
		}
	}

	//================================================================================
	public class DistinctNameSearchFieldIndex : IEqualityComparer<DMS_SearchFieldIndex>
	{
		//--------------------------------------------------------------------------------
		public bool Equals(DMS_SearchFieldIndex x, DMS_SearchFieldIndex y)
		{
			if (x == null || y == null)    //optional
				return false;
			else
				return x.FieldName == y.FieldName;
		}

		//--------------------------------------------------------------------------------
		public int GetHashCode(DMS_SearchFieldIndex objAttachSearchIndex)
		{
			return objAttachSearchIndex.FieldName.GetHashCode();
		}
	}

	//================================================================================
	public class DistinctDocNamespace : IEqualityComparer<string>
	{
		//--------------------------------------------------------------------------------
		public bool Equals(string x, string y)
		{
			if (x == null || y == null)    //optional
				return false;
			else
				return x.CompareNoCase(y);
		}

		//--------------------------------------------------------------------------------
		public int GetHashCode(string objDocNamespace)
		{
			return objDocNamespace.GetHashCode();
		}
	}
}