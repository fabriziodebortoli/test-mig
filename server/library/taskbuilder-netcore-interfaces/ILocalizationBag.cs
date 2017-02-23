using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Interfaces
{
    //=========================================================================
    public interface ILocalizationBag
    {
        string Days { get; set; }
        string Key { get; set; }
        string ProductName { get; set; }
        long RenewalPeriodTicks { get; set; }
        string UserEmail { get; set; }
    }
}
