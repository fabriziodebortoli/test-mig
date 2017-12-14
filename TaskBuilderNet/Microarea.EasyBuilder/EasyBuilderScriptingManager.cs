using Microarea.EasyBuilder.MVC;
using Microarea.EasyBuilder.Scripting;
using Microarea.TaskBuilderNet.Core.CoreTypes;

namespace Microarea.EasyBuilder
{
    /// <summary>
    /// Manages Tb scripting in EasyBuilder controller
    /// </summary>
    public class EasyBuilderScriptingManager : TBScriptManager
    {
        /// <summary>
        /// Construct EasyBuilderScriptingManager
        /// </summary>
        public EasyBuilderScriptingManager(DocumentController controller)
            :
            base(controller.Document)
        {
            this.ScriptingSymbolTable.Add(new Variable("Controller", controller));	//radice
            this.ScriptingSymbolTable.Add(new Variable("View", controller.View));
        }
    }
}
