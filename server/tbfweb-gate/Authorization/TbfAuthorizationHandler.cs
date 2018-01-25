using Microarea.Common;
using Microarea.Common.Applications;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.TbfWebGate.Authorization
{
    public class TbfAuthorizationHandler : AuthorizationHandler<TbfAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TbfAuthorizationRequirement requirement)
        {
            var res = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;
            if (res == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var httpContext = res.HttpContext;
            var ui = requirement.GetLoginInformation(httpContext.Request, null);

            if (ui != null)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
