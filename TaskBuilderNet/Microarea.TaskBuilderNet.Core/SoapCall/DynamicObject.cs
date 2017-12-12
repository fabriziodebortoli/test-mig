namespace Microarea.TaskBuilderNet.Core.SoapCall
{
	using System;
	using System.Reflection;

    //================================================================================
	public class DynamicObject
	{
		private Type objType;
		private object obj;

		private BindingFlags CommonBindingFlags =
			BindingFlags.Instance |
			BindingFlags.Public;

		//--------------------------------------------------------------------------------
		public DynamicObject(Object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			this.obj = obj;
			this.objType = obj.GetType();
		}

		//--------------------------------------------------------------------------------
		public DynamicObject(Type objType)
		{
			if (objType == null)
				throw new ArgumentNullException("objType");

			this.objType = objType;
		}

		//--------------------------------------------------------------------------------
		public void CallConstructor()
		{
			CallConstructor(new Type[0], new object[0]);
		}

		//--------------------------------------------------------------------------------
		public void CallConstructor(Type[] paramTypes, object[] paramValues)
		{
			ConstructorInfo ctor = this.objType.GetConstructor(paramTypes);
			if (ctor == null)
			{
				throw new DynamicProxyException(SoapCallStrings.ProxyCtorNotFound);
			}

			this.obj = ctor.Invoke(paramValues);
		}

		//--------------------------------------------------------------------------------
		public object GetProperty(string property)
		{
			object retval = this.objType.InvokeMember(
				property,
				BindingFlags.GetProperty | CommonBindingFlags,
				null /* Binder */,
				this.obj,
				null /* args */);

			return retval;
		}

		//--------------------------------------------------------------------------------
		public object SetProperty(string property, object value)
		{
			object retval = this.objType.InvokeMember(
				property,
				BindingFlags.SetProperty | CommonBindingFlags,
				null /* Binder */,
				this.obj,
				new object[] { value });

			return retval;
		}

		//--------------------------------------------------------------------------------
		public object GetField(string field)
		{
			object retval = this.objType.InvokeMember(
				field,
				BindingFlags.GetField | CommonBindingFlags,
				null /* Binder */,
				this.obj,
				null /* args */);

			return retval;
		}

		//--------------------------------------------------------------------------------
		public object SetField(string field, object value)
		{
			object retval = this.objType.InvokeMember(
				field,
				BindingFlags.SetField | CommonBindingFlags,
				null /* Binder */,
				this.obj,
				new object[] { value });

			return retval;
		}

		//--------------------------------------------------------------------------------
		public object CallMethod(string method, params object[] parameters)
		{
			MethodInfo mi = GetMethod(method);
			
			return mi.Invoke(this.obj, parameters);
		}

		//--------------------------------------------------------------------------------
		public object CallMethod(MethodInfo method, params object[] parameters)
		{
			return method.Invoke(this.obj, parameters);
		}

		//--------------------------------------------------------------------------------
		public MethodInfo GetMethod(string method)
		{ 
			return this.objType.GetMethod(method);
		}

		//--------------------------------------------------------------------------------
		public object CallMethod(string method, Type[] types,
					object[] parameters)
		{
			if (types.Length != parameters.Length)
				throw new ArgumentException(
					SoapCallStrings.ParameterValueMistmatch);

			MethodInfo mi = this.objType.GetMethod(method, types);
			if (mi == null)
				throw new ApplicationException(string.Format(
					SoapCallStrings.MethodNotFound, method));

			object retval = mi.Invoke(this.obj, CommonBindingFlags, null,
				parameters, null);

			return retval;
		}

		//--------------------------------------------------------------------------------
		public Type ObjectType
		{
			get
			{
				return this.objType;
			}
		}

		//--------------------------------------------------------------------------------
		public object ObjectInstance
		{
			get
			{
				return this.obj;
			}
		}

		//--------------------------------------------------------------------------------
		public BindingFlags BindingFlags
		{
			get
			{
				return this.CommonBindingFlags;
			}

			set
			{
				this.CommonBindingFlags = value;
			}
		}
	}
}
