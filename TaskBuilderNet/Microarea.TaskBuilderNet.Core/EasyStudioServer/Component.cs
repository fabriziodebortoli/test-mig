using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microarea.TaskBuilderNet.Interfaces.EasyStudioServer;
using System.ComponentModel;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer
{
	//====================================================================
	public class Component : Interfaces.EasyStudioServer.IComponent
	{
		//---------------------------------------------------------------
		virtual public string Name
		{
			get
			{
				var nameAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
				return nameAttribute == null ? null : nameAttribute.DisplayName;
			}
		}

		//---------------------------------------------------------------
		virtual public string Description
		{
			get
			{
				var desAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() as DescriptionAttribute;
				return desAttribute == null ? string.Empty : desAttribute.Description;
			}
		}

        //---------------------------------------------------------------
        virtual public Type DefaultSerializerType
        {
            get
            {
                var serAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(DefaultSerializerAttribute), true).FirstOrDefault() as DefaultSerializerAttribute;
                return serAttribute == null ? null : serAttribute.SerializerType;
            }
        }

        //---------------------------------------------------------------
        virtual public ISerializer DefaultSerializer
        {
            get
            {
                Type type = DefaultSerializerType;
                if (type != null && typeof(ISerializer).IsAssignableFrom(type))
                {
                    ISerializer serializer = Activator.CreateInstance(type) as ISerializer;
                     return serializer;
                }

                return null;
            }
        }

        //---------------------------------------------------------------
        virtual public bool HasAction(string name)
		{
			MethodInfo method = GetType().GetTypeInfo().GetMethod(name, BindingFlags.Public);
			return method != null;
		}

	}
}
