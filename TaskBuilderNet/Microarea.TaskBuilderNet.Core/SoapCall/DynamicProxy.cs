namespace Microarea.TaskBuilderNet.Core.SoapCall
{
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Channels;

	//================================================================================
	public class DynamicProxy : DynamicObject
	{
		//--------------------------------------------------------------------------------
		public DynamicProxy(Type proxyType, Binding binding, EndpointAddress address)
			: base(proxyType)
		{
			Type[] paramTypes = new Type[2];
			paramTypes[0] = typeof(Binding);
			paramTypes[1] = typeof(EndpointAddress);

			object[] paramValues = new object[2];
			paramValues[0] = binding;
			paramValues[1] = address;

			CallConstructor(paramTypes, paramValues);
		}

		//--------------------------------------------------------------------------------
		public Type ProxyType
		{
			get
			{
				return ObjectType;
			}
		}


		//--------------------------------------------------------------------------------
		internal bool Valid
		{
			get 
			{
				CommunicationState s = (CommunicationState) GetProperty("State");
				return 
					(s == CommunicationState.Opened) ||
					(s == CommunicationState.Opening) ||
					(s == CommunicationState.Created);
			}
		}
		//--------------------------------------------------------------------------------
		public object Proxy
		{
			get
			{
				return ObjectInstance;
			}
		}

		//--------------------------------------------------------------------------------
		public void Close()
		{
			CallMethod("Close");
		}

	}
}
