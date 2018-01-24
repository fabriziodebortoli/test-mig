
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using TaskBuilderNetCore.Documents.Model;

namespace TaskBuilderNetCore.Documents.Controllers
{
    //====================================================================================    
    [Name("Logger"), Description("It manages logging operations on diagnostic messages")]
    public class Logger : Controller , ILogger
    {
        //-----------------------------------------------------------------------------------------------------
        public Logger()
        {
        }
        //-----------------------------------------------------------------------------------------------------
        public Logger(PathFinder pathFinder)
        {
        }
    }
}
