using System.Collections;

namespace Microarea.Library.SqlScriptUtility
{
	public class ConstraintList : Hashtable
	{
		public string name = string.Empty;
		public int lastValue = 0;
	}

	public class ConstraintManager
	{
		public ConstraintList constraintList = new ConstraintList();
		
		public void Update(string newConstraintName)
		{
			if (newConstraintName == string.Empty)
				return;

			string cName = newConstraintName.Substring(0, newConstraintName.Length - 4);
			int cValue = int.Parse(newConstraintName.Substring(cName.Length + 2));
			if (constraintList.ContainsKey(cName))
			{
				if ((int)constraintList[cName] < cValue)
				{
					constraintList[cName] = cValue;
				}
			}
			else
				constraintList.Add(cName, cValue);
		}
		
		public string GetConstraint(string nTabella, string nColonna)
		{
			try
			{
				if (nTabella.Length > 13)
					nTabella = nTabella.Substring(3, 10);
				else
					nTabella = nTabella.Substring(3, nTabella.Length - 3);
			}
			catch
			{
			}

			if (nColonna.Length > 10)
				nColonna = nColonna.Substring(0, 10);

			string tmpConstraint = string.Format("DF_{0}_{1}", nTabella, nColonna);

			if (constraintList.ContainsKey(tmpConstraint))
			{
				int cValue = (int)constraintList[tmpConstraint];
				cValue++;
				constraintList[tmpConstraint] = cValue;
					
				string cnt = string.Empty;
				if (cValue < 10)
					cnt = string.Format("0{0}", cValue.ToString());
				else
					cnt = cValue.ToString();

				return string.Format("{0}_{1}", tmpConstraint, cnt);
			}
			else
			{
				constraintList.Add(tmpConstraint, 0);
				return string.Format("{0}_00", tmpConstraint);
			}
		}
	}
}
