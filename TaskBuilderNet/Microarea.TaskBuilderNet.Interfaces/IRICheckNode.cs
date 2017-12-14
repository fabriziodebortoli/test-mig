using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.Interfaces
{
    //--------------------------------------------------------------------------------
    public interface IRICheckerInfo
    {
        string CheckerClass { get; }
        string MassiveQuery { get; }
    }

    ///<summary>
    /// Struttura utilizzata per la creazione in memoria de tree a partire dalla stringa dell'xml serializzato
    ///</summary>
    /////--------------------------------------------------------------------------------
    public interface IRICheckNode
    {
        IRICheckNode Father { get; set; }
        string Name { get; }
        List<IRICheckerInfo> RICheckerInfoList { get; set; }
        List<IRICheckNode> Sons { get; set; }
    }

    //--------------------------------------------------------------------------------
    public interface IValidationError
    {
        string Name { get; }
        string Type { get; } // Attualmente può essere XSD o FK a seconda del tipo di validazione che ha rilevato l'errore
        string MessageError { get; }
    }

    ///<summary>
    /// Struttura utilizzata per la creazione della stringa xml che viene salvata sulla DS_ValidationInfo
    ///</summary>
    /////--------------------------------------------------------------------------------
    public interface IValidationReport
    {
        string Name { get; }
        List <IValidationError> Errors { get; set; }
        string TreeToXml();
    }
}
