using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Interfaces
{
    //=========================================================================
    public interface IAdvertisementBody
    {
        string Html { get; set; }
        string Link { get; set; }
        ILocalizationBag LocalizationBag { get; set; }
        string Text { get; set; }
    }
}
