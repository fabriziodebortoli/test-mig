using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Implementations
{
    public class XSDResolverWithCache : IXSDResolver
    {
        private IXSDResolver MemoryXSDResolver { get; set; }
        private IXSDResolver RemoteXSDResolver { get; set; }

        public XSDResolverWithCache() 
        {
        }

        public XSDResolverWithCache(IBusinessObjectInfo memoryBOInfo, IBusinessObjectInfo remoteBOInfo)
        {
            MemoryXSDResolver = Factory.Instance.CreateXSDResolver(memoryBOInfo);
            RemoteXSDResolver = Factory.Instance.CreateXSDResolver(remoteBOInfo);
        }

        public IBusinessObjectXSDInfo Resolve(IBusinessObjectInfo bOInfo)
        {
            IBusinessObjectXSDInfo result = MemoryXSDResolver.Resolve(bOInfo);

            if (!result.Found)
            {
                result = RemoteXSDResolver.Resolve(bOInfo);

                if (result.Found)
                {
                    string error = string.Empty;
                    if (!((MemoryXSDResolver)MemoryXSDResolver).TryPut(bOInfo, result.XsdContenct, out error))
                    {
                        result.XsdContenct = string.Empty;
                        result.Found = false;
                    }
                }
            }



            return result;
        }
    }
}
