using System.ComponentModel;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
    //=========================================================================
    public interface IEventNameGenerator
    {
        string GenerateEventName(IComponent component);
    }
}
