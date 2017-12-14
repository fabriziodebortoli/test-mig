using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace Microarea.Library.TBWizardProjects
{
	/// <summary>
	/// Summary description for InjectionPoint.
	/// </summary>
	public class InjectionPoint
	{
		#region InjectionPoint public constant members
		
		public const string	BeginInjectionPointMarker = "//TBWIZ-INJECT{";
		public const string	EndInjectionPointMarker = "//TBWIZ-INJECT}";

		#endregion

		#region InjectionPoint private members

		private string	fileName = String.Empty;
		private string	header = String.Empty;
		private string	codeInside = String.Empty;
		private int		lineNumber = -1;

		#endregion

		//---------------------------------------------------------------------------
		public InjectionPoint(string aFileName, string aHeader, string aCodeInside, int aLineNumber)
		{
			fileName = aFileName;
			header = aHeader;
			codeInside = aCodeInside;
			lineNumber = aLineNumber;
		}

		//---------------------------------------------------------------------------
		public InjectionPoint(string aFileName, string aHeader, int aLineNumber): this(aFileName, aHeader, String.Empty, aLineNumber)
		{
		}

		//---------------------------------------------------------------------------
		public InjectionPoint(string aFileName, string aHeader): this(aFileName, aHeader, String.Empty, -1)
		{
		}

		#region InjectionPoint overridden methods

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is InjectionPoint))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(fileName,((InjectionPoint)obj).FileName, true) == 0 &&
				String.Compare(header,	((InjectionPoint)obj).Header) == 0 &&
				String.Compare(codeInside,((InjectionPoint)obj).CodeInside) == 0 &&
				lineNumber == ((InjectionPoint)obj).LineNumber
				);
		}
		
		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		#endregion

		#region InjectionPoint public properties
		
		//---------------------------------------------------------------------------
		public string	FileName		{ get { return fileName; } }
		public string	Header		{ get { return header; } }
		public string	CodeInside	{ get { return codeInside; } }
		public int		LineNumber	{ get { return lineNumber; } }

		#endregion

		#region InjectionPoint public methods

		//---------------------------------------------------------------------------
		public bool IsValidCodeLine(string aCodeLine)
		{
			if (aCodeLine == null)
				return false;

			// Non è consentito innestare punti di iniezione.
			string cleanedCodeLine = aCodeLine.TrimStart(new char[] {' ','\t'}).Trim();
			return 
				!cleanedCodeLine.StartsWith(BeginInjectionPointMarker) &&
				!cleanedCodeLine.StartsWith(EndInjectionPointMarker);
		}

		//---------------------------------------------------------------------------
		public void AddCodeLine(string aCodeLine)
		{
			if (!IsValidCodeLine(aCodeLine))
				return;
				
			if (codeInside == null)
				codeInside = String.Empty;

			codeInside += aCodeLine + "\n";
		}

		//------------------------------------------------------------------------------
		public static InjectionPointsCollection GetInjectionPoints(System.IO.FileStream aFileStream, bool onlyWithCode)
		{
			if (aFileStream == null || !aFileStream.CanRead)
				return null;

			InjectionPointsCollection injectionPoints = null;
			System.IO.StreamReader fileStreamReader = null;

			try
			{
				fileStreamReader = new System.IO.StreamReader(aFileStream);
				if (fileStreamReader == null)
					return null;

				fileStreamReader.BaseStream.Seek(0, SeekOrigin.Begin);
				string line = null;
				int currentLineNumber = 0;
				InjectionPoint	currentInjectionPoint = null;
				
				// ReadLine returns the next line from the input stream, or a null reference if the end of the input stream is reached.
				while ((line = fileStreamReader.ReadLine()) != null)
				{
					string currentLine = line.TrimStart(new char[] {' ','\t'}).Trim();
					currentLineNumber ++;
 
					if (currentInjectionPoint != null)
					{
						if (currentLine.StartsWith(InjectionPoint.EndInjectionPointMarker))
						{
							if 
								(
								!onlyWithCode ||
								(currentInjectionPoint.CodeInside != null && currentInjectionPoint.CodeInside.Length > 0)
								)
							{
								if (injectionPoints == null)
									injectionPoints = new InjectionPointsCollection();
								injectionPoints.Add(currentInjectionPoint);
							}
							currentInjectionPoint = null;
							continue;
						}
						currentInjectionPoint.AddCodeLine(line);
					}
					else if (currentLine.StartsWith(InjectionPoint.BeginInjectionPointMarker))
					{
						string injectionPointHeader = currentLine.Substring(InjectionPoint.BeginInjectionPointMarker.Length);
						if (injectionPointHeader == null || injectionPointHeader.Length == 0)
							continue;
						
						currentInjectionPoint = new InjectionPoint(aFileStream.Name, injectionPointHeader, currentLineNumber);
					}
				}

				if (currentInjectionPoint != null)
				{ 
					// Si è terminata la lettura del file con un punto di iniezione ancora "aperto",
					// Nel senso che non si è riusciti a trovare la corrispondente sequenza di
					// terminazione prima della fine del file
					throw new TBWizardException(String.Format(TBWizardProjectsStrings.NotEndedInjectionPointErrorMsg, currentInjectionPoint.FileName, currentInjectionPoint.Header));
				}

				return injectionPoints;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in InjectionPoint.GetInjectionPoints: " + exception.Message);

				throw new TBWizardException(exception.Message);
			}
			finally
			{
				if (fileStreamReader != null)
					fileStreamReader.Close();
			}
		}

		//------------------------------------------------------------------------------
		public static InjectionPointsCollection GetInjectionPoints(System.IO.FileStream aFileStream)
		{
			return GetInjectionPoints(aFileStream, false);
		}
		
		#endregion
	}

	#region InjectionPointsCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for InjectionPointsCollection.
	/// </summary>
	public class InjectionPointsCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public InjectionPointsCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is InjectionPoint))
					throw new NotSupportedException();

				this[index] = (InjectionPoint)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is InjectionPoint))
				throw new NotSupportedException();

			return this.Contains((InjectionPoint)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((InjectionPoint)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is InjectionPoint))
				throw new NotSupportedException();

			return this.IndexOf((InjectionPoint)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is InjectionPoint))
				throw new NotSupportedException();

			Insert(index, (InjectionPoint)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is InjectionPoint))
				throw new NotSupportedException();

			Remove((InjectionPoint)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public InjectionPoint this[int index]
		{
			get {  return (InjectionPoint)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (InjectionPoint)value; 
			}
		}

		//---------------------------------------------------------------------------
		public InjectionPoint[] ToArray()
		{
			return (InjectionPoint[])InnerList.ToArray(typeof(InjectionPoint));
		}

		//---------------------------------------------------------------------------
		public int Add(InjectionPoint aInjectionPointToAdd)
		{
			if (Contains(aInjectionPointToAdd))
				return IndexOf(aInjectionPointToAdd);

			return InnerList.Add(aInjectionPointToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(InjectionPointsCollection aInjectionPointsCollectionToAdd)
		{
			if (aInjectionPointsCollectionToAdd == null || aInjectionPointsCollectionToAdd.Count == 0)
				return;

			foreach (InjectionPoint aInjectionPointToAdd in aInjectionPointsCollectionToAdd)
				Add(aInjectionPointToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, InjectionPoint aInjectionPointToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aInjectionPointToInsert))
				return;

			InnerList.Insert(index, aInjectionPointToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(InjectionPoint beforeInjectionPoint, InjectionPoint aInjectionPointToInsert)
		{
			if (beforeInjectionPoint == null)
				Add(aInjectionPointToInsert);

			if (!Contains(beforeInjectionPoint))
				return;

			if (Contains(aInjectionPointToInsert))
				return;

			Insert(IndexOf(beforeInjectionPoint), aInjectionPointToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(InjectionPoint aInjectionPointToRemove)
		{
			if (!Contains(aInjectionPointToRemove))
				return;

			InnerList.Remove(aInjectionPointToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(InjectionPoint aInjectionPointToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aInjectionPointToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(InjectionPoint aInjectionPointToSearch)
		{
			if (!Contains(aInjectionPointToSearch))
				return -1;
			else
				return InnerList.IndexOf(aInjectionPointToSearch);
		}
	
		//---------------------------------------------------------------------------
		public bool Contains(string aHeader)
		{
			if (this.Count == 0)
				return false;

			foreach (InjectionPoint aInjectionPoint in this)
			{
				if (String.Compare(aHeader,	aInjectionPoint.Header) == 0)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public InjectionPoint GetInjectionPoint(string aFileName, string aHeader)
		{
			if (this.Count == 0)
				return null;

			foreach (InjectionPoint aInjectionPoint in this)
			{
				if 
					(
					String.Compare(aFileName, aInjectionPoint.FileName, true) == 0 &&
					String.Compare(aHeader,	aInjectionPoint.Header) == 0
					)
					return aInjectionPoint;
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		public InjectionPoint GetNamedInjectionPoint(string aHeader)
		{
			if (this.Count == 0)
				return null;

			foreach (InjectionPoint aInjectionPoint in this)
			{
				if (String.Compare(aHeader, aInjectionPoint.Header) == 0)
					return aInjectionPoint;
			}
			return null;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is InjectionPointsCollection))
				return false;

			if (obj == this)
				return true;

			if (((InjectionPointsCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!InjectionPoint.Equals(this[i], ((InjectionPointsCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	}

	#endregion
}
