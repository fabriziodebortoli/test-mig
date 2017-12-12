using System;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using ICSharpCode.NRefactory.CSharp;

namespace Microarea.EasyBuilder
{
	class InvalidNameException : ApplicationException
	{
		public InvalidNameException(string text)
			: base(text)
		{
		}
	}
	class ItemSorter : IComparer
	{
		public int Compare(object x, object y)
		{
			if (x is IComparable)
				return ((IComparable)x).CompareTo(y);
			return x.GetHashCode().CompareTo(y.GetHashCode());
		}
	}
	//================================================================================
	/// Microarea S.p.a
	/// 
	/// <summary>
	/// Class castomization for serialize the obj, using the reflection
	/// </summary>
	///
	//================================================================================
	public class CustomizationSerializer
	{
		private const string root = "root";
		private const string beginStringToken = "#BS";
		private const string bareEndStringToken = "ES";
		private const string endStringToken = "#" + bareEndStringToken;
		private const string itemToken = "i";
		private const string nullToken = "(nil)";
		List<Assembly> knownAssemblies = new List<Assembly>();
		int level = 0;
		/// <summary>
		/// costruttore
		/// </summary>
		public CustomizationSerializer()
		{
			knownAssemblies.Add(typeof(string).Assembly);
			knownAssemblies.Add(typeof(NamespaceDeclaration).Assembly);
			knownAssemblies.Add(typeof(CustomizationInfos).Assembly);
			knownAssemblies.Add(typeof(Microarea.TaskBuilderNet.Core.Generic.NameSpace).Assembly);
			knownAssemblies.Add(typeof(Microarea.TaskBuilderNet.Interfaces.NameSpaceObjectType).Assembly);
			
		}
		/// <summary>
		/// Serializza l'oggetto in formato testo
		/// </summary>
		/// <param name="obj">oggetto da serializzare</param>
		/// <param name="tw">text writer</param>
		public void Serialize(object obj, TextWriter tw)
		{
			Serialize(tw, obj, null, root);
		}
		/// <summary>
		/// Deserializza l'oggetto da uno stream di testo
		/// </summary>
		/// <param name="tr">il text reader</param>
		/// <returns>l'oggetto</returns>
		public object Deserialize(TextReader tr)
		{
			string n;
			object o = Deserialize(root, null, tr, out n);
			if (n != root)
				throw new InvalidNameException(string.Format(Properties.Resources.ParsingErrorSerializer, root, n));
			return o;
		}
		private object Deserialize(string fieldName, Type declaringType, TextReader tr, out string name)
		{
			string typeName; 
			ReadMeta(tr, out name, out typeName);
			if (name != fieldName)
				return null;
			string sNormal = tr.ReadLine();
			string sTrim = sNormal.Trim();
			if (sTrim == nullToken)
			{
				return null;
			}
			Type t = typeName.Length == 0 ? declaringType : GetType(typeName);
			if (t.IsEnum)
			{ // Enum
				return Enum.Parse(t, sTrim);
			}
			else if (t == typeof(string))
			{ // string
				if (sNormal != beginStringToken)
					return sNormal;
				StringBuilder sb = new StringBuilder();
				string token;
				while (ReadStringToken(tr, out token))
				{
					if (sb.Length > 0)
						sb.Append(Environment.NewLine);
					sb.Append(token);
				}
				return sb.ToString();
			}
			else if (t == typeof(bool))
			{ 
				return bool.Parse(sTrim);
			}
			else if (t == typeof(char))
			{ 
				return char.Parse(sNormal);
			}
			else if (t == typeof(SByte))
			{ 
				return SByte.Parse(sTrim);
			}
			else if (t == typeof(Byte))
			{
				return Byte.Parse(sTrim);
			}
			else if (t == typeof(Int16))
			{
				return Int16.Parse(sTrim);
			}
			else if (t == typeof(UInt16))
			{
				return UInt16.Parse(sTrim);
			}
			else if (t == typeof(Int32))
			{
				return Int32.Parse(sTrim);
			}
			else if (t == typeof(UInt32))
			{
				return UInt32.Parse(sTrim);
			}
			else if (t == typeof(Int64))
			{
				return Int64.Parse(sTrim);
			}
			else if (t == typeof(UInt64))
			{
				return UInt64.Parse(sTrim);
			}
			else if (t == typeof(Single))
			{
				return Single.Parse(sTrim);
			}
			else if (t == typeof(Double))
			{
				return Double.Parse(sTrim);
			}
			else if (t == typeof(Decimal))
			{
				return Decimal.Parse(sTrim);
			}
			else if (t == typeof(DateTime))
			{
				return DateTime.Parse(sTrim);
			}
			else if (t == typeof(ListDictionary))
			{
				ListDictionary ld = new ListDictionary();
				int l = int.Parse(sTrim);
				for (int i = 0; i < l; i++)
				{
					string n;
					object item = Deserialize(itemToken, null, tr, out n);
					if (n != itemToken)
						throw new InvalidNameException(string.Format(Properties.Resources.ParsingErrorSerializer, itemToken, n));
					ld.Add(((DictionaryEntry)item).Key, ((DictionaryEntry)item).Value);
				}
				return ld;
			}
			else if (t.GetInterface(typeof(IList).FullName) != null)
			{
				IList list = (IList)Activator.CreateInstance(t);
				int l = int.Parse(sTrim);
				for (int i = 0; i < l; i++)
				{
					string n;
					object item = Deserialize(itemToken, null, tr, out n);
					if (n != itemToken)
						throw new InvalidNameException(string.Format(Properties.Resources.ParsingErrorSerializer, itemToken, n));
					list.Add(item);
				}
				return list;
			}
			else
			{
				object o = Activator.CreateInstance(t);
				List<FieldInfo> fields = t.GetAllFields();
				foreach (FieldInfo field in fields)
				{
					string n;
					object oField = Deserialize(field.Name, field.FieldType, tr, out n);
					if (n != field.Name)
						throw new InvalidNameException(string.Format(Properties.Resources.ParsingErrorSerializer, field.Name, n));
					field.SetValue(o, oField);
				}
				return o;
			}
		}

