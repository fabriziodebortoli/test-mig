using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// ProducerKey.
	/// </summary>
	//=========================================================================
	[Serializable]
	public class ProducerKey : ISerializable
	{
		public const string CodeTag = "Code";

		private string code = string.Empty;

		public string Code
		{
			get
			{
				return code;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				if (value.Trim().Length == 0)
					throw new ArgumentException("value cannot be empty");

				code = value;
			}
		}

		//---------------------------------------------------------------------
		public ProducerKey() {}  // DO NOT REMOVE! Needed for serialization

		//---------------------------------------------------------------------
		public ProducerKey(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

			if (code.Trim().Length == 0)
				throw new ArgumentException("code cannot be empty");
			
			this.code = code;
		}

		//---------------------------------------------------------------------
		protected ProducerKey(SerializationInfo info, StreamingContext context)
		{
			try
			{this.code = info.GetString(CodeTag);}
			catch (SerializationException)
			{this.code = string.Empty;}
		}

		#region ISerializable Members

		//---------------------------------------------------------------------
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(CodeTag, code);
		}

		#endregion

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return code;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return code.GetHashCode ();
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			ProducerKey pk = obj as ProducerKey;

			if (pk == null)
				return false;

			return String.Compare(code, pk.code, true, CultureInfo.InvariantCulture) == 0;
		}
	}
}
