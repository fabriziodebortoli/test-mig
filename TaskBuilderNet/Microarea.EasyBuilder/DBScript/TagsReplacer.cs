using System.IO;
using System.Text;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Summary description for Tag.
	/// </summary>
	//================================================================================
	public class Tag
	{
		private string[] mTags = 
		{	
			"$Comment", 
			"$TablePhysicalName", 
			"$TableLogicalName", 
			"$TableHeader", 
			"$TableSource", 
			"$ShortTableHeader", 
			"$ShortTableSource", 
			"$FieldPhysicalName", 
			"$FieldLogicalName", 
			"$PrimaryKeyConstraint", 
			"$Constraint", 
			"$Type",
			"$DefaultValue",
			"$UpdateScript",
			"$IndexFields",
			"$IndexName"
	};

		private bool mOracle = false;
		private bool mAlsoField = true;

		private string mComment = string.Empty;
		private string mCommentText = string.Empty;
		private string mTablePhysicalName = string.Empty;
		private string mTablePhysicalNameOracle = string.Empty;
		private string mTableLogicalName = string.Empty;
		private string mShortTableHeader = string.Empty;
		private string mShortTableSource = string.Empty;
		private string mTableHeader = string.Empty;
		private string mTableSource = string.Empty;
		private string mFieldPhysicalName = string.Empty;
		private string mFieldPhysicalNameOracle = string.Empty;
		private string mFieldLogicalName = string.Empty;
		private string mPrimaryKeyConstraint = string.Empty;
		private string mPrimaryKeyConstraintOracle = string.Empty;
		private string mConstraint = string.Empty;
		private string mConstraintOracle = string.Empty;
		private string mType = string.Empty;
		private string mTypeOracle = string.Empty;
		private string mDefaultValue = string.Empty;
		private string mDefaultValueOracle = string.Empty;
		private string mUpdateScript = string.Empty;
		private string mUpdateScriptOracle = string.Empty;
		private string mIndexFields = string.Empty;
		private string mIndexFieldsOracle = string.Empty;
		private string mIndexName = string.Empty;
		private string mIndexNameOracle = string.Empty;

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public Tag()
		{
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public Tag(string aComment)
		{
			mCommentText = aComment;
			if (mCommentText != string.Empty)
				mComment = " // " + aComment;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string Comment
		{
			get
			{
				return mComment;
			}

			set
			{
				mComment = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string CommentText
		{
			get
			{
				return mCommentText;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool Oracle
		{
			get
			{
				return mOracle;
			}

			set
			{
				mOracle = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool AlsoField
		{
			get
			{
				return mAlsoField;
			}

			set
			{
				mAlsoField = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string TablePhysicalName
		{
			get
			{
				return mTablePhysicalName;
			}

			set
			{
				mTablePhysicalName = value;
				mPrimaryKeyConstraint = "PK_" + mTablePhysicalName;
				mTablePhysicalNameOracle = mTablePhysicalName.ToUpper();
				mPrimaryKeyConstraintOracle = mPrimaryKeyConstraint.ToUpper();
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string TablePhysicalNameOracle
		{
			get
			{
				return mTablePhysicalNameOracle;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string TableLogicalName
		{
			get
			{
				return mTableLogicalName;
			}

			set
			{
				mTableLogicalName = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string TableHeader
		{
			get
			{
				return mTableHeader;
			}

			set
			{
				mTableHeader = value;
				mShortTableHeader = Path.GetFileName(mTableHeader);
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string TableSource
		{
			get
			{
				return mTableSource;
			}

			set
			{
				mTableSource = value;
				mShortTableSource = Path.GetFileName(mTableSource);
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string ShortTableHeader
		{
			get
			{
				return mShortTableHeader;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string ShortTableSource
		{
			get
			{
				return mShortTableSource;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string FieldPhysicalName
		{
			get
			{
				return mFieldPhysicalName;
			}

			set
			{
				mFieldPhysicalName = value;
				mFieldPhysicalNameOracle = mFieldPhysicalName.ToUpper();
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string FieldPhysicalNameOracle
		{
			get
			{
				return mFieldPhysicalNameOracle;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string FieldLogicalName
		{
			get
			{
				return mFieldLogicalName;
			}

			set
			{
				mFieldLogicalName = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string Constraint
		{
			get
			{
				return mConstraint;
			}

			set
			{
				mConstraint = value;
				mConstraintOracle = mConstraint.ToUpper();
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string ConstraintOracle
		{
			get
			{
				return mConstraintOracle;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string PrimaryKeyConstraint
		{
			get
			{
				return mPrimaryKeyConstraint;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string PrimaryKeyConstraintOracle
		{
			get
			{
				return mPrimaryKeyConstraintOracle;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string Type
		{
			get
			{
				return mType;
			}

			set
			{
				mType = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string TypeOracle
		{
			get
			{
				return mTypeOracle;
			}

			set
			{
				mTypeOracle = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string DefaultValue
		{
			get
			{
				return mDefaultValue;
			}

			set
			{
				mDefaultValue = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string DefaultValueOracle
		{
			get
			{
				return mDefaultValueOracle;
			}

			set
			{
				mDefaultValueOracle = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string UpdateScript
		{
			get
			{
				return mUpdateScript;
			}

			set
			{
				mUpdateScript = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string UpdateScriptOracle
		{
			get
			{
				return mUpdateScriptOracle;
			}

			set
			{
				mUpdateScriptOracle = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string IndexFields
		{
			get
			{
				return mIndexFields;
			}

			set
			{
				mIndexFields = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string IndexFieldsOracle
		{
			get
			{
				return mIndexFieldsOracle;
			}

			set
			{
				mIndexFieldsOracle = value;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string IndexName
		{
			get
			{
				return mIndexName;
			}

			set
			{
				mIndexName = value;
				mIndexNameOracle = mIndexName.ToUpper();
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string IndexNameOracle
		{
			get
			{
				return mIndexNameOracle;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string Replace(string aString)
		{
			aString = aString.Replace(mTags[0], mComment);
			if (mOracle)
				aString = aString.Replace(mTags[1], mTablePhysicalNameOracle);
			else
				aString = aString.Replace(mTags[1], mTablePhysicalName);
			aString = aString.Replace(mTags[2], mTableLogicalName);
			aString = aString.Replace(mTags[3], mTableHeader);
			aString = aString.Replace(mTags[4], mTableSource);
			aString = aString.Replace(mTags[5], mShortTableHeader);
			aString = aString.Replace(mTags[6], mShortTableSource);
			if (mAlsoField)
			{
				if (mOracle)
					aString = aString.Replace(mTags[7], mFieldPhysicalNameOracle);
				else
					aString = aString.Replace(mTags[7], mFieldPhysicalName);
				aString = aString.Replace(mTags[8], mFieldLogicalName);
			}
			if (mOracle)
			{
				aString = aString.Replace(mTags[9], mPrimaryKeyConstraintOracle);
				aString = aString.Replace(mTags[10], mConstraintOracle);
				aString = aString.Replace(mTags[11], mTypeOracle);
				aString = aString.Replace(mTags[12], DefaultValueOracle);
				aString = aString.Replace(mTags[13], mUpdateScriptOracle);
				aString = aString.Replace(mTags[14], mIndexFieldsOracle);
				aString = aString.Replace(mTags[15], mIndexNameOracle);
			}
			else
			{
				aString = aString.Replace(mTags[9], mPrimaryKeyConstraint);
				aString = aString.Replace(mTags[10], mConstraint);
				aString = aString.Replace(mTags[11], mType);
				aString = aString.Replace(mTags[12], mDefaultValue);
				aString = aString.Replace(mTags[13], mUpdateScript);
				aString = aString.Replace(mTags[14], mIndexFields);
				aString = aString.Replace(mTags[15], mIndexName);
			}
			return aString;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public int GetCount()
		{
			return mTags.GetUpperBound(0);
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string Get(int i)
		{
			return mTags[i];
		}
	}
}
//================================================================================
internal class SignalStreamWriter : StreamWriter
{
	string fileName;
	//--------------------------------------------------------------------------------
	public SignalStreamWriter(string fileName, bool append = false)
		: base(fileName, append, Encoding.UTF8)
	{
		this.fileName = fileName;
	}

	//--------------------------------------------------------------------------------
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(fileName);
	}
}
