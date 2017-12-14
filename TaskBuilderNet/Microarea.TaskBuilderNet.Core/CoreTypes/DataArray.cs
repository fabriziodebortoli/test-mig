using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Core.CoreTypes
{
	/// <summary>
	/// Descrizione di riepilogo per DataArray.
	/// </summary>
	//=========================================================================
	[DataContract]
	[KnownType(typeof(DataObj))]
	[KnownType(typeof(DataEnum))]
	[Serializable]
	public class DataArray : DataObj
	{
		private const string baseTypeTag = "BaseType";
		private const string elementsTag = "Elements";
		private const string attachedArraysTag = "AttachedArrays";
		private const string sortDescendingTag = "SortDescending";

		private ArrayList elements;
		private string baseType;
		private bool sortDescending;
		private ArrayList attachedArrays;

		//---------------------------------------------------------------------
		protected override object GetValue()
		{
			return null;
		}
		
		protected override void SetValue(object value)
		{
		}


		//---------------------------------------------------------------------
		public  int Count { get { return elements.Count; } }

		//---------------------------------------------------------------------
		[DataMember]
		public  string BaseType
		{
			get { return baseType; }
			set
			{
				OnPropertyChanging("BaseType");

				if (IsValueLocked)
					return;

				baseType = value;

				IsModified = true;
				IsDirty = true;

				OnPropertyChanged("BaseType");
			}
		}
		//---------------------------------------------------------------------
		[DataMember]
		public ArrayList Elements
		{
			get
			{
				return elements;
			}
			set
			{
				OnPropertyChanging("Elements");

				if (IsValueLocked)
					return;

				elements = value;

				IsModified = true;
				IsDirty = true;

				OnPropertyChanged("Elements");
			}
		}

		//---------------------------------------------------------------------
		public override IDataType DataType { get { return CoreTypes.DataType.Array; } }

		//---------------------------------------------------------------------
		public DataArray()
		{
			this.baseType = string.Empty;
			elements = new ArrayList();
			attachedArrays = new ArrayList();
			// inizializzato a false dal runtime
			//sortDescending = false;
		}
		
		//---------------------------------------------------------------------
		public DataArray(string	baseType)
		{
			this.baseType = baseType;
			elements = new ArrayList();
			attachedArrays = new ArrayList();
			// inizializzato a false dal runtime
			//sortDescending = false;
		}

		//---------------------------------------------------------------------
		public void Assign(DataArray dataArray)
		{
			if (dataArray == null)
				return;

			this.baseType = dataArray.baseType;
			this.elements = dataArray.elements.Clone() as ArrayList;
		}

        //---------------------------------------------------------------------
        public bool Copy(DataArray dataArray)
        {
            Assign(dataArray);
            return true;
        }

        //---------------------------------------------------------------------
        public bool Append(DataArray dataArray)
        {
            if (dataArray == null)
                return false;
            if (this.baseType != dataArray.baseType)
                return false;

            for (int i = 0; i < dataArray.Elements.Count; i++)
                Add(dataArray.Elements[i]);

            return true;
        }

		//---------------------------------------------------------------------
		public override void Clear()
		{
			Clear(true);
		}

		//---------------------------------------------------------------------
		public override void Clear(bool valid) 
		{ 
			if (IsValueLocked)
				return;

			base.Clear(valid);

			elements.Clear();
		}

		//---------------------------------------------------------------------
		public bool Attach(DataArray ar)
		{
			if (ar == null)
				return false;

			if (ar == this) 
				return false; //previene ricorsione

			if (ar.attachedArrays.Count > 0)
				foreach (DataArray a in ar.attachedArrays)
				{
					if (a == this)
						return false;	//previene ricorsione
				}

			return attachedArrays.Add(ar) >= 0;
		}

		//---------------------------------------------------------------------
		public void Detach()
		{
			attachedArrays.Clear();
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return elements.GetHashCode();
		}

		//---------------------------------------------------------------------
		public static bool operator !=(DataArray e1, DataArray e2)
		{
			return !(e1 == e2);
		}

		//---------------------------------------------------------------------
		public static bool operator ==(DataArray e1, DataArray e2)
		{
			if (Object.ReferenceEquals(e1, e2))
				return true;

			if (Object.ReferenceEquals(null, e1) || Object.ReferenceEquals(null, e2))
				return false;

			return e1.Equals(e2);
		}

		//---------------------------------------------------------------------
		public override object Clone()
		{
			DataArray a = new DataArray(baseType);

			a.Assign(this);

			return a;
		}

        //---------------------------------------------------------------------
        public int Add(object obj)
        {
            if (!ObjectHelper.Compatible(obj, this.BaseType))
            {
                System.Diagnostics.Debug.Fail("Array - add element with wrong data type");
                throw (new ObjectHelperException(CoreTypeStrings.IncompatibleType));
                //return -1;
            }
            return  this.Elements.Add(obj);
        }

        //---------------------------------------------------------------------
        public void Insert(int index, object obj)
        {
            if (!ObjectHelper.Compatible(obj, this.BaseType))
            {
                System.Diagnostics.Debug.Fail("Array - add element with wrong data type");
                throw (new ObjectHelperException(CoreTypeStrings.IncompatibleType));
                //return;
            }

            if (index == this.Count)
            {
                Add(obj);
                return;
            }
            if (index > this.Count)
            {
                SetAtGrow(index, obj);
                return;
            }

            this.Elements.Insert(index, obj);
        }

        //---------------------------------------------------------------------
        public object Remove(int index)
        {
            if (index >= this.Count)
            {
                return null;
            }
            object obj = this.Elements[index];
            this.Elements.RemoveAt(index);
            return obj;
        }

		//---------------------------------------------------------------------
		public void SetAtGrow(int index, object obj)
		{
            if (!ObjectHelper.Compatible(obj, this.BaseType))
            {
                System.Diagnostics.Debug.Fail("Array - add element with wrong data type");
                throw (new ObjectHelperException(CoreTypeStrings.IncompatibleType));
                //return;
            }

			if (index >= this.Count) 
			{
				for (int j = this.Count; j <= index; j++)
					this.Elements.Add(ObjectHelper.CreateObject(this.BaseType));
			}

			object elem = this.Elements[index];

			ObjectHelper.Assign(ref elem, obj);	

			this.Elements[index] = elem;
		}

        //---------------------------------------------------------------------
        public object GetAt(int index) 
        {
            if (index >= this.Count)
            {
                return null;
            }

            return this.Elements[index];
         }

		//---------------------------------------------------------------------
		public int Find(object obj, int startIndex = 0)
		{
			if (obj == null || elements.Count == 0)
				return -1;

			if (startIndex < 0 || startIndex > (elements.Count - 1))
				throw new ArgumentOutOfRangeException("startIndex");

			Object valBaseType = ObjectHelper.CreateObject(this.BaseType);
			ObjectHelper.Assign(ref valBaseType, obj);
			return (this.elements.IndexOf(valBaseType, startIndex));
		}

		//---------------------------------------------------------------------
		public bool Sort(bool descending = false, int start = 0, int end = -1) 
		{ 
			bool ok = true;
			if (attachedArrays.Count == 0 && start == 0 && (end == -1 || end == (this.elements.Count - 1)))
			{
				this.elements.Sort();	
				if (descending)
					this.elements.Reverse();
			}
			else
			{
				ok = QuickSort(descending, start, end);
			}
			return ok;
		}

		// Compatibile al formato Soap (vedi la classe System.Xml.XmlConvert)
		//---------------------------------------------------------------------
		public string XmlConvertToString()
		{ 
			StringBuilder sbDataArray = new StringBuilder();
			for (int i = 0; i < Count; i++)
			{
				sbDataArray.AppendFormat("<{0}>", baseType );
				sbDataArray.Append(SoapTypes.To(elements[i]));
				sbDataArray.AppendFormat("</{0}>", baseType);
			}
			return sbDataArray.ToString();
		}

		// Compatibile al formato Soap (vedi la classe System.Xml.XmlConvert)
		//---------------------------------------------------------------------
		public static DataArray XmlConvertToDataArray(string from)
		{
			return new DataArray();
		}

		//---------------------------------------------------------------------
		public static DataArray XmlConvertToDataArray(string from, string baseType)
		{
			XmlDocument dom = new XmlDocument();
			dom.LoadXml(from);
			DataArray values = new DataArray(baseType);
			
			foreach (XmlNode arrayElementNode in dom.FirstChild.ChildNodes)
			{
				values.Elements.Add(SoapTypes.From(arrayElementNode.FirstChild.Value, baseType));
			}
			
			return values;
		}

		//---------------------------------------------------------------------
		// Sposta di posizione i puntatori all'interno dell' Array
		private void Swap (int index1, int index2)
		{
			object pTemp = this.Elements[index1];
			this.Elements[index1] = this.Elements[index2];
			this.Elements[index2] = pTemp;

			if (attachedArrays.Count > 0)
				foreach (DataArray a in attachedArrays)
				{
					a.Swap(index1, index2);	//eventuale ricorsione
				}
		}

		//---------------------------------------------------------------------
		private int QSPartitionIt (int left,	int right, object pPivot)
		{
			int LeftPtr		= left - 1;
			int RightPtr	= right;
	
			for (;;)
			{
				//Cerco all'interno dell'array il massimo elemento
				while (((IComparable)(this.Elements[++LeftPtr])).CompareTo(pPivot) == -1);

				//Cerco all'interno dell'array il minimo elemento
				while (RightPtr > 0 && ((IComparable)(this.Elements[--RightPtr])).CompareTo(pPivot) == 1);

				if (LeftPtr >= RightPtr)
					break;
				else
					Swap (LeftPtr, RightPtr);
			}
			Swap (LeftPtr, right);
	
			return LeftPtr;
		}

		//---------------------------------------------------------------------
		// L'algoritmo di quicksort, separa una array in due segmenti di array, e applica 
		//	ricorsivamente l'ordinamento sui due segmenti.
		private void QuickSort (int left, int right)
		{
			// Se la dimensione dell'array e' <= 1, allora vuol dire che l'array e'ordinato
			if ((right - left) <= 0)
				return;
			else
			{
				// Il pivot e' l'elemento che si trova in fondo all'array o 
				// al segmento di array
				object pPivot = this.Elements[right];  

				int Partition = QSPartitionIt (left, right, pPivot);
		
				// applico l'algoritmo di ordinamento ai due segmenti di array
				QuickSort (left, Partition-1);
				QuickSort (Partition+1, right);
			}
		}

		//---------------------------------------------------------------------
		public bool QuickSort (bool descending, int start = 0, int end = -1)
		{
			if (attachedArrays.Count > 0)
				foreach (DataArray a in attachedArrays)
				{
					if (a.Count != this.Count)
						return false;	//eventuale ricorsione
				}

			sortDescending = descending;
            if (end == -1)
                end = this.Elements.Count - 1;

            QuickSort (start, end);
			return true;
		}

		#region IComparable Members

		//---------------------------------------------------------------------
		//solo per uniformità con gli altri tipi di dato
		public override int CompareTo(object obj)
		{
			if (Object.ReferenceEquals(obj ,null))
				return 1;

			DataArray dataArray = obj as DataArray;
			if (Object.ReferenceEquals(dataArray, null))
				throw new ArgumentException(CoreTypeStrings.InvalidArgType);

			return this.elements.Count.CompareTo(dataArray.elements.Count);
		}

		#endregion

		public override string ToString(int minLen, int maxLen)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override bool IsEmpty()
		{
			return (elements == null || elements.Count == 0);
		}

		//---------------------------------------------------------------------
		public override string GetXmlType(bool soapType)
		{
			System.Diagnostics.Debug.Assert(false);
			return string.Empty;
		}

		//---------------------------------------------------------------------
		public override string GetXmlType()
		{
			return GetXmlType(true);
		}

		//---------------------------------------------------------------------
		public override string FormatDataForXml(bool soapType)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override void AssignFromXmlString(string xmlFragment)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override string FormatDataForXml()
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override bool IsEqual(IDataObj dataObj)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override bool IsLessThan(IDataObj dataObj)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override bool IsLessEqualThan(IDataObj dataObj)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override bool IsGreaterThan(IDataObj dataObj)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override bool IsGreaterEqualThan(IDataObj dataObj)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override bool Equals(IDataObj other)
		{
			if (!(other is DataArray))
				return false;
			DataArray ar = (DataArray)other;
			if (ar.Count != Count)
				return false;

			for (int i = 0; i < Count; i++)
				if (!Elements[i].Equals(ar.Elements[i]))
					return false;
			return true;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(obj, null))
				return false;

			DataArray itemDataEnum = obj as DataArray;
			if (Object.ReferenceEquals(itemDataEnum, null))
				throw new ArgumentException(CoreTypeStrings.InvalidArgType);

			if (String.Compare(this.baseType, itemDataEnum.baseType) != 0)
				return false;

			if (this.elements.Count != itemDataEnum.elements.Count)
				return false;

			for (int i = 0; i < this.elements.Count; i++)
				if (this.elements[i] != itemDataEnum.elements[i])
					return false;

			return true;
		}

		//---------------------------------------------------------------------
		public override int CompareTo(IDataObj other)
		{
			throw new NotImplementedException();
		}
        //---------------------------------------------------------------------
        public object CalcSum()
        {
            throw new NotImplementedException();
        }
        //---------------------------------------------------------------------
        public object GetMinElem()
        {
            throw new NotImplementedException();
        }
        //---------------------------------------------------------------------
        public object GetMaxElem()
        {
            throw new NotImplementedException();
        }

	}
}
