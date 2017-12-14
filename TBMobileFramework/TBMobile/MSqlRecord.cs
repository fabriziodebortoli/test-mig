using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace TBMobile
{
	public class TableAttribute : Attribute
	{
		public string Name { get; set; }
	}
	public class ColumnAttribute : Attribute
	{
		public string Name { get; set; }
		public int Length { get; set; }
		public bool SegmentKey { get; set; }
		public bool DescriptionField { get; set; }
	}
	public class MSqlRecord :IDisposable, INotifyPropertyChanged
	{
		private string tableName = "";
		List<MSqlRecordItem> fields = new List<MSqlRecordItem>();
		List<MSqlRecordItem> primaryKeyFields = new List<MSqlRecordItem>();

		public event PropertyChangedEventHandler PropertyChanged;

		protected MSqlRecord()
		{
			Type t = GetType();
			TableAttribute attr = t.GetTypeInfo().GetCustomAttribute<TableAttribute>();
			if (attr != null)
				tableName = attr.Name;

			//transfers properties into sql record items so as they can be easily accessed
			foreach (PropertyInfo pi in t.GetRuntimeProperties())
			{
				ColumnAttribute cAttr = pi.GetCustomAttribute<ColumnAttribute>();
				if (cAttr != null && pi.CanRead && pi.CanWrite)
				{
					MDataObj dataObj = (MDataObj)pi.GetValue(this);
					if (dataObj == null)
					{
						dataObj = (MDataObj) Activator.CreateInstance(pi.PropertyType);
						pi.SetValue(this, dataObj);
					}
					dataObj.PropertyChanged += DataObj_PropertyChanged;
					MSqlRecordItem item = Add(DataModelEntityFieldType.Column, dataObj, cAttr.Name, cAttr.Length, cAttr.SegmentKey, cAttr.DescriptionField);
					item.PropertyName = pi.Name;
				}
			}
		}

		void DataObj_PropertyChanged(object sender, EventArgs e)
		{
			if (PropertyChanged != null)
			{
				MDataObj dataObj = sender as MDataObj;
				if (dataObj == null)
					return;
				MSqlRecordItem item = GetField(dataObj);
				if (item == null)
					return;
				PropertyChangedEventArgs args = new PropertyChangedEventArgs(item.PropertyName);
				PropertyChanged(this, args);
			}
		}
		public List<MSqlRecordItem> Fields { get { return fields; } }
		public bool IsStorable { get; set; }
		public string Name { get { return tableName; } }
		public List<MSqlRecordItem> PrimaryKeyFields { get { return primaryKeyFields; } }
		public MSqlRecordItem Add(DataModelEntityFieldType type, MDataObj dataObj, string name, int length, bool segmentKey, bool isDescriptionField)
		{
			MSqlRecordItem item = new MSqlRecordItem(name, length, this, type, dataObj, segmentKey, isDescriptionField);
			fields.Add(item);
			if (segmentKey)
				primaryKeyFields.Add(item);
			return item;
		}
		public void Assign(MSqlRecord record)
		{
			Assign (record.GetJSONData ());
		}

		internal void Assign(JObject rec)
		{
			foreach (MSqlRecordItem item in fields)
			{
				object val = rec[item.Name];
				if (val != null)
					item.DataObj.AssignFromXmlString(val.ToString());
			}
		}
		/// <summary>
		/// returns the json description of the record values (limited to primary key fields if needed)
		/// </summary>
		/// <param name="onlyPrimary"></param>
		/// <returns></returns>
		internal JObject GetJSONData(bool onlyPrimary = false)
		{
			JObject rec = new JObject();
			foreach (MSqlRecordItem item in fields)
			{
				if (onlyPrimary && !item.IsSegmentKey)
					continue;
				rec[item.Name] = item.DataObj.FormatDataForXml();
			}
			return rec;
		}
		protected void Dispose(bool A_0)
		{
		}

		public void GetCompatibleDataTypes(string columnName, List<MDataType> dataTypes)
		{
		}
//		public MDataObj GetData(string fieldName)
//		{
//			foreach (MSqlRecordItem item in fields)
//				if (item.Name == fieldName)
//					return item.DataObj;
//			return null;
//		} 
		public MSqlRecordItem GetField(MDataObj dataObj)
		{
			foreach (MSqlRecordItem item in fields)
				if (item.DataObj == dataObj)
					return item;
			return null;
		}
		public MSqlRecordItem GetField(string fieldName)
		{
			foreach (MSqlRecordItem item in fields)
				if (item.Name == fieldName)
					return item;
			return null;
		}


		public void Dispose()
		{

		}

		internal MSqlRecord Clone()
		{
			MSqlRecord clone = (MSqlRecord)Activator.CreateInstance(GetType());
			for (int i = 0; i < fields.Count; i++)
			{
				clone.fields[i].DataObj.Assign(fields[i].DataObj);
			}
			return clone;
		}

		internal JToken ToJSONObject()
		{
			JArray ar = new JArray();
			foreach (MSqlRecordItem item in fields)
			{
				ar.Add(item.ToJSONObject());
			}
			return ar;
		}

		internal JToken KeyToJSONObject()
		{
			JArray ar = new JArray();
			foreach (MSqlRecordItem item in primaryKeyFields)
			{
				ar.Add(item.ToJSONObject());
			}
			return ar;
		}
			
		public bool EqualsByKey (MSqlRecord other) 
		{
			bool result = false;
			foreach (MSqlRecordItem item in other.PrimaryKeyFields) {
				foreach (MSqlRecordItem it in primaryKeyFields) {
					if (item.DataObj.Value.Equals (it.DataObj.Value)) {
						result = true;
					} else
						result = false;
				}
			}
			return result;
		}

	}

}
