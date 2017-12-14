using System;
using System.Collections;
using System.Xml;
using Microarea.TaskBuilderNet.Licence.Licence.XmlSyntax;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
    [Serializable]
    public class LawInfo
    {
        public enum			LawType {Allow, Deny, None}
		public ArrayList	List = null;
		public LawType      LawTypeUsed = LawType.None;

		//---------------------------------------------------------------------
		public LawInfo(){}
        private LawInfo(LawType type, ArrayList list)
		{
            List = list;
			LawTypeUsed = type;
		}


        //---------------------------------------------------------------------
        public bool Verify(string item)
        {
            if (String.IsNullOrWhiteSpace(item))


                return true;
            if (LawTypeUsed == LawType.None)
                return true;
            //21/02/2017 con Germano è stato deciso che sanmarino e vaticano seguono la law info it
            if (String.Compare(item, "sm", StringComparison.InvariantCultureIgnoreCase) == 0 || String.Compare(item, "va", StringComparison.InvariantCultureIgnoreCase) == 0)
                item = "IT";
            return
            this.LawTypeUsed == LawInfo.LawType.Allow && this.List.Contains(item) ||
            this.LawTypeUsed == LawInfo.LawType.Deny && !this.List.Contains(item);
        }
        //---------------------------------------------------------------------
        public virtual void Create(XmlElement parentElement)
        {
           
        }
      
		/// <summary>
		/// Legge se il modulo è soggetto a vincoli di localizzazione 
		/// normativa, per cui potrebbe essere incluso od escluso solo per talune isoCountry
		/// </summary>
		/// <param name="parentElement">elemento che contiene le indicazioni di eventuale localizzazione normativa</param>
		/// <returns>Oggetto che specifica come è organizzata la localizzazione normativa</returns>
		//---------------------------------------------------------------------
        protected  void Create(XmlElement parentElement, string itemName)
		{
			LawType lawType = LawType.Allow;
			XmlNode n = parentElement.SelectSingleNode(Consts.TagAllow);
			if (n == null)
			{
				lawType = LawType.Deny;
				n = parentElement.SelectSingleNode(Consts.TagDeny);
			}
			
			if (n == null)
				return ;
			
			ArrayList list = new ArrayList();

			XmlAttribute att	= n.Attributes[itemName];
			if (att == null) 
                return ;
			string attValue		= att.Value;
			attValue			= attValue.Replace(" ", "");//trim
			string[] values = attValue.Split(Consts.IsoAttributeSeparator);
			list.AddRange(values);
         
			if (lawType != LawType.None && list.Count > 0)
            {
                this.LawTypeUsed = lawType;
                this.List= list;
            }
          
		}
    }

	//=========================================================================
	[Serializable]
	public class CountryLawInfo : LawInfo
	{
        
        public override void Create(XmlElement parentElement)
        {
            Create(parentElement, Consts.IsoAttribute);
        } 
		
	}

	//=========================================================================
    [Serializable]
    public class SNTypeLawInfo: LawInfo
    {
      
        public override void Create(XmlElement parentElement)
        {
            Create(parentElement, "sntype");
        } 
    }

}
