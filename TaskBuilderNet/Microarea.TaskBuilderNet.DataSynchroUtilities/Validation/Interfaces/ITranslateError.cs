using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces
{
    
    public class TranslateResult
    {
        public bool IsOK { get; set; }

        public string ResultMessage { get; set; }
    }

    public interface ITranslate
    {
        TranslateResult Translate(string Action, string Field);
    }
}
