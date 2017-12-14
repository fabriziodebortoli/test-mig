using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Globalization;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	//=========================================================================
	[Serializable]
	public class IncludePathInfo
	{
		
		private string path;
		private CountryLawInfo lawInfo;
		private string dependencyExpression;
		private bool onActivated = false;
		private DependencyEvaluationStatus depEvalStatus= DependencyEvaluationStatus.NotEvaluated;
		

		public string	Path			{get {return path;}}
		public bool		OnActivated		{get {return onActivated;}}
		public CountryLawInfo	CountryLawInfo			{get {return lawInfo;}}
		public string DependencyExpression{get {return dependencyExpression;}}
		public DependencyEvaluationStatus DepEvalStatus{get {return depEvalStatus;} set {depEvalStatus = value;}}


		public enum DependenciesEvaluation {NoEvaluated, According, NotAccording}
		//---------------------------------------------------------------------
		public IncludePathInfo(string path, CountryLawInfo lawInfo, string depExp, bool onactivated)
		{
			this.path			= path;
			this.lawInfo		= lawInfo;
			dependencyExpression= depExp;
			onActivated			= onactivated;
			
		}

		#region overridden object methods
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is IncludePathInfo))
				return false;

			IncludePathInfo comp = obj as IncludePathInfo;

			return (string.Compare(path, comp.path, true, CultureInfo.InvariantCulture) == 0);
			
		}
		
		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return path.ToLower(CultureInfo.InvariantCulture).GetHashCode();
		}
		#endregion
		
		#region Operators overloads
		//----------------------- Overloading degli operatori -----------------
		public static bool operator == (IncludePathInfo m1, IncludePathInfo m2) 
		{
			return m1.Equals(m2);
		}

		public static bool operator != (IncludePathInfo m1, IncludePathInfo m2)
		{
			return !m1.Equals(m2);
		}
		#endregion
	}

	//=========================================================================
}
