using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBMobile
{
	public enum DataModelEntityFieldType
	{
		Column = 0,
		Variable = 1,
		Parameter = 2,
	}
	public class MSqlRecordItem : IDisposable
	{
		protected MDataType dataObjType;
		protected MSqlRecord record;
		protected DataModelEntityFieldType fieldType;
		MDataObj dataObj;
		private string name;
		private int length;
		private bool isSegmentKey;
		private bool isDescriptionField;

		internal MSqlRecordItem(
			string name, 
			int length, 
			MSqlRecord record, 
			DataModelEntityFieldType type, 
			MDataObj dataObj, 
			bool isSegmentKey,
			bool isDescriptionField)
		{
			this.name = name;
			this.length = length;
			this.record = record;
			this.fieldType = type;
			this.dataObjType = dataObj.DataType;
			this.dataObj = dataObj;
			this.isSegmentKey = isSegmentKey;
			this.isDescriptionField = isDescriptionField;
			dataObj.Owner = this;
		}

		//property name of the corresponding dataobj in the derived class
		public string PropertyName { get; set; }

		public MDataObj DataObj { get { return dataObj; } }
		public MDataType DataObjType { get { return dataObjType; } }
		public DataModelEntityFieldType Type { get { return fieldType; } }
		public bool IsSegmentKey { get { return isSegmentKey; } }
		public bool IsDescriptionField { get { return isDescriptionField; } }
		public int Length { get { return length; } }
		public string Name { get { return name; } }
		public string QualifiedName { get{ return record.Name + "." + name; }}
		public MSqlRecord Record { get { return record; } }

		public void Dispose() { }
		protected void Dispose(bool disposing) { }
		protected void OnValueChanged(object sender, MobileEventArgs args) { }
		protected void OnValueChanging(object sender, MobileEventArgs args) { }


		internal JToken ToJSONObject()
		{
			return new JObject(
				 new JProperty("name", Name),
				 new JProperty("type", fieldType),
				 new JProperty("dataType", (int)dataObjType),
				 new JProperty("key", isSegmentKey),
				 new JProperty("length", length)
				 );
		}

	}
}
