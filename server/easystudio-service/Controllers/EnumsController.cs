using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.EasyStudio.Services;
using Microarea.Common.Generic;

namespace Microarea.EasyStudio.Controllers
{
    //=========================================================================
    [Route("easystudio/enums")]
    public class EnumsController : BaseController
    {
        //=========================================================================
        internal class Strings
        {
            // prameters
            internal static readonly string moduleNamespace = "moduleNamespace";
            internal static readonly string value = "value";
            internal static readonly string name = "name";
            internal static readonly string description = "description";
            internal static readonly string itemName = "itemName";
            internal static readonly string hidden = "hidden";
            internal static readonly string defaultValue = "defaultValue";
        }

        //---------------------------------------------------------------------
        Service<EnumsService> Service { get; set; }
        EnumsService EnumsService { get => Service.Obj; }
        public override IDiagnosticProvider Diagnostic => EnumsService.Diagnostic;

        //---------------------------------------------------------------------
        public EnumsController(IServiceManager serviceManager)
            : 
            base(serviceManager)
        {
            Service = Services?.GetService<EnumsService>();
        }

        //-----------------------------------------------------------------------
        [Route("createTag"), HttpGet]
        public IActionResult CreateTag(string moduleNamespace, ushort uValue, string name, string description = "", bool hidden = false)
        {
            // le get le teniamo verbose
            if (EnumsService.CreateTag(new NameSpace(moduleNamespace), uValue, name))
                Diagnostic.Add(DiagnosticType.Information, string.Concat(name, " ", BaseStrings.ObjectSuccessfullyCreated));

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("createTag"), HttpPost]
        public IActionResult CreateTag([FromBody] JObject jsonParams)
        {
            string moduleNamespace = jsonParams[Strings.moduleNamespace]?.Value<string>();
            var value = jsonParams[Strings.value]?.Value<ushort>();
            string name = jsonParams[Strings.name]?.Value<string>();
            ushort uValue = (ushort) (value == null ? 0 : value);
            string description = jsonParams[Strings.description]?.Value<string>();
            bool? hidden = jsonParams[Strings.hidden]?.Value<bool>();

            EnumsService.CreateTag(new NameSpace(moduleNamespace), uValue, name, description, hidden ?? false);

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("createItem"), HttpGet]
        public IActionResult CreateItem(string moduleNamespace, string tagName, ushort uValue, string name, string description = "", bool hidden = false)
        {
            // le get le teniamo verbose
            if (EnumsService.CreateItem(new NameSpace(moduleNamespace), tagName, uValue, name, description, hidden))
                Diagnostic.Add(DiagnosticType.Information, string.Concat(name, " ", BaseStrings.ObjectSuccessfullyCreated));

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("createItem"), HttpPost]
        public IActionResult CreateItem([FromBody] JObject jsonParams)
        {
            string moduleNamespace = jsonParams[Strings.moduleNamespace]?.Value<string>();
            string tagName = jsonParams[Strings.name]?.Value<string>();
            string itemName = jsonParams[Strings.itemName]?.Value<string>();
            var value = jsonParams[Strings.value]?.Value<ushort>();
            ushort uValue = (ushort)(value == null ? 0 : value);
            string description = jsonParams[Strings.description]?.Value<string>();
            bool? hidden = jsonParams[Strings.hidden]?.Value<bool>();

            EnumsService.CreateItem(new NameSpace(moduleNamespace), tagName, uValue, itemName, description, hidden ?? false);

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("delete"), HttpGet]
        public IActionResult Delete(string moduleNamespace, string name, string itemName)
        {
            // le get le teniamo verbose
            if (EnumsService.Delete(new NameSpace(moduleNamespace), name, itemName))
                Diagnostic.Add(DiagnosticType.Information, string.Concat(name, " ", itemName, ": ", BaseStrings.ObjectSuccessfullyDeleted));

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("delete"), HttpDelete]
        public IActionResult Delete([FromBody] JObject jsonParams)
        {
            string moduleNamespace = jsonParams[Strings.moduleNamespace]?.Value<string>();
            string name = jsonParams[Strings.name]?.Value<string>();
            string itemName = jsonParams[Strings.itemName]?.Value<string>();

            EnumsService.Delete(new NameSpace(moduleNamespace), name, itemName);

            return ToResult(Diagnostic);
        }

        //----------------------------------------------------------------------------------
        [Route("changeTagDefaultValue"), HttpGet]
        public IActionResult ChangeTagDefaultValue(string moduleNamespace, string name, ushort defaultValue)
        {
            if (EnumsService.ChangeTagDefaultValue(new NameSpace(moduleNamespace), name, defaultValue))
                Diagnostic.Add(DiagnosticType.Information, string.Concat(name, " ", BaseStrings.TagDefaultValueSuccessfullyChanged));

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("changeTagDefaultValue"), HttpPost]
        public IActionResult ChangeTagDefaultValue([FromBody] JObject jsonParams)
        {
            string moduleNamespace = jsonParams[Strings.moduleNamespace]?.Value<string>();
            string name = jsonParams[Strings.name]?.Value<string>();
            var val = jsonParams[Strings.defaultValue]?.Value<ushort>();
            ushort defaultValue = (ushort)(val == null ? 0 : val);
            EnumsService.ChangeTagDefaultValue(new NameSpace(moduleNamespace), name, defaultValue);

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------------------------
        [Route("generateSourceCode"), HttpGet]
        public IActionResult GenerateSourceCode(string moduleNamespace)
        {
            if (EnumsService.GenerateSourceCode(new NameSpace(moduleNamespace)))
                Diagnostic.Add(DiagnosticType.Information, string.Concat(moduleNamespace, " ", BaseStrings.EnumsGenerateSourceCodeSuccessfullyTerminated));

            return ToResult(Diagnostic);
        }

        //--------------------------------------------------------------------------------------------
        [Route("generateSourceCode"), HttpPost]
        public IActionResult GenerateSourceCode([FromBody] JObject jsonParams)
        {
            string moduleNamespace = jsonParams[Strings.moduleNamespace]?.Value<string>();
            string name = jsonParams[Strings.name]?.Value<string>();
            EnumsService.GenerateSourceCode(new NameSpace(moduleNamespace));

            return ToResult(Diagnostic);
        }
    }
}
