using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Microarea.TaskBuilderNet.Woorm.WoormViewer
{
	//================================================================================
	[Serializable]
	public class WoormSerializable : ISerializable
	{
		public WoormSerializable()
		{
		}

		public WoormSerializable(SerializationInfo info, StreamingContext context)
		{
			foreach (PropertyInfo pi in GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				object[] attrs = pi.GetCustomAttributes(typeof(WoormSerializableAttribute), true);
				if (attrs.Length != 1)
					continue;

				WoormSerializableAttribute a = (WoormSerializableAttribute)attrs[0];
				string name = string.IsNullOrEmpty(a.Name) ? pi.Name : a.Name;
				try
				{
					pi.SetValue(this, info.GetValue(name, pi.GetGetMethod().ReturnType), null);
				}
				catch
				{
				}
			}

			foreach (FieldInfo fi in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				object[] attrs = fi.GetCustomAttributes(typeof(WoormSerializableAttribute), true);
				if (attrs.Length != 1)
					continue;

				WoormSerializableAttribute a = (WoormSerializableAttribute)attrs[0];
				string name = string.IsNullOrEmpty(a.Name) ? fi.Name : a.Name;
				try
				{
					fi.SetValue(this, info.GetValue(name, fi.FieldType));
				}
				catch
				{
				}
			}
		}

		//------------------------------------------------------------------------------				
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			foreach (PropertyInfo pi in GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				object[] attrs = pi.GetCustomAttributes(typeof(WoormSerializableAttribute), true);
				if (attrs.Length != 1)
					continue;

				WoormSerializableAttribute a = (WoormSerializableAttribute)attrs[0];
				if (!a.Conditional || ShouldSerialize(pi.Name))
				{
					string name = string.IsNullOrEmpty(a.Name) ? pi.Name : a.Name;
					info.AddValue(name, pi.GetValue(this, null));
				}
			}

			foreach (FieldInfo fi in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				object[] attrs = fi.GetCustomAttributes(typeof(WoormSerializableAttribute), true);
				if (attrs.Length != 1)
					continue;

				WoormSerializableAttribute a = (WoormSerializableAttribute)attrs[0];
				if (!a.Conditional || ShouldSerialize(fi.Name))
				{
					string name = string.IsNullOrEmpty(a.Name) ? fi.Name : a.Name;
					info.AddValue(name, fi.GetValue(this));
				}
			}
		}

		//------------------------------------------------------------------------------				
		public virtual bool ShouldSerialize(string name) { return true; }
	}

	//================================================================================
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class WoormSerializableAttribute : Attribute
	{
		public string Name { get; set; }
		public bool Conditional { get; set; }

		public WoormSerializableAttribute()
		{
		}
	}
}
