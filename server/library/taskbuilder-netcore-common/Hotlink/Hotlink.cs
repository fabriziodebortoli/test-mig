using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.ExpressionManager;
using TaskBuilderNetCore.Interfaces;
using System;
using Microarea.Common.Generic;

namespace Microarea.Common.Hotlink
{
    //============================================================================
    public class Hotlink
	{
		public enum HklAction {Code = 0,  Description = 1, Combo = 2, DirectAccess = 3 }

		public List<Expression>				ActualParams = new List<Expression>();
		public ReferenceObjectsPrototype	Prototype;
		public string						FieldName = "";
		public string						QueryString = "";
		public ArrayList					QueryParams = new ArrayList();
		public HklAction					CurrentAction = HklAction.Code;

        //----------------------------------------------------------------------------
        private TbSession session = null;
        public TbSession Session { get { return session; }}

        //----------------------------------------------------------------------------
        public Hotlink(TbSession s)
        {
            session = s;
        }

        //----------------------------------------------------------------------------
        public bool HasMember(string name) 
        {
            foreach (Expression param in ActualParams)
            {
                if (param.HasMember(name))
                    return true;
            }
            return false;
        }

  		/*
			<?xml version="1.0" encoding="utf-8" ?> 
			<HotKeyLink>
				<ControlData value="">	// Nota ControlData è il valore contenuto nel CurrentAskEntry
				<Param value="">		// i singoli parametri dell'Hotlink nell'ordine incontrato
				<Param value="">		// i valori sono in formato Soap
			</HotKeyLink>
		*/
		// i parametri sono valorizzati i tipi in formato Soap
		//----------------------------------------------------------------------------
		public string GetActualParamsAsXML(object fieldData)
		{
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(string.Format("<{0}/>", ReferenceObjectsXML.Element.HotKeyLink));
            StringBuilder s = new StringBuilder();
			StringWriter writer = new StringWriter(s);

			// Aggiunge il valore del field attivo
			XmlElement controlDataNode = dom.CreateElement(ReferenceObjectsXML.Element.ControlData);
			object controlData = ObjectHelper.CastToDBData(fieldData);
			controlDataNode.SetAttribute(ReferenceObjectsXML.Attribute.Value, controlData.ToString());
			dom.DocumentElement.AppendChild(controlDataNode);

			// Aggiunge i parametri.
			foreach (Expression expression in ActualParams)
			{
				Value result = expression.Eval();
				if (result != null)
				{
					XmlElement param = dom.CreateElement(ReferenceObjectsXML.Element.Param);
					object o = SoapTypes.To(result.Data);
					param.SetAttribute(ReferenceObjectsXML.Attribute.Value, o.ToString());
					dom.DocumentElement.AppendChild(param);
				}
			}

			dom.Save(writer);
			return s.ToString();
		}

        // i parametri sono valorizzati i tipi in formato Soap
        //----------------------------------------------------------------------------
        public string GetActualParamsAsJson()
        {
            string args = "\"args\":{";
            int i = 0;
            foreach (Expression expression in ActualParams)
            {
                Value result = expression.Eval();
                if (result != null)
                {
                    string name = this.Prototype.GetParameter(i).Name;
                    object v = result.Data;

                    if (i > 0) args +=  ',' ;
    
                    args += v.ToJson(name);
                }
                i++;
            }
            args += '}';
            return args;
        }

        //----------------------------------------------------------------------------
        public string ParamName(int i)
		{
			return "@P" + i.ToString();
		}

		// La query contiene ? al posto dei parametri da rimappare nell'ordine dei param ritornati
		// e deve essere trasformata in quella Ado.net (@Nome al posto di ?)
		//----------------------------------------------------------------------------
		public string ChangeParameterTag(string query)
		{
			string[] segment = query.Split('?');
			if (segment.Length == 1) return query;

			string buffer = segment[0];
			for (int i = 1; i < segment.Length; i++)
				buffer += ParamName(i) + segment[i];

			return buffer;
		}
	
