using Microarea.DataService.Managers;
using Microarea.DataService.Managers.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Microarea.DataService
{
    public static class Extensions
    {
        public static IServiceCollection AddDependencies(this IServiceCollection _this)
        {
            _this.AddSingleton<IParameterManafer, ParameterManager>();

            return _this;
        }
    }
}
