﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microarea.Common;

namespace Microarea.Menu.Controllers
{
    [Route("MetaDataController")]
    //=========================================================================
    public class MetaDataController
    {

        [Route("InsertDatainDB")]
        //-----------------------------------------------------------------------
        public void InsertDatainDB(string istance)
        {
            MetaDataManagerTool metadata = new MetaDataManagerTool(istance);
            metadata.InsertAllMetaDataInDB();

        }

        [Route("DeleteDatainDBByIstance")]
        //-----------------------------------------------------------------------
        public void DeleteDatainDBByIstance(string istance)
        {
            MetaDataManagerTool metadata = new MetaDataManagerTool(istance);
           // metadata.InsertAllMetaDataInDB();

        }
    }
}
