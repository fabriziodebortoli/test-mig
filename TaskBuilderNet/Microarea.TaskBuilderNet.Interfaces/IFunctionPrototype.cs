using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.Interfaces
{
	public enum ParameterModeType { In, Out, InOut };

    public interface IParameter 
	{
		string Name { get; }
		string Title { get; }
		string Type { get; }
        string TbType { get; }
        string BaseType { get; }
		bool Optional { get; }
		ParameterModeType Mode { get; }
	}

	public interface IFunctionPrototype
	{
		string FullName { get; }
		string Name { get; }
        INameSpace NameSpace { get; }

		string Title { get;	 }
		string ReturnType { get; }
        string ReturnTbType { get; }
 
		int Port { get; set; }
		string Server { get; set; }
		string Service { get; set; }
		string ServiceNamespace { get; set; }

		int NrParameters { get; }
		int ParamIndex (string name);
		IParameter GetParameter (int i);

        string GetFunctionDescription();
	}

    public interface IFunctions
    {
        List<IFunctionPrototype> Prototypes { get; }
        void LoadPrototypes();
    }

}
