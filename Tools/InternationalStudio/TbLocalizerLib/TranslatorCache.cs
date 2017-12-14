using System.Collections;

using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	//================================================================================
	public class TranslatorCache
	{
		private static Hashtable translators = new Hashtable();


		//--------------------------------------------------------------------------------
		public static bool GetTranslator(DataDocument tblslnWriter, ArrayList toolsInfo, DictionaryTreeNode nodeToTranslate, out Translator translator)
		{
			translator = GetExistingTranslator(nodeToTranslate);
			if (translator == null)
			{
				translator = new Translator(tblslnWriter, toolsInfo);
				translators[nodeToTranslate] = translator;
				translator.TreeNodeToTranslate = nodeToTranslate;
				return true;
			}
		
			return false;		
		}
	
		//--------------------------------------------------------------------------------
		public static Translator GetExistingTranslator(DictionaryTreeNode nodeToTranslate)
		{
			return translators[nodeToTranslate] as Translator;
		}

		//--------------------------------------------------------------------------------
		public static bool RefreshCache(DictionaryTreeNode oldNodeToTranslate, DictionaryTreeNode newNodeToTranslate)
		{
			if (newNodeToTranslate != null)
			{
				if (oldNodeToTranslate == null ||
					oldNodeToTranslate == newNodeToTranslate)
					return true;
			
				// esiste già un altro translator aperto su quel file!
				if (GetExistingTranslator(newNodeToTranslate) != null)
					return false;

				translators.Add(newNodeToTranslate, translators[oldNodeToTranslate]);
			}
			
			translators.Remove(oldNodeToTranslate);

			return true;

		}

		//--------------------------------------------------------------------------------
		public static void ActivateNextTranslator()
		{
			// ciclo fittizio per pescare il promo elemento e visualizzarlo
			foreach (Translator t in translators.Values)
			{
				if (t == null)
					continue;

				t.Activate();
				return;
			}
		}

		//--------------------------------------------------------------------------------
		public static bool CloseAllTranslators()
		{
			ArrayList list = new ArrayList();

			// per evitare che si sporchi la lista durante il foreach ne faccio una copia
			foreach (Translator t in translators.Values)
				list.Add(t);

			foreach (Translator t in list)
			{
				t.Close() ;
				
				if (t.Visible) return false;
			}
			return true;
		}		
	}
}
