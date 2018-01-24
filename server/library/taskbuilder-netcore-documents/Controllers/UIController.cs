using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using TaskBuilderNetCore.Documents.Model;

namespace TaskBuilderNetCore.Documents.Controllers
{
    [Name("UIController"), Description("It manages user interface code.")]
    //====================================================================================    
    public class UIController : Controller,  IUIController
    {
    }
}
