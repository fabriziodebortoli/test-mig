using System;
using System.Globalization;
using System.IO;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	//=========================================================================
	[Serializable]
	public class ModuleInfo : IModule, IDeployModule
	{
		public enum ModuleType {Module, Functionality}

		private string name;
		private string container;
		private string application;
		private ModuleType type;
		private CountryLawInfo lawInfo;
        private SNTypeLawInfo lawInfosn = null;
		private string dependencyExpression;
		private DependencyEvaluationStatus depEvalStatus= DependencyEvaluationStatus.NotEvaluated;


		public string	Name			{get {return name;}}
		public string	Container		{get {return container;}}
		public string	Application		{get {return application;}}
		public ModuleType Type			{get {return type;}}
		public CountryLawInfo	CountryLawInfo			{get {return lawInfo;}}
		public string DependencyExpression{get {return dependencyExpression;}}
		public DependencyEvaluationStatus DepEvalStatus{get {return depEvalStatus;} set {depEvalStatus = value;}}

        public SNTypeLawInfo LawInfoSN { get { return lawInfosn; } set { lawInfosn = value; } }

		//---------------------------------------------------------------------
		public ModuleInfo(string name, string application, string container, ModuleType type, CountryLawInfo lawInfo, string depExp)
		{
			this.name			= name;
			this.application	= application;
			this.container		= container;
			this.type			= type;
			this.lawInfo		= lawInfo;
			dependencyExpression= depExp;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}{3}{1}{3}{2}", container, application, name, Path.DirectorySeparatorChar);
		}

		#region overridden object methods
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ModuleInfo))
				return false;

			ModuleInfo comp = obj as ModuleInfo;

			if (string.Compare(name, comp.name, true, CultureInfo.InvariantCulture) == 0 &&
				string.Compare(application, comp.application, true, CultureInfo.InvariantCulture) == 0 &&
				string.Compare(container, comp.container, true, CultureInfo.InvariantCulture) == 0)
				return true;

			return false;
		}
		
		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return (name + "," + application + "," + container).ToLower(CultureInfo.InvariantCulture).GetHashCode();
		}
		#endregion
		
		#region Operators overloads
		//----------------------- Overloading degli operatori -----------------
		public static bool operator == (ModuleInfo m1, ModuleInfo m2) 
		{
			if (Object.ReferenceEquals(null, m1) &&  Object.ReferenceEquals(null, m2))
				return true;
			if (Object.ReferenceEquals(null, m1) ||  Object.ReferenceEquals(null, m2))
				return false;
			return m1.Equals(m2);
		}

		public static bool operator != (ModuleInfo m1, ModuleInfo m2)
		{
			if (Object.ReferenceEquals(null, m1) &&  Object.ReferenceEquals(null, m2))
				return false;
			if (Object.ReferenceEquals(null, m1) ||  Object.ReferenceEquals(null, m2))
				return true;
			return !m1.Equals(m2);
		}
		#endregion

		#region IDeployModule Members

		public PolicyType DeploymentPolicy
		{
			get
			{
				return this.type == ModuleType.Module 
					? PolicyType.Full
					: PolicyType.Unknown;
			}
		}

		#endregion
	}
	//=========================================================================
}
