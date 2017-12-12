using System;
using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.Interfaces
{
    //=========================================================================
    public interface IAdvertisement
    {
        IAdvertisementBody Body { get; set; }
        bool HideDisclaimer { get; set; }
        bool ExpireWithRestart { get; set; }
        DateTime CreationDate { get; }
        bool Expired { get; }
        DateTime ExpiryDate { get; set; }
        string ID { get; set; }
        int Severity { get; set; }
        MessageType Type { get; set; }
        
        /// <summary>
        /// stringa customizzabile per la quale è possibile riconoscere gli Advertisement ( utilizzabile come tipazione custom)
        /// </summary>
        string Tag { get; set; }
        List<String> Recipients { get;  }
        bool Historicize { get; set; }
        bool Immediate { get; set; }
        MessageSensation Sensation { get; set; }
        int AutoClosingTime { get; set; }
       
    }

    //=========================================================================
    [Flags]
    public enum MessageType
    {
        None = 0x0,
        Contract = 0x1,
        Advrtsm = 0x2,
        Updates = 0x4,
        PostaLite = 0x8,
        Default = 0x32,
        DataSynch = 0x64,

    }

    //=========================================================================
    public enum MessageSensation //come imagelist
    {
      Information, ResultGreen, Warning, Error,AccessDenied, Help
    }
}