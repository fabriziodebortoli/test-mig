using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	public class MsilReader
	{
		private static Dictionary<short, OpCode> _instructionLookup;
		private static object _syncObject = new object();
		private BinaryReader _methodReader;
		private MsilInstruction _current;
		private Module _module;
		static MsilReader()
		{
			if (_instructionLookup == null)
			{ 
				lock (_syncObject)
				{
					if (_instructionLookup == null)
					{
						_instructionLookup = GetLookupTable();
					}
				}
			}
		}
		public MsilReader(MethodInfo method)
		{
			if (method == null)
			{
				throw new ArgumentException("method");
			}
			_module = method.Module;
			_methodReader = new BinaryReader(new MemoryStream(method.GetMethodBody().GetILAsByteArray()));
		}


		public MsilInstruction Current
		{
			get
			{
				return _current;
			}
		}

		public bool Read()
		{
			if (_methodReader.BaseStream.Length == _methodReader.BaseStream.Position)
			{
				return false;
			}
			int index = (int)_methodReader.BaseStream.Position;
			int instructionValue;

			if (_methodReader.BaseStream.Length - 1 == _methodReader.BaseStream.Position)
			{
				instructionValue = _methodReader.ReadByte();
			}
			else
			{
				instructionValue = _methodReader.ReadUInt16();
				if ((instructionValue & OpCodes.Prefix1.Value) != OpCodes.Prefix1.Value)
				{
					instructionValue &= 0xff;
					_methodReader.BaseStream.Position--;
				}
				else
				{
					instructionValue = ((0xFF00 & instructionValue) >> 8) |
						((0xFF & instructionValue) << 8);
				}
			}
			OpCode code;
			if (!_instructionLookup.TryGetValue((short)instructionValue, out code))
			{
				throw new InvalidProgramException();
			}
			int dataSize = GetSize(code.OperandType);
			byte[] data = new byte[dataSize];
			_methodReader.Read(data, 0, dataSize);

			object objData = GetData(_module, code, data);


			_current = new MsilInstruction(code, data, index, objData);
			return true;
		}

		private static object GetData(Module module, OpCode code, byte[] rawData)
		{
			object data = null;
			switch (code.OperandType)
			{
				case OperandType.InlineField:
					data = module.ResolveField(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineBrTarget:
				case OperandType.InlineSwitch:
				case OperandType.InlineI:
					data = BitConverter.ToInt32(rawData, 0);
					break;
				case OperandType.InlineI8:
					data = BitConverter.ToInt64(rawData, 0);
					break;
				case OperandType.InlineMethod:
					data = module.ResolveMethod(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineR:
					data = BitConverter.ToDouble(rawData, 0);
					break;
				case OperandType.InlineSig:
					data = module.ResolveSignature(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineString:
					data = module.ResolveString(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineTok:
				case OperandType.InlineType:
					data = module.ResolveType(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineVar:
					data = BitConverter.ToInt16(rawData, 0);
					break;
				case OperandType.ShortInlineVar:
				case OperandType.ShortInlineI:
				case OperandType.ShortInlineBrTarget:
					data = rawData[0];
					break;
				case OperandType.ShortInlineR:
					data = BitConverter.ToSingle(rawData, 0);
					break;

			}
			return data;
		}

		private static int GetSize(OperandType opType)
		{
			switch (opType)
			{
				case OperandType.InlineNone:
					return 0;
				case OperandType.ShortInlineBrTarget:
				case OperandType.ShortInlineI:
				case OperandType.ShortInlineVar:
					return 1;

				case OperandType.InlineVar:
					return 2;
				case OperandType.InlineBrTarget:
				case OperandType.InlineField:
				case OperandType.InlineI:
				case OperandType.InlineMethod:
				case OperandType.InlineSig:
				case OperandType.InlineString:
				case OperandType.InlineSwitch:
				case OperandType.InlineTok:
				case OperandType.InlineType:
				case OperandType.ShortInlineR:


					return 4;
				case OperandType.InlineI8:
				case OperandType.InlineR:

					return 8;
				default:
					return 0;

			}
		}

		private static Dictionary<short, OpCode> GetLookupTable()
		{
			Dictionary<short, OpCode> lookupTable = new Dictionary<short, OpCode>();
			FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo field in fields)
			{
				OpCode code = (OpCode)field.GetValue(null);
				lookupTable.Add(code.Value, code);
			}
			return lookupTable;
		}
	}

	public struct MsilInstruction
	{
		internal MsilInstruction(OpCode code, byte[] rawData, int index, object data)
		{
			Instruction = code;
			RawData = rawData;
			Index = index;
			Data = data;
		}
		public readonly OpCode Instruction;
		public readonly byte[] RawData;
		public readonly int Index;
		public readonly object Data;


		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("0x{0:x4} ", Index);
			builder.Append(Instruction.Name);
			if (RawData != null && RawData.Length > 0)
			{
				builder.Append(" 0x");
				for (int i = RawData.Length - 1; i >= 0; i--)
				{
					builder.Append(RawData[i].ToString("x2"));
				}
			}
			if (Data != null)
			{
				builder.Append(" " + Data.ToString());
			}
			return builder.ToString();
		}
	}
}
