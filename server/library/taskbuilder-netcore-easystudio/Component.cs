using System;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Common.CustomAttributes;
using TaskBuilderNetCore.EasyStudio.Serializers;

namespace TaskBuilderNetCore.EasyStudio
{
	//====================================================================
	public class Component : EasyStudio.Interfaces.IComponent
    {
		//---------------------------------------------------------------
		virtual public string Name { get => GetNameAttributeFrom(GetType()); }

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
                return serAttribute?.SerializerType;
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

        //---------------------------------------------------------------
        public static string GetNameAttributeFrom (Type type)
        {
            NameAttribute nameAttribute = type.GetTypeInfo().GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault() as NameAttribute;
            return nameAttribute?.Name;
        }
    }
}
