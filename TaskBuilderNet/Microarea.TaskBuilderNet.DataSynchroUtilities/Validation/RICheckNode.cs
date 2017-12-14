using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation
{
    //--------------------------------------------------------------------------------
    public class RICheckerInfo : IRICheckerInfo
    {
        public RICheckerInfo(string className, string query)
        {
            CheckerClass = className;
            MassiveQuery = query;
        }

        public string CheckerClass { get; private set; }
        public string MassiveQuery{ get; private set; }
    }

    //--------------------------------------------------------------------------------
    public class RICheckNode : IRICheckNode
    {
        public RICheckNode(string name)
        {
            Name = name;
            Sons = new List<IRICheckNode>();
            RICheckerInfoList = new List<IRICheckerInfo>();            
        }

        public IRICheckNode Father { get; set; }
        public string Name { get; }

        public List<IRICheckerInfo> RICheckerInfoList { get; set; }
        public List<IRICheckNode> Sons { get; set;}
    }

    //--------------------------------------------------------------------------------
    public class ValidationErrorNode : IValidationError
    {
        public ValidationErrorNode(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public string Type { get; } // Attualmente può essere XSD o FK a seconda del tipo di validazione che ha rilevato l'errore
        public string MessageError { get; set; }
    }    

    //--------------------------------------------------------------------------------
    public class ValidationReportNode : IValidationReport
    {
        public ValidationReportNode(string name)
        {
            Name = name;
            Errors = new List<IValidationError>();
        }
        public string Name { get; }
        public List<IValidationError> Errors { get; set; }

        public string TreeToXml()
        {
            string xmlMessageError = "<? xml version = \"1.0\" ?>";

            if (Errors.Count() == 0)
                xmlMessageError = $"<{Name}/>";
            else
            {
                xmlMessageError = $"<{Name}>";
                xmlMessageError += "<Errors>";

                foreach (var elem in Errors)
                {
                    xmlMessageError += $"<{elem.Name} Type=\"{elem.Type}\">";
                    xmlMessageError += $"{elem.MessageError}";
                    xmlMessageError += $"</{elem.Name}>";
                }

                xmlMessageError += "</Errors>";
                xmlMessageError += $"</{Name}>";
            }
            
            return xmlMessageError;
        }
    }
}
