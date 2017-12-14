using System;

namespace Microarea.Console.Core.PlugIns
{
	/// <summary>
	/// IsPlugIn
	/// Se true, la dll è un plugIn. Va impostato a livello di Assembly, nel file
	/// AssemblyInfo.cs del plugIn
	/// </summary>
	//=========================================================================
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public class IsPlugIn: System.Attribute
	{
		private bool isPlugInValue = false;
		//IsPlugIn
		//---------------------------------------------------------------------
		public IsPlugIn(bool isPlugIn)
		{
			isPlugInValue = isPlugIn;
		}
		//IsPlugInValue
		//---------------------------------------------------------------------
		public bool IsPlugInValue
		{
			get {return isPlugInValue; }
		}
	}

	/// <summary>
	/// RunningFromServer
	/// true se il plugIn runna da MicroareaServer, false altrimenti
	/// </summary>
	//=========================================================================
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public class RunningFromServer: System.Attribute
	{
		private bool runningFromServerValue = false;
		//RunningFromServer
		//---------------------------------------------------------------------
		public RunningFromServer(bool runningFromServer)
		{
			runningFromServerValue = runningFromServer;
		}
		//RunningFromServerValue
		//---------------------------------------------------------------------
		public bool RunningFromServerValue
		{
			get { return runningFromServerValue; }
		}
	}

	/// <summary>
	/// RunningFromClient
	/// true se il plugIn runna da MicroareaClient, false altrimenti
	/// </summary>
	//=========================================================================
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public class RunningFromClient: System.Attribute
	{
		private bool runningFromClientValue = false;
		//RunningFromClient
		//---------------------------------------------------------------------
		public RunningFromClient(bool runningFromClient)
		{
			runningFromClientValue = runningFromClient;
		}
		//RunningFromClientValue
		//---------------------------------------------------------------------
		public bool RunningFromClientValue
		{
			get{ return runningFromClientValue; }
		}
	}
	
	/// <summary>
	/// DependencyPlugIn
	/// Specifica da quale altro plugIn un certo plugIn dipende
	/// E' un attributo a livello di Assembly (ovvero va impostato
	/// nell'AssemblyInfo.cs del plugIn)
	/// </summary>
	//=========================================================================
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public class DependencyFromPlugIn: System.Attribute
	{
		private string assemblyDependency;
		
		//DependencyFromPlugIn
		//---------------------------------------------------------------------
		public DependencyFromPlugIn( string assemblyDependency)
		{
			this.assemblyDependency = assemblyDependency;
		}
		//AssemblyDependency
		//---------------------------------------------------------------------
		public string AssemblyDependency
		{
			get{ return assemblyDependency; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	//=========================================================================
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public class Activated: System.Attribute
	{
		private bool isActivated = false;
		
		//---------------------------------------------------------------------
		public Activated(bool isActivated)
		{
			this.isActivated = isActivated;
		}
		
		//---------------------------------------------------------------------
		public bool IsActivatedValue
		{
			get {return isActivated; }
		}
	}

	/// <summary>
	/// PlugInFolderName
	/// E' un attributo a livello di Assembly (ovvero va impostato nell'AssemblyInfo.cs del plugIn)
	/// Attributo aggiunto da Michela per tenere sotto controllo il nome del folder di caricamento del plugin
	/// (x gestire la coesistenza di due plugin con lo stesso namespace - 14/01/09)
	/// </summary>
	/// 
	//@@TODOMICHI DA TOGLIERE 3.0
	//=========================================================================
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public class PlugInFolderName: System.Attribute
	{
		private string plugInFolderName;

		//PlugInFolderName
		//---------------------------------------------------------------------
		public PlugInFolderName(string plugInFolderName)
		{
			this.plugInFolderName = plugInFolderName;
		}
		//GetPlugInFolderName
		//---------------------------------------------------------------------
		public string GetPlugInFolderName { get { return plugInFolderName; } }
	}
}
