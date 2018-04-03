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

        [Route("InsertDatainDB/{dbconnection}/{serverName}/{installation}")]
        //-----------------------------------------------------------------------
        public void InsertDatainDB(string dbconnection, string serverName, string installation)
        {
            MetaDataManagerTool metadata = new MetaDataManagerTool(dbconnection, serverName, installation);
            metadata.InsertAllStandardMetaDataInDB();

        }

        [Route("DeleteDatainDB")]
        //-----------------------------------------------------------------------
        public void DeleteDatainDB(string dbconnection, string serverName, string installation)
        {
            MetaDataManagerTool metadata = new MetaDataManagerTool(dbconnection, serverName, installation);
            metadata.DeleteAllStandardMetaDataInFS();

        }
     }
}
