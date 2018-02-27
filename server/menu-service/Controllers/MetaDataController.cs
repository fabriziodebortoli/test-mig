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

        [Route("InsertDatainDB/{istance}/{dbconnection}/{serverName}/{installation}")]
        //-----------------------------------------------------------------------
        public void InsertDatainDB(string istance, string dbconnection, string serverName, string installation)
        {
            string a = istance;
            MetaDataManagerTool metadata = new MetaDataManagerTool(istance, dbconnection, serverName, installation);
            metadata.InsertAllStandardMetaDataInDB();

        }

        [Route("DeleteDatainDB")]
        //-----------------------------------------------------------------------
        public void DeleteDatainDB(string istance, string dbconnection, string serverName, string installation)
        {
            MetaDataManagerTool metadata = new MetaDataManagerTool(istance, dbconnection, serverName, installation);
            metadata.DeleteAllStandardMetaDataInFS();

        }
        [Route("DeleteDatainDBByIstance")]
        //-----------------------------------------------------------------------
        public void DeleteDatainDBByIstance(string istance, string dbconnection, string serverName, string installation)
        {
            MetaDataManagerTool metadata = new MetaDataManagerTool(istance, dbconnection, serverName, installation);
            metadata.DeleteAllStandardMetaDataInDBByInstance(istance);

        }
    }
}