		/*
				<?xml version="1.0" encoding="utf-8" ?> 
				<HotKeyLink>
				<Query value="sqlquery">
					<Param value=""	type="" basetype=""/>
					<Param value=""	type="" basetype=""/>
					<Param value=""	type="" basetype=""/>
				</HotKeyLink>
		*/
		//----------------------------------------------------------------------------
		public bool BuildQueryString(object fieldData)
		{
			FieldName = Prototype.DbFieldName;
            string response = null;

            try
            {
                response = TbSession.GetHotLinkQuery(session, GetActualParamsAsXML(fieldData), (int) CurrentAction).Result;
            }
            catch (Exception)
            {
                return false; 
            }
			// se fallisce la chiamata o non torna nulla inibisco la chiamata all'hotlink
			if (response == null || response == string.Empty)
				return false;

			try
			{
                XmlDocument dom = new XmlDocument();
				dom.LoadXml(response);
				XmlNode root = dom.DocumentElement;

				// la risposta contiene il frammento XML contenente quanto serve.
				// deve esistere la dichiarazione altrimenti considero il referenceObject inesistente
				XmlElement queryElement = (XmlElement)root.SelectSingleNode(ReferenceObjectsXML.Element.Query);
				if (queryElement == null)
					return false;
				
				// sostituisce ? e mette @Pn dove n è il numero del parametro
				QueryString = ChangeParameterTag(queryElement.GetAttribute(ReferenceObjectsXML.Attribute.Value));

				QueryParams.Clear();
				XmlNodeList parameters = root.SelectNodes(ReferenceObjectsXML.Element.Param);
				foreach (XmlElement parameter in parameters)
				{
					string paramValue	= parameter.GetAttribute(ReferenceObjectsXML.Attribute.Value);
					string paramType	= ObjectHelper.FromTBType(parameter.GetAttribute(ReferenceObjectsXML.Attribute.Type));

					object o = SoapTypes.From(paramValue, paramType);
					QueryParams.Add(o);
				}
				return true;
			}
			catch (XmlException e)
			{
				Debug.Fail(e.Message);
				return false;
			}
		}

		//------------------------------------------------------------------------------
		public string GetParamsOuterXml(HklAction action, string hotLinkFilter)
		{
			XmlDocument d = new XmlDocument();
			d.AppendChild(d.CreateElement(WebMethodsXML.Element.Arguments));
            FunctionPrototype fi = new FunctionPrototype();

			for (int i = 0; i < ActualParams.Count; i++)
			{
				Expression expr = ActualParams[i] as Expression;
				if (expr == null)
					return string.Empty;

				Expression actualParam = (Expression)ActualParams[i];

				Parameter prototypeParam = Prototype.Parameters[i];

                Parameter pInfo = new Parameter(prototypeParam.Name, prototypeParam.Type, prototypeParam.Mode);

				if (expr != null)
				{
					Value v = expr.Eval();
					pInfo.ValueString = SoapTypes.To(v.Data);
				}
			
				fi.Parameters.Add(pInfo);
			}

			//fixed parameter for hotlink (selection type, code, description)
			
			Parameter prototypeParamHklSelection = Prototype.Parameters[0];
			Parameter pInfoHklSelection = new Parameter(prototypeParamHklSelection.Name, prototypeParamHklSelection.Type, prototypeParamHklSelection.Mode);
			pInfoHklSelection.ValueString = SoapTypes.To(action == HklAction.Code ? 0 : 1);
			fi.Parameters.Add(pInfoHklSelection);
			
			int index = 2; //array position of "Description" parameter
			if (action == HklAction.Code)
				index = 1; //array position of "Code" parameter

			Parameter prototypeParamHklFilter = Prototype.Parameters[index];
			Parameter pInfoHklFilter = new Parameter(prototypeParamHklFilter.Name, prototypeParamHklFilter.Type, prototypeParamHklFilter.Mode);
			pInfoHklFilter.ValueString = SoapTypes.To(hotLinkFilter);
			fi.Parameters.Add(pInfoHklFilter);

            fi.Parameters.Unparse(d.DocumentElement);
			return d.OuterXml;
			//		<Arguments>
			//		<Param name="w_IsJournal" type="Bool" value="0" />
			//		</Arguments>
		}
	}
}