		private bool ReadStringToken(TextReader tr, out string token)
		{
			int ch;
			//leggo il nome
			while ((ch = tr.Read()) != '#');
			token = tr.ReadLine();
			return token != bareEndStringToken;
		}

		private Type GetType(string typeName)
		{
			foreach (Assembly asm in knownAssemblies)
			{
				Type t = asm.GetType(typeName, false);
				if (t != null)
					return t;
			}
			return null;
		}

		
		
		private void IterateFields(TextWriter tw, object obj)
		{
			if (obj == null) return;
			Type objType = obj.GetType();
			List<FieldInfo> fields = objType.GetAllFields();
			foreach (FieldInfo field in fields)
			{
				var val = field.GetValue(obj);
				if (val == null && field.FieldType.GetInterface(typeof(ICollection).FullName) != null && !field.FieldType.IsAbstract && !field.FieldType.IsInterface)
					val = Activator.CreateInstance(field.FieldType);
				Serialize(tw, val, field.FieldType, field.Name);

			}
		}

		private void Serialize(TextWriter tw, object val, Type declaredType, string name)
		{
			WriteMeta(tw, val, declaredType, name);
			if (val == null)
			{
				tw.WriteLine(nullToken);
				return;
			}

			Type t = val.GetType();

			if (t.IsEnum)
			{ // Enum
				tw.WriteLine(Convert.ToInt32(val));
			}
			else if (t == typeof(string))
			{ // string
				string s = (string)val;
				s = s.Replace("\r", "");
				string[] tokens = s.Split('\n');
				if (tokens.Length == 1)
					tw.WriteLine(s);
				else
				{
					tw.WriteLine(beginStringToken);
					foreach (string token in tokens)
					{
						for (int i = 0; i < level + 1; i++)
							tw.Write("\t");
						tw.Write("#");
						tw.WriteLine(token);
					}
					for (int i = 0; i < level + 1; i++)
						tw.Write("\t");
					tw.WriteLine(endStringToken);
					
				}
			}
			else if (t == typeof(bool))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(char))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(SByte))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(Byte))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(Int16))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(UInt16))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(Int32))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(UInt32))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(Int64))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(UInt64))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(Single))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(Double))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(Decimal))
			{
				tw.WriteLine(val.ToString());
			}
			else if (t == typeof(DateTime))
			{
				tw.WriteLine(val.ToString());
			}
			else if (val is ICollection)
			{
				tw.WriteLine(((ICollection)val).Count);
				level++;
				// Enumerate
				ICollection enumerableElement = val as ICollection;
				//ArrayList tmpList = new ArrayList();
				//tmpList.AddRange(enumerableElement);
				//tmpList.Sort(new ItemSorter());
				foreach (object item in enumerableElement)
					Serialize(tw, item, null, itemToken);

				level--;
			}
			else
			{
				tw.WriteLine();
				level++;
				IterateFields(tw, val);
				level--;
			}

		}

		private void WriteMeta(TextWriter tw, object val, Type declaredType, string name)
		{
			for (int i = 0; i < level; i++)
				tw.Write("\t");
			tw.Write(name);
			if (val != null && val.GetType() != declaredType)
			{
				tw.Write("{");
				tw.Write(val.GetType().FullName);
				tw.Write("}");
			}
			tw.Write(": ");
		}
		private void ReadMeta(TextReader tr, out string name, out string type)
		{
			StringBuilder sbName = new StringBuilder();
			StringBuilder sbType = new StringBuilder();

			bool hasType = true;
			int ch;
			//leggo il nome
			while (true)
			{
				ch = tr.Read();
				if (ch == '{')
					break;
				if (ch == ':')
				{
					hasType = false;
					break;
				}
				if (ch != '\t')
					sbName.Append((char)ch);
			}
			//leggoil tipo
			if (hasType)
			{
				while ((ch = tr.Read()) != '}')
				{
					sbType.Append((char)ch);
				}
				tr.Read();//salto :
			}
			tr.Read();//salto lo spazio
			name = sbName.ToString();
			type = sbType.ToString();
		}
	}

	

	static class Extensions
	{
		internal static List<FieldInfo> GetAllFields(this Type objType)
		{
			List<FieldInfo> fields = new List<FieldInfo>();
			fields.AddRange(objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

			if (objType.BaseType != typeof(object))
				fields.AddRange(objType.BaseType.GetAllFields());

			return fields;
		}
	}
}

