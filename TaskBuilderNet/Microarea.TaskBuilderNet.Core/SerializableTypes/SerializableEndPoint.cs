using System;
using System.Collections.Generic;
using System.ServiceModel.Description;

namespace Microarea.TaskBuilderNet.Core.SerializableTypes
{
	[Serializable]
	///uso questo wrapper perche' la classe ServiceEndpoint non e' serializzabile!
	public class SerializableEndpoint
	{
		private ServiceEndpoint endpoint;

		//--------------------------------------------------------------------------------
		public SerializableEndpoint(ServiceEndpoint endpoint)
		{
			this.endpoint = SanitizeContractInEndpoint(endpoint);
		}
		//--------------------------------------------------------------------------------
		public SerializableEndpoint()
		{

		}
		//--------------------------------------------------------------------------------
		public ServiceEndpoint GetEndPoint() { return this.endpoint; }



		//--------------------------------------------------------------------------------
		public MetadataSet Metadata
		{
			get
			{
				WsdlExporter exporter = new WsdlExporter();
				exporter.ExportEndpoint(this.endpoint);
				return exporter.GetGeneratedMetadata();
			}

			set
			{
				WsdlImporter importer = new WsdlImporter(value);
				ServiceEndpointCollection endpoints = importer.ImportAllEndpoints();
				if (endpoints.Count != 1)
					throw new ArgumentException("MetadataBundle must contain exactly one endpoint.");
				this.endpoint = SanitizeContractInEndpoint(endpoints[0]);
			}
		}

		//--------------------------------------------------------------------------------
		private ServiceEndpoint SanitizeContractInEndpoint(ServiceEndpoint endpoint)
		{
			ServiceEndpoint newEndpoint = new ServiceEndpoint(new ContractDescription(endpoint.Contract.Name, endpoint.Contract.Namespace));
			newEndpoint.Address = endpoint.Address;
			newEndpoint.Binding = endpoint.Binding;
			newEndpoint.Name = endpoint.Name;
			return newEndpoint;
		}
	}

	[Serializable]
	public class SerializableEndPointCollection : List<SerializableEndpoint>
	{
	}
}
