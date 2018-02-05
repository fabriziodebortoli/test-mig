using System;
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
            metadata.InsertAllStandardMetaDataInDB();

        }

        [Route("DeleteDatainDB")]
        //-----------------------------------------------------------------------
        public void DeleteDatainDB(string istance)
        {
            MetaDataManagerTool metadata = new MetaDataManagerTool(istance);
            metadata.DeleteAllStandardMetaDataInFS();

        }
        [Route("DeleteDatainDBByIstance")]
        //-----------------------------------------------------------------------
        public void DeleteDatainDBByIstance(string istance)
        {
            MetaDataManagerTool metadata = new MetaDataManagerTool(istance);
            metadata.DeleteAllStandardMetaDataInDBByInstance(istance);

        }
    }
}
