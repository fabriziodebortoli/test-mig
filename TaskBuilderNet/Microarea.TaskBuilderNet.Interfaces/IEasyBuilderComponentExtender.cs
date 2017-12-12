using System.Collections;
using System.ComponentModel;

namespace Microarea.TaskBuilderNet.Interfaces.EasyBuilder
{
    //=======================================================================
    public interface IEasyBuilderComponentExtender : IComponent
    {
        string Name { get; }
        string AccessorPropertyName { get; }

        bool CanExtendObject(IEasyBuilderComponentExtendable e);
        void AdjustSite();
    }

    //=======================================================================
    public interface IEasyBuilderComponentExtenders : IList
    {
        IEasyBuilderComponentExtender this[string name] { get; }
        IEasyBuilderComponentExtenderService Service { get; }
    }

    //=======================================================================
    public interface IEasyBuilderComponentExtenderService
    {
        IEasyBuilderComponentExtendable Parent { set; }


        IEasyBuilderComponentExtender CreateExtension(string name);
        void AdjustSites();
        void Refresh();
    }

    //=======================================================================
    public interface IEasyBuilderComponentExtendable 
    {
        string  SerializedName { get; }
        ISite   Site { set; }
        IEasyBuilderComponentExtenders Extensions { get; }
    }
}

