using System;
using System.Collections.Generic;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{

	internal enum VirtualKey { NONE, CTRL, ALT, SHIFT, CTRL_ALT, CTRL_SHIFT, ALT_SHIFT, CTRL_ALT_SHIFT }
	class Accelerator
	{
		private const short FSHIFT = 0x04;
		private const short FCONTROL = 0x08;
		private const short ALT = 0x10;
		
		public Int16 fVirt;
		public Int16 key;
		public Int32 cmd;
		public VirtualKey VirtualKey;

		public Accelerator(Int16 fVirt, Int16 key, Int32 cmd )
		{
			this.fVirt = fVirt;
			this.key = key;
			this.cmd = cmd;
			VirtualKey = GetVirtualKey();
		}

		private VirtualKey GetVirtualKey() 
		{
			if ((fVirt & (FSHIFT | FCONTROL | ALT)) == (FSHIFT | FCONTROL | ALT))
				return VirtualKey.CTRL_ALT_SHIFT;

			if ((fVirt & (FSHIFT | FCONTROL)) == (FSHIFT | FCONTROL))
				return VirtualKey.CTRL_SHIFT;

			if ((fVirt & (FSHIFT | ALT)) == (FSHIFT | ALT))
				return VirtualKey.ALT_SHIFT;

			if ((fVirt & (FCONTROL | ALT)) == (FCONTROL | ALT))
				return VirtualKey.CTRL_ALT;

			if ((fVirt & (FSHIFT)) == (FSHIFT))
				return VirtualKey.SHIFT;

			if ((fVirt & (FCONTROL)) == (FCONTROL))
				return VirtualKey.CTRL;

			if ((fVirt & (ALT)) == (ALT))
				return VirtualKey.ALT;

			return VirtualKey.NONE;
		}
	}

	internal class AcceleratorDescriptionComparer : IEqualityComparer<AcceleratorDescription>
	{

		#region IEqualityComparer<AcceleratorDescription> Members

		public bool Equals(AcceleratorDescription x, AcceleratorDescription y)
		{
			return x.WindowId.Equals(y.WindowId);
		}

		public int GetHashCode(AcceleratorDescription obj)
		{
			return obj.WindowId.GetHashCode();
		}

		#endregion
	}

	internal class AcceleratorManager
	{
		string id;
		Dictionary<VirtualKey, StringBuilder> builders = new Dictionary<VirtualKey, StringBuilder>();
		internal AcceleratorManager(string id)
		{
			this.id = id;
		}
		internal string GenerateScript(List<AcceleratorDescription> accelerators)
		{
			InitScripts();
			foreach (AcceleratorDescription accelerator in accelerators)
			{
				foreach (Accelerator acc in accelerator.Accelerators)
				{
					StringBuilder script = builders[acc.VirtualKey];
					script.AppendFormat("Array_{0}_{1}.push(new Accelerator({2}, {3}, '{4}'));\r\n", id, acc.VirtualKey, acc.key, acc.cmd, accelerator.WindowId);
				}
			}
			StringBuilder sb = new StringBuilder();
			foreach (StringBuilder s in builders.Values)
				sb.Append(s);
			
			string ret = sb.ToString();
#if !DEBUG
			JavaScriptMinifier min = new JavaScriptMinifier();
			ret = min.Minify(ret);
#endif
			return ret;
		}

		private void InitScripts()
		{
			builders[VirtualKey.NONE] = new StringBuilder();
			builders[VirtualKey.CTRL] = new StringBuilder();
			builders[VirtualKey.ALT] = new StringBuilder();
			builders[VirtualKey.SHIFT] = new StringBuilder();
			builders[VirtualKey.CTRL_ALT] = new StringBuilder();
			builders[VirtualKey.CTRL_SHIFT] = new StringBuilder();
			builders[VirtualKey.ALT_SHIFT] = new StringBuilder();
			builders[VirtualKey.CTRL_ALT_SHIFT] = new StringBuilder();

			foreach (KeyValuePair<VirtualKey, StringBuilder> entry in builders)
				entry.Value.AppendFormat("Array_{0}_{1} = new Array();\r\n", id, entry.Key);
			
		}
	}
}
