//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microarea.TaskBuilderNet.DataSynchroProviders.PatWSConnector
{


    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "PatWSConnector.IWSConnector")]
    public interface IWSConnector
    {

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IWSConnector/Execute", ReplyAction = "http://tempuri.org/IWSConnector/ExecuteResponse")]
        string Execute(string xml);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IWSConnector/ExecuteEx", ReplyAction = "http://tempuri.org/IWSConnector/ExecuteExResponse")]
        System.Xml.Linq.XElement ExecuteEx(System.Xml.Linq.XElement xml);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWSConnectorChannel : Microarea.TaskBuilderNet.DataSynchroProviders.PatWSConnector.IWSConnector, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WSConnectorClient : System.ServiceModel.ClientBase<Microarea.TaskBuilderNet.DataSynchroProviders.PatWSConnector.IWSConnector>, Microarea.TaskBuilderNet.DataSynchroProviders.PatWSConnector.IWSConnector
    {

        public WSConnectorClient()
        {
        }

        public WSConnectorClient(string endpointConfigurationName) :
                base(endpointConfigurationName)
        {
        }

        public WSConnectorClient(string endpointConfigurationName, string remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public WSConnectorClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public WSConnectorClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(binding, remoteAddress)
        {
        }

        public string Execute(string xml)
        {
            return base.Channel.Execute(xml);
        }

        public System.Xml.Linq.XElement ExecuteEx(System.Xml.Linq.XElement xml)
        {
            return base.Channel.ExecuteEx(xml);
        }
    }
}