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
            internal static readonly string moduleNamespace = "moduleNamespace";
            internal static readonly string value = "value";
            internal static readonly string name = "name";
            internal static readonly string itemName = "itemName";
            internal static readonly string verbose = "verbose";

            internal static readonly string ObjectSuccessfullyCreated = "Successfully Created";
            internal static readonly string ObjectSuccessfullyDeleted = "Successfully Deleted";
        }

        Service<EnumsService> Service { get; set; }

        //---------------------------------------------------------------------
        public EnumsController(IServiceManager serviceManager)
            : 
            base(serviceManager)
        {
            Service = Services?.GetService<EnumsService>();
        }

        //-----------------------------------------------------------------------
        [Route("create")]
        public IActionResult Create(string parameters)
        {
            JObject jsonParams = JObject.Parse(parameters);
            string moduleNamespace = jsonParams[Strings.moduleNamespace]?.Value<string>();
            var value = jsonParams[Strings.value]?.Value<ushort>();
            string name = jsonParams[Strings.name]?.Value<string>();
            var verbose = jsonParams[Strings.verbose];

            ushort uValue = (ushort) (value == null ? 0 : value);
            bool success = Service.Obj.Create(new NameSpace(moduleNamespace), uValue, name);

            if (success && verbose != null)
                Service.Obj.Diagnostic.Add(DiagnosticType.Information, string.Concat(name, " ", Strings.ObjectSuccessfullyCreated));

            return Ok(Service.Obj.Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("delete")]
        public IActionResult Delete(string parameters)
        {
            JObject jsonParams = JObject.Parse(parameters);
            string moduleNamespace = jsonParams[Strings.moduleNamespace]?.Value<string>();
            var value = jsonParams[Strings.value]?.Value<ushort>();
            string name = jsonParams[Strings.name]?.Value<string>();
            string itemName = jsonParams[Strings.itemName]?.Value<string>();
            var verbose = jsonParams[Strings.verbose];

            ushort uValue = (ushort)(value == null ? 0 : value);
            bool success = Service.Obj.Delete(new NameSpace(moduleNamespace), name, itemName);

            if (success && verbose != null)
                Service.Obj.Diagnostic.Add(DiagnosticType.Information, string.Concat(name, " ", itemName, ": ", Strings.ObjectSuccessfullyDeleted));

            return Ok(Service.Obj.Diagnostic);
        }
    }
}
