using System;
using System.Globalization;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	//=========================================================================
	public enum SolutionType {None, VerticalIntegration, Embedded, StandAlone};

	//=========================================================================
	[Serializable]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.microarea.it/")]
	public class IntegratedSolution : IComparable
	{
		public string	Username;
		public string	CompanyCode;
		public string	SolutionName;
		public bool		IsActive;
		public bool		IsNew;
		public string	Description;
		public string	WebServiceUrl;
		public SolutionType SolutionType;
		public string[]	ProductCodes;
		public string	SelectedFreeChars;
		
		//---------------------------------------------------------------------
		public IntegratedSolution(
			string username,
			string companyCode,
			string solutionName,
			bool isActive,
			bool isNew,
			string description,
			string webServiceUrl,
			SolutionType solutionType,
			string[] productCodes,
			string selectedFreeChars
			)
		{
			Username		= username;
			CompanyCode		= companyCode;
			SolutionName	= solutionName;
			IsActive		= isActive;
			IsNew			= isNew;
			Description		= description;
			WebServiceUrl	= webServiceUrl;
			SolutionType	= solutionType;
			ProductCodes	= productCodes;
			SelectedFreeChars = selectedFreeChars;
		}

		//---------------------------------------------------------------------
		public IntegratedSolution()
		{}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return SolutionName;
		}

		/// <summary>
		/// Returns a value indicating if the solution name is that passed
		/// </summary>
		//---------------------------------------------------------------------
		public bool HasName(string solutionName)
		{
			return String.Compare(SolutionName, solutionName, true, CultureInfo.InvariantCulture) == 0;
		}

		#region IComparable Members

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			IntegratedSolution aSolution = obj as IntegratedSolution;

			if (aSolution == null)
				throw new ArgumentException("'obj' is not an 'IntegratedSolution'");

			return this.SolutionName.CompareTo(aSolution.SolutionName);
		}

		#endregion
	}
}
