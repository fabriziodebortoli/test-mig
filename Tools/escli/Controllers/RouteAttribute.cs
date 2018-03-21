
using TaskBuilderNetCore.Common.CustomAttributes;

namespace escli.Controllers
{
    //====================================================================
    public class RouteAttribute : NameAttribute
    {
        //---------------------------------------------------------------
        public RouteAttribute(string routing)
            : base(routing)
        {
        }
    }
}
