using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Implementations
{
    public class MemoryXSDResolver : IXSDResolver
    {
        private Dictionary<string, object> Cache { get; set; }

        public MemoryXSDResolver() 
        {
            Cache = new Dictionary<string, object>();
        }

        public bool TryPut(IBusinessObjectInfo bOInfo, object contenct, out string error) 
        {
            try
            {
                if (!Cache.ContainsKey(bOInfo.Name))
                    Cache[bOInfo.Name] = contenct;
                else
                {
                    error = string.Format("Overwriting contect for BO {0}", bOInfo.Name);
                    return false;
                }

                error = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public IBusinessObjectXSDInfo Resolve(IBusinessObjectInfo bOInfo)
        {

            IBusinessObjectXSDInfo result = Factory.Instance.CreateBusinessObjectXSDInfo();
            try
            {
                if (Cache.ContainsKey(bOInfo.Name))
                {
                    result.Found = true;
                    result.XsdContenct = Cache[bOInfo.Name].ToString();
                }

            }
            catch (Exception e)
            {

                Console.WriteLine("ERROR XSD Resolve" + e);
                result.Found = false;
            }

            return result;
        }
    }
}